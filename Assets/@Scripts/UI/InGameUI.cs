using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    public Transform remotePlayersConatiner;

    [Header("CardUI")]
    public Transform[] fieldCardTransforms;
    public Dictionary<int, Transform[]> FieldCardLevelContainer = new();

    public Transform[] dummyCardObjects;
    public Dictionary<int, Transform> DummyCardLevelContainer = new();
    
    public Transform[] specialCardsContainer;
    [Header("CoinUI")]
    public Transform[] coinsTransforms;
    public Dictionary<int, Transform> CoinsContainer = new();
    

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
    }

    public void DetectCoinChanges(int[] coinChanges)
    {

    }
}
