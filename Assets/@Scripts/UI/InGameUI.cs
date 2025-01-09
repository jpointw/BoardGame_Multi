using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    public Transform remotePlayersConatiner;

    public Transform[] fieldCardTransforms;
    public Dictionary<int, Transform[]> FieldCardLevelContainer = new();

    public Transform[] dummyCardObjects;
    public Dictionary<int, Transform> DummyCardLevelContainer = new();
    
    public Transform[] coinsTransforms;
    public Dictionary<int, Transform> CoinsContainer = new();
    
    public Transform[] specialCardsContainer;

    public Transform localSelectedCoinHolder;
    public GameObject[] localSelectedCards;
    
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
}
