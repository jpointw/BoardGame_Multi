using System;
using DG.Tweening;
using UnityEngine;
using TMPro;
using Doozy.Runtime.UIManager.Components;
using UnityEngine.UI;

public class TitleScene : MonoBehaviour
{
    [Header("UI Buttons")]
    public UIButton[] titleButtons;

    [Header("Fill Amount Images")]
    public Image[] fillAmountImages;

    [Header("UI Texts")]
    public TMP_Text[] titleTexts;
    
    [Header("Game Start Buttons")]
    public UIButton createRoomButton;
    public UIButton findRoomButton;

    public TMP_InputField InputField;
    
    private float animationDuration = 0.5f;

    private int[] playerAmounts = {2, 3, 4};
    private int[] victoryPoints = {12, 15, 18};
    private int[] turnTimers = {30, 60, 90};
    
    public UIDataBase UIDataBase;
    public CardModelData CardModelData;

    private void Start()
    {
        GameSharedData.PlayerCount = 2;
        GameSharedData.GameVictoryPoints = 12;
        GameSharedData.PlayerTurnTime = 30;

        UpdateUI(0, GameSharedData.PlayerCount, 2,4);
        UpdateUI(1, GameSharedData.GameVictoryPoints,12, 18);
        UpdateUI(2, GameSharedData.PlayerTurnTime,30, 90);

        titleButtons[0].onClickEvent.AddListener(ChangePlayerAmount);
        titleButtons[1].onClickEvent.AddListener(ChangeVictoryPoints);
        titleButtons[2].onClickEvent.AddListener(ChangeTurnTimer);
        
        createRoomButton.onClickEvent.AddListener(CreateRoom);
        findRoomButton.onClickEvent.AddListener(FindRoom);
    }

    private void ChangePlayerAmount()
    {
        int index = Array.IndexOf(playerAmounts, GameSharedData.PlayerCount);
        GameSharedData.PlayerCount = playerAmounts[(index + 1) % playerAmounts.Length];
        UpdateUI(0, GameSharedData.PlayerCount, 2, 4);
    }

    private void ChangeVictoryPoints()
    {
        int index = Array.IndexOf(victoryPoints, GameSharedData.GameVictoryPoints);
        GameSharedData.GameVictoryPoints = victoryPoints[(index + 1) % victoryPoints.Length];
        UpdateUI(1, GameSharedData.GameVictoryPoints, 12, 18);
    }

    private void ChangeTurnTimer()
    {
        int index = Array.IndexOf(turnTimers, GameSharedData.PlayerTurnTime);
        GameSharedData.PlayerTurnTime = turnTimers[(index + 1) % turnTimers.Length];
        UpdateUI(2, GameSharedData.PlayerTurnTime, 30, 90);
    }


    private void UpdateUI(int index, float currentValue, float minValue, float maxValue)
    {
        titleTexts[index].text = currentValue.ToString();

        float targetValue = (currentValue - minValue) / (maxValue - minValue);

        fillAmountImages[index].DOFillAmount(targetValue, animationDuration)
            .SetEase(Ease.InOutQuad);
    }

    private async void CreateRoom()
    {
        GameSharedData.MyNickname = InputField.text;
        NetworkSystem.Instance.CreateRoom();
    }

    private async void FindRoom()
    {
        GameSharedData.MyNickname = InputField.text;
        NetworkSystem.Instance.FindRoom();
    }
}
