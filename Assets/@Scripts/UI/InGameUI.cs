using System;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Components;
using TMPro;
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
    public Transform[] fieldCardTransforms;
    
    public Dictionary<(int, Transform), CardElement> fieldCardElements = new();
    
    public Transform[] dummyCardObjects;
    public Dictionary<int, Transform> DummyCardLevelContainer = new();
    
    public Transform[] specialCardsContainer;
    [Header("CoinUI")]
    public CoinElement[] coinElements;
    
    #endregion

    #region Prefabs

    public CoinElement coinPrefab;
    public CardElement cardPrefab;
    public SpecialCardElement specialCardPrefab;

    #endregion

    #region ObjectPools

    public ObjectPool<CoinElement> AnimationCoinPool;
    public ObjectPool<CardElement> CardPool;
    public ObjectPool<SpecialCardElement> SpecialPool;

    #endregion
    
    public ReservedCardDetailUI reservedCardDetailUI;
    public MenuUI menuUI;
    public ResultUI resultUI;
    
    [Header("MenuSide")]
    public TMP_Text victoryPointsText;
    public UIButton menuButton;

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
        }

        var fieldSpecialCards = GameSystem.Instance.CardSystem.FieldSpecialCards;

        for (int i = 0; i < fieldSpecialCards.Count; i++)
        {
            var SpecialCardElement = Instantiate(specialCardPrefab, specialCardsContainer[i]);
            SpecialCardElement.GetComponent<SpecialCardElement>().InitializeCard(
                cardData.GetSpecialCardInfoById(fieldSpecialCards[i]));
        }
        
        GameSystem.Instance.OnCoinChanged += UpdateCoinTexts;
    }
    
    private void InitializeObjectPools()
    {
        AnimationCoinPool = new ObjectPool<CoinElement>(
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
            defaultCapacity: 40,
            maxSize: 40
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


    public void DetectCoinChanges(int[] coinChanges,Transform startTransform)
    {
        
    }

    private void CoinAnimationStart(int coinType, Transform startTransform)
    {
        
    }

    public void UpdateCoinTexts(int[] coins)
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
}
