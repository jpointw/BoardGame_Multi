using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{

    #region Transform Holders

    public Transform remotePlayersHolder;

    [Header("CardUI")]
    public Transform[] fieldCardTransforms;
    public Dictionary<int, Transform[]> FieldCardLevelContainer = new();

    public Transform[] dummyCardObjects;
    public Dictionary<int, Transform> DummyCardLevelContainer = new();
    
    public Transform[] specialCardsContainer;
    [Header("CoinUI")]
    public Transform[] coinsTransforms;
    public Dictionary<int, Transform> CoinsContainer = new();
    
    #endregion

    #region Prefabs

    public CoinElement coinPrefab;
    public CardElement cardPrefab;
    public SpecialCardElement specialCardPrefab;

    #endregion

    #region ObjectPools

    public ObjectPool<CoinElement> CoinPool;
    public ObjectPool<CardElement> CardPool;
    public ObjectPool<SpecialCardElement> SpecialPool;

    #endregion
    
    public ReservedCardDetailUI reservedCardDetailUI;

    [Header("LocalSelectedAsset")]
    public Transform localSelectedCoinHolder;
    public GameObject[] localSelectedCards;
    
    [Header("MenuSide")]
    public TMP_Text victoryPointsText;

    private void Awake()
    {
        InitializeTransformContainers();

        InitializeObjectPools();
    }

    public void InitializeUI()
    {
        victoryPointsText.text = GameSystem.Instance.VictoryPoint.ToString();
        
        GameSystem.Instance.OnCoinChanged += DetectCoinChanges;
        GameSystem.Instance.OnCoinChanged += UpdateCoinTexts;
    }

    private void InitializeTransformContainers()
    {
        for (int i = 0; i < 3; i++)
        {
            Transform[] tempTransforms = {
                fieldCardTransforms[(i * 4) + i],
                fieldCardTransforms[(i * 4) + i + 1],
                fieldCardTransforms[(i * 4) + i + 2]
            };
            FieldCardLevelContainer.TryAdd(i, tempTransforms);
        }

        for (int i = 0; i < 3; i++)
        {
            DummyCardLevelContainer.TryAdd(i, dummyCardObjects[i]);
        }

        for (int i = 0; i < 6; i++)
        {
            CoinsContainer.TryAdd(i, coinsTransforms[i]);
        }
    }
    
    private void InitializeObjectPools()
    {
        CoinPool = new ObjectPool<CoinElement>(
            createFunc: () => Instantiate(coinPrefab),
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: Destroy,
            defaultCapacity: 40,
            maxSize: 40
        );

        CardPool = new ObjectPool<CardElement>(
            createFunc: () => Instantiate(cardPrefab),
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: Destroy,
            defaultCapacity: 20,
            maxSize: 90
        );

        SpecialPool = new ObjectPool<SpecialCardElement>(
            createFunc: () => Instantiate(specialCardPrefab),
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: Destroy,
            defaultCapacity: 5,
            maxSize: 5
        );
    }

    private void OnServerInitialized()
    {
        throw new NotImplementedException();
    }

    public void DetectCoinChanges(int[] coinChanges)
    {
        for (int i = 0; i < coinChanges.Length; i++)
        {
            var coinImages = CoinsContainer[i].GetComponentsInChildren<Image>();

            for (int j = 0; j < coinImages.Length; j++)
            {
                coinImages[j].enabled = j < coinChanges[i];
            }
        }
    }

    public void UpdateCoinTexts(int[] coins)
    {
        for (int i = 0; i < coins.Length; i++)
        {
            CoinsContainer[i].GetComponentInChildren<TMP_Text>().text = coins[i].ToString();
        }
    }

    public void ShowReservedCardsDetail()
    {
        reservedCardDetailUI.Open();
    }
}
