using System;
using UnityEngine;
using TMPro;
using Doozy.Runtime.UIManager.Components;

public class TitleScene : MonoBehaviour
{
    [Header("UI Buttons")]
    public UIButton playerAmountButton;
    public UIButton scoreButton;
    public UIButton timerButton;

    [Header("UI Texts")]
    public TMP_Text playerAmountText;
    public TMP_Text scoreText;
    public TMP_Text timerText;

    private int[] playerAmounts = {2, 3, 4};
    private int[] victoryPoints = {12, 15, 18};
    private int[] turnTimers = {30, 60, 90};

    private void Start()
    {
        GameSharedData.PlayerCount = 2;
        GameSharedData.GameVictoryPoints = 12;
        GameSharedData.PlayerTurnTime = 30;

        UpdateUI();

        playerAmountButton.onClickEvent.AddListener(ChangePlayerAmount);
        scoreButton.onClickEvent.AddListener(ChangeVictoryPoints);
        timerButton.onClickEvent.AddListener(ChangeTurnTimer);
    }

    private void ChangePlayerAmount()
    {
        int index = Array.IndexOf(playerAmounts, GameSharedData.PlayerCount);
        GameSharedData.PlayerCount = playerAmounts[(index + 1) % playerAmounts.Length];
        UpdateUI();
    }

    private void ChangeVictoryPoints()
    {
        int index = Array.IndexOf(victoryPoints, GameSharedData.GameVictoryPoints);
        GameSharedData.GameVictoryPoints = victoryPoints[(index + 1) % victoryPoints.Length];
        UpdateUI();
    }

    private void ChangeTurnTimer()
    {
        int index = Array.IndexOf(turnTimers, GameSharedData.PlayerTurnTime);
        GameSharedData.PlayerTurnTime = turnTimers[(index + 1) % turnTimers.Length];
        UpdateUI();
    }

    private void UpdateUI()
    {
        playerAmountText.text = $"Players: {GameSharedData.PlayerCount}";
        scoreText.text = $"Victory: {GameSharedData.GameVictoryPoints}";
        timerText.text = $"Turn Time: {GameSharedData.PlayerTurnTime}s";
    }
}