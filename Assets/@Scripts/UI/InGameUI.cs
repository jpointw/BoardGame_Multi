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
    public ObjectPool<CoinElement> CardPool;
    public ObjectPool<CoinElement> SpecialPool;

    #endregion

    [Header("LocalSelectedAsset")]
    public Transform localSelectedCoinHolder;
    public GameObject[] localSelectedCards;
    
    [Header("MenuSide")]
    public TMP_Text victoryPointsText;
    
    public Button endTurnButton;

    private void Awake()
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

    public void InitializeUI()
    {
        endTurnButton.onClick.AddListener(GameSystem.Instance.TurnSystem.EndTurn);
        GameSystem.Instance.OnCoinChanged += DetectCoinChanges;
        victoryPointsText.text = GameSystem.Instance.VictoryPoint.ToString();
    }

    public void DetectCoinChanges(int[] coinChanges)
    {

    }
}
