using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Doozy.Runtime.UIManager.Components;
using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{

    #region Transform Holders

    //리모트 유저들 UI
    public Transform remotePlayersHolder; 
    //로컬 유저 UI
    public Transform localPlayerHolder;

    [Header("CardUI")] 
    public UIToggleGroup FieldCardToggleGroup;
    public Transform[] fieldCardTransforms;
    public List<CardElement> cardElements = new List<CardElement>();
    // public Dictionary<(int, Transform), CardElement> fieldCardElements = new();
    
    public Transform[] dummyCardObjects;
    public Dictionary<int, Transform> DummyCardLevelContainer = new();
    
    public Transform[] specialCardsContainer;
    public List<SpecialCardElement> specialCardElements = new();
    [Header("CoinUI")]
    public CoinElement[] coinElements;
    
    #endregion

    #region Prefabs

    public Image coinPrefab;
    public CardElement cardPrefab;
    public SpecialCardElement specialCardPrefab;

    #endregion

    #region ObjectPools

    public ObjectPool<Image> AnimationCoinPool;
    public ObjectPool<CardElement> CardPool;
    public ObjectPool<SpecialCardElement> SpecialPool;

    #endregion
    
    public ReservedCardDetailUI reservedCardDetailUI;
    public MenuUI menuUI;
    public ResultUI resultUI;
    
    [Header("MenuSide")]
    public TMP_Text victoryPointsText;
    public UIButton menuButton;
    
    public Queue<Action> CardActionQueue = new();
    public bool isProcessingCardQueue;
    public Queue<Action> CoinActionQueue = new();
    public bool isProcessingCoinQueue;

    private void Awake()
    {
        // InitializeTransformContainers();

        InitializeObjectPools();
    }

    public void InitializeUI()
    {
        victoryPointsText.text = GameSystem.Instance.VictoryPoint.ToString();
        

        var fieldCards = GameSystem.Instance.CardSystem.FieldCards;
        var cardData = CardModelData.Instance;
        
        for (int i = 0; i < fieldCards.Count; i++)
        {
            var cardElement = Instantiate(cardPrefab, fieldCardTransforms[i]);
            cardElement.GetComponent<CardElement>().InitializeCard(cardData.GetCardInfoById(fieldCards[i]));
            cardElements.Add(cardElement);
        }

        var fieldSpecialCards = GameSystem.Instance.CardSystem.FieldSpecialCards;

        for (int i = 0; i < fieldSpecialCards.Count; i++)
        {
            var specialCardElement = Instantiate(specialCardPrefab, specialCardsContainer[i]);
            specialCardElement.GetComponent<SpecialCardElement>().InitializeCard(
                cardData.GetSpecialCardInfoById(fieldSpecialCards[i]));
            specialCardElements.Add(specialCardElement);
        }

        GameSystem.Instance.CardSystem.OnCardAdded += ScheduleCardAddition;
        GameSystem.Instance.CardSystem.OnCardRemoved += ScheduleCardRemoval;
        GameSystem.Instance.OnCoinChanged += UpdateCoinTexts;
        GameSystem.Instance.OnCoinChanged += ScheduleCoinAnimation;
    }
    
    private void InitializeObjectPools()
    {
        AnimationCoinPool = new ObjectPool<Image>(
            createFunc: () => Instantiate(coinPrefab),
            actionOnGet: obj =>
            {
                obj.gameObject.SetActive(true);
            },
            actionOnRelease: obj =>
            {
                obj.gameObject.SetActive(false);
            },
            actionOnDestroy: Destroy,
            defaultCapacity: 10,
            maxSize: 10
        );

        CardPool = new ObjectPool<CardElement>(
            createFunc: () => Instantiate(cardPrefab),
            actionOnDestroy: Destroy,
            defaultCapacity: 20,
            maxSize: 90
        );
    }

    public void DetectCardChanges()
    {
        
    }
    public void UpdateCoinTexts(int[] coins, PlayerRef playerRef, bool isToCentral)
    {
        for (int i = 0; i < coins.Length; i++)
        {
            coinElements[i].GetComponentInChildren<TMP_Text>().text = coins[i].ToString();
        }
    }

    public void ShowReservedCardsDetail(int[] cardInfos)
    {
        reservedCardDetailUI.Open(cardInfos);
    }

    public void ShowMenuPopup()
    {
        menuUI.Open();
    }

    private void ScheduleCoinAnimation(int[] coins, PlayerRef playerRef, bool isToCentral)
    {
        CoinActionQueue.Enqueue(() => OnCoinMoved(coins, playerRef, isToCentral));
        ProcessCardQueue();
    }
    
    private void ScheduleCardRemoval(int cardId, int slotIndex)
    {
        CardActionQueue.Enqueue(() => OnCardRemoved(cardId, slotIndex));
        ProcessCardQueue();
    }

    private void ScheduleCardAddition(int cardId, int slotIndex)
    {
        CardActionQueue.Enqueue(() => OnCardAdded(cardId, slotIndex));
        ProcessCardQueue();
    }

    private void ProcessCardQueue()
    {
        if (isProcessingCardQueue || CardActionQueue.Count == 0) return;

        isProcessingCardQueue = true;
        ExecuteCardQueue().Forget();
    }

    private async UniTaskVoid ExecuteCardQueue()
    {
        while (CardActionQueue.Count > 0)
        {
            var action = CardActionQueue.Dequeue();
            action?.Invoke();

            await UniTask.WaitUntil(() => !isProcessingCardQueue);
        }
    }

    private async UniTaskVoid ExecuteCoinQueue()
    {
        while (CoinActionQueue.Count > 0)
        {
            var action = CoinActionQueue.Dequeue();
            action?.Invoke();

            await UniTask.WaitUntil(() => !isProcessingCoinQueue);
        }
    }

    private async void OnCoinMoved(int[] coins, PlayerRef playerRef, bool isToCentral)
    {
        if (playerRef == PlayerRef.Invalid) return;

        var playerUI = GameSystem.Instance.Players.FirstOrDefault(p => p.PlayerRef == playerRef);
        if (playerUI == null) return;

        for (int i = 0; i < coins.Length; i++)
        {
            if (coins[i] > 0)
            {
                for (int j = 0; j < coins[i]; j++)
                {
                    Transform startTransform, endTransform;

                    if (isToCentral)
                    {
                        startTransform = playerUI.transform;
                        endTransform = coinElements[i].transform;
                    }
                    else
                    {
                        startTransform = coinElements[i].transform;
                        endTransform = playerUI.transform;
                    }

                    await AnimateCoinMovement(startTransform, endTransform);
                }
            }
        }

        isProcessingCoinQueue = false;
    }
    
    private async UniTask AnimateCoinMovement(Transform start, Transform end)
    {
        var coin = AnimationCoinPool.Get();
        coin.transform.position = start.position;
        coin.gameObject.SetActive(true);

        await coin.transform
            .DOMove(end.position, 0.5f)
            .SetEase(Ease.InOutQuad)
            .ToUniTask();

        AnimationCoinPool.Release(coin);
    }

    private async void OnCardRemoved(int cardId, int slotIndex)
    {
        Transform slot = fieldCardTransforms[slotIndex];

        CardElement cardElement = slot.GetComponentInChildren<CardElement>();

        if (cardElement != null)
        {
            await cardElement.transform
                .DOScale(Vector3.zero, 0.5f)
                .SetEase(Ease.InBack)
                .ToUniTask();

            Destroy(cardElement.gameObject);
            cardElements[slotIndex] = null;
        }

        isProcessingCardQueue = false;
    }

    private async void OnCardAdded(int cardId, int slotIndex)
    {
        Transform slot = fieldCardTransforms[slotIndex];

        var newCard = Instantiate(cardPrefab, slot);
        newCard.InitializeCard(CardModelData.Instance.GetCardInfoById(cardId));

        newCard.transform.localScale = Vector3.zero;
        await newCard.transform
            .DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .ToUniTask();

        cardElements[slotIndex] = newCard;
        isProcessingCardQueue = false;
    }

}
