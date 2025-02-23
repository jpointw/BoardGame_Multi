using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Doozy.Runtime.UIManager.Components;
using Fusion;
using ParrelSync;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

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
    // public List<CardElement> cardElements = new List<CardElement>();
    public Dictionary<int,CardElement> cardElementDictionary = new Dictionary<int,CardElement>();
    // public Dictionary<(int, Transform), CardElement> fieldCardElements = new();
    
    public Transform[] dummyCardObjects;
    public Dictionary<int, Transform> DummyCardLevelContainer = new();
    
    public Transform[] specialCardsContainer;
    public List<SpecialCardElement> specialCardElements = new();
    [Header("CoinUI")]
    public CoinElement[] coinElements;
    public TMP_Text[] coinTexts;
    
    #endregion

    #region Prefabs

    public LocalBoardPlayer localBoardPlayerPrefab;
    public RemoteBoardPlayer remoteBoardPlayerPrefab;

    private List<BasePlayer2> _playerUIs = new();
    
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

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            InitializeUI();
        }
    }

    public void InitializeUI()
    {
        CreatePlayerUI();
        
        victoryPointsText.text = GameSystem.Instance.VictoryPoint.ToString();
        

        var fieldCards = GameSystem.Instance.CardSystem.FieldCards;
        var cardData = CardModelData.Instance;
        
        // for (int i = 0; i < fieldCards.Count; i++)
        // {
        //     CardElement cardElement = Instantiate(cardPrefab, fieldCardTransforms[i]);
        //     cardElement.GetComponent<CardElement>().InitializeCard(cardData.GetCardInfoById(fieldCards[i]),fieldCardTransforms[i]);
        //     cardElementDictionary.TryAdd(cardElement.CardInfo.uniqueId, cardElement);
        // }

        for (int i = 0; i < fieldCards.Count; i++)
        {
            ScheduleCardAddition(fieldCards[i], i);
        }

        var fieldSpecialCards = GameSystem.Instance.CardSystem.FieldSpecialCards;

        for (int i = 0; i < fieldSpecialCards.Count; i++)
        {
            var specialCardElement = Instantiate(specialCardPrefab, specialCardsContainer[i]);
            specialCardElement.GetComponent<SpecialCardElement>().InitializeCard(
                cardData.GetSpecialCardInfoById(fieldSpecialCards[i]));
            specialCardElements.Add(specialCardElement);
        }

        for (int i = 0; i < coinElements.Length; i++)
        {
            coinElements[i].InitializeCoin();
        }

        GameSystem.Instance.CardSystem.OnCardAdded += ScheduleCardAddition;
        GameSystem.Instance.CardSystem.OnCardRemoved += ScheduleCardRemoval;
        GameSystem.Instance.CoinSystem.OnCoinChanged += UpdateCoinTexts;
        GameSystem.Instance.OnGameEnded += ShowResultUI;
        GameSystem.Instance.CardSystem.OnSpecialCardRemoved += OnSpecialCardRemoved;
        // GameSystem.Instance.OnCoinChanged += UpdateCoinTexts;
        // GameSystem.Instance.OnCoinChanged += ScheduleCoinAnimation;
    }
    private void CreatePlayerUI()
    {
        foreach (var player in GameSystem.Instance.Players)
        {
            if (player.Key == NetworkSystem.Instance.Runner.LocalPlayer)
            {
                GameSystem.Instance.RPC_ChangeName(player.Key, GameSharedData.MyNickname);
                CreatePlayerUI(localBoardPlayerPrefab, player.Value, localPlayerHolder);
            }
            else
            {
                CreatePlayerUI(remoteBoardPlayerPrefab, player.Value, remotePlayersHolder);
            }
        }
    }
    private void CreatePlayerUI(BasePlayer2 prefab, BasePlayerInfo playerInfo, Transform parent)
    {
        var playerUI = Instantiate(prefab, parent);
        _playerUIs.Add(playerUI);
        playerUI.Init(playerInfo);
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
    
    public void UpdateCoinTexts(int[] coins)
    {
        for (int i = 0; i < coins.Length; i++)
        {
            coinTexts[i].text = coins[i].ToString();
        }
    }

    public void UpdateTempCoinText(int coinType, int amount)
    {
        coinTexts[coinType].text = (GameSystem.Instance.CoinSystem.CentralCoins[coinType] - amount).ToString();

    }

    public void ShowReservedCardsDetail(int[] cardInfos)
    {
        reservedCardDetailUI.Open(cardInfos);
    }

    public void ShowResultUI(string winner)
    {
        resultUI.Open(winner);
    }

    public void ShowMenuPopup()
    {
        menuUI.Open();
    }

    private void ScheduleCoinAnimation(int[] coins, PlayerRef playerRef, bool isToCentral)
    {
        CoinActionQueue.Enqueue(() => OnCoinMoved(coins, playerRef, isToCentral));
        ProcessCoinQueue();
    }
    
    private void ScheduleCardRemoval(int cardId, int slotIndex)
    {
        CardActionQueue.Enqueue(() => OnCardRemoved(cardId, slotIndex));
        ProcessCardQueue();
    }

    private void ScheduleCardAddition(int cardId, int slotIndex)
    {
        Debug.LogError($"ScheduleCardAddition: CardId: {cardId}, SlotIndex: {slotIndex}");
        CardActionQueue.Enqueue(() => OnCardAdded(cardId, slotIndex));
        ProcessCardQueue();
    }

    private void ProcessCardQueue()
    {
        if (isProcessingCardQueue || CardActionQueue.Count == 0) return;

        isProcessingCardQueue = true;
        ExecuteCardQueue().Forget();
    }

    private void ProcessCoinQueue()
    {
        if (isProcessingCoinQueue || CardActionQueue.Count == 0) return;
        
        isProcessingCoinQueue = true;
        ExecuteCoinQueue().Forget();
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

        var playerUI = GameSystem.Instance.Players[playerRef];
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

        CardElement cardElement = cardElementDictionary[cardId];
        
        if (cardElement != null)
        {
            CanvasGroup canvasGroup = cardElement.GetComponent<CanvasGroup>();
            float heightOffset = cardElement.transform.localScale.y / 2;

            // 🔹 Sequence 생성
            Sequence sequence = DOTween.Sequence();

            sequence.Append(canvasGroup.DOFade(0, 0.5f))
                .Join(cardElement.transform.DOLocalMoveY(cardElement.transform.localPosition.y - heightOffset, 1f)
                    .SetEase(Ease.InOutQuad))
                .Append(cardElement.transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack))
                .OnComplete(() =>
                {
                    cardElementDictionary.Remove(cardId);
                    Destroy(cardElement.gameObject);
                });

            await sequence.ToUniTask();

        }

        isProcessingCardQueue = false;
    }

    private async void OnCardAdded(int cardId, int slotIndex)
    {
        Transform slot = fieldCardTransforms[slotIndex];
        CardElement newCard = Instantiate(cardPrefab, slot);

        newCard.InitializeCard(CardModelData.Instance.GetCardInfoById(cardId), slot);

        newCard.transform.localScale = Vector3.zero;
        await newCard.transform
            .DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .ToUniTask();

        cardElementDictionary.TryAdd(newCard.CardInfo.uniqueId,newCard);
        isProcessingCardQueue = false;
    }

    private void OnSpecialCardRemoved(int specialCardId)
    {

        SpecialCardElement specialCardElement =
            specialCardElements.Find(s => s.SpecialCardInfo.uniqueId == specialCardId);
        
        if (specialCardElement != null)
        {
            CanvasGroup canvasGroup = specialCardElement.GetComponent<CanvasGroup>();
            float heightOffset = specialCardElement.transform.localScale.y / 2;
            Sequence sequence = DOTween.Sequence();

            sequence.Append(canvasGroup.DOFade(0, 0.5f))
                .Join(specialCardElement.transform.DOLocalMoveY(specialCardElement.transform.localPosition.y - heightOffset, 1f)
                    .SetEase(Ease.InOutQuad))
                .Append(specialCardElement.transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack))
                .OnComplete(() =>
                {
                    specialCardElements.Remove(specialCardElement);
                    Destroy(specialCardElement.gameObject);
                });
        }
    }
}
