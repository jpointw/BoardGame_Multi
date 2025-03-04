using System;
using DG.Tweening;
using UnityEngine;
using TMPro;
using Doozy.Runtime.UIManager.Components;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TitleScene : MonoBehaviour
{
    public UIButton[] titleButtons;
    public Image[] fillAmountImages;
    public TMP_Text[] titleTexts;
    public UIButton createRoomPanelButton;
    public UIButton findRoomButton;
    
    public Canvas createRoomCanvas;
    public UIButton createRoomButton;
    public UIButton createRoomCanvasBackButton;
    
    public Canvas playerWaitingCanvas;
    public Image[] players;
    public UIButton playerWaitingBackButton;
    
    public Canvas findWaitingCanvas;
    public UIButton findWaitingBackButton;

    public TMP_InputField InputField;
    
    private float animationDuration = 0.5f;

    private int[] playerAmounts = {2, 3, 4};
    private int[] victoryPoints = {12, 15, 18};
    private int[] turnTimers = {30, 60, 90};
    
    public UIDataBase UIDataBase;
    public CardModelData CardModelData;

    private void Start()
    {
        createRoomCanvas.enabled = false;
        playerWaitingCanvas.enabled = false;
        findWaitingCanvas.enabled = false;
        
        GameSharedData.PlayerCount = 2;
        GameSharedData.GameVictoryPoints = 12;
        GameSharedData.PlayerTurnTime = 30;

        UpdateUI(0, GameSharedData.PlayerCount, 2,4);
        UpdateUI(1, GameSharedData.GameVictoryPoints,12, 18);
        UpdateUI(2, GameSharedData.PlayerTurnTime,30, 90);

        titleButtons[0].onClickEvent.AddListener(ChangePlayerAmount);
        titleButtons[1].onClickEvent.AddListener(ChangeVictoryPoints);
        titleButtons[2].onClickEvent.AddListener(ChangeTurnTimer);
        
        createRoomPanelButton.onClickEvent.AddListener(OpenCreateRoomPanel);
        findRoomButton.onClickEvent.AddListener(FindRoom);
        
        createRoomButton.onClickEvent.AddListener(CreateRoom);

        InputField.onValueChanged.AddListener(CheckInputFieldChanged);
        
        createRoomCanvasBackButton.onClickEvent.AddListener(() => createRoomCanvas.enabled = false);
        
        playerWaitingBackButton.onClickEvent.AddListener(StopCreateRoom);
        
        findWaitingBackButton.onClickEvent.AddListener(StopFindRoom);
        
        CheckInputFieldChanged(null);
    }

    private void CheckInputFieldChanged(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            createRoomPanelButton.gameObject.SetActive(false);
            findRoomButton.gameObject.SetActive(false);
        }
        else
        {
            createRoomPanelButton.gameObject.SetActive(true);
            findRoomButton.gameObject.SetActive(true);
        }
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

    private void OpenCreateRoomPanel()
    {
        createRoomCanvas.enabled = true;
    }

    private void OpenWaitingPanel()
    {
        ChangePlayersImages(1);
        playerWaitingCanvas.enabled = true;
    }

    private void OpenFindRoomPanel()
    {
        findWaitingCanvas.enabled = true;
    }

    private async void CreateRoom()
    {
        OpenWaitingPanel();
        GameSharedData.MyNickname = InputField.text;
        NetworkSystem.Instance.CreateRoom();
    }

    private async void FindRoom()
    {
        OpenFindRoomPanel();
        GameSharedData.MyNickname = InputField.text;
        NetworkSystem.Instance.FindRoom();
    }

    private async void StopCreateRoom()
    {
        NetworkSystem.Instance.CancelRoom();
        playerWaitingCanvas.enabled = false;
    }
    private async void StopFindRoom()
    {
        NetworkSystem.Instance.CancelRoom();
        findWaitingCanvas.enabled = false;
    }

    public void ChangePlayersImages(int playerAmounts)
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].gameObject.SetActive(false);
            players[i].color = Color.gray;
        }

        for (int i = 0; i < GameSharedData.PlayerCount; i++)
        {
            players[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < playerAmounts; i++)
        {
            players[i].color = Color.white;
        }
    }
}
