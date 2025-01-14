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

    [Header("LocalSelectedAsset")]
    public Transform localSelectedCoinHolder;
    public GameObject[] localSelectedCards;
    
    [Header("MenuSide")]
    public TMP_Text victoryPointsText;
    
    public Button endTurnButton;

    private void Awake()
    {
        InitializeTransformContainers();

        InitializeObjectPools();
    }

    public void InitializeUI()
    {
        endTurnButton.onClick.AddListener(GameSystem.Instance.TurnSystem.EndTurn);
        victoryPointsText.text = GameSystem.Instance.VictoryPoint.ToString();
        GameSystem.Instance.OnCoinChanged += DetectCoinChanges;
    }

    private void InitializeTransformContainers()
    {
        for (int i = 0; i < 3; i++)
        {
            Transform[] tempTransforms = {
                fieldCardTransforms[(i * 4) + i],
                fieldCardTransforms[(i * 4) + i + 1],
                fieldCardTransforms[(i * 4) + i + 2],
                fieldCardTransforms[(i * 4) + i + 3]
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
    
    public void DetectCoinChanges(int[] coinChanges)
    {
        // 자식들 찾아와서 현재 코인 수 만큼 키기
    }
}
