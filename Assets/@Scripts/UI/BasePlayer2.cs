using System;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasePlayer2 : MonoBehaviour
{
    public BasePlayerInfo BasePlayerInfo;

    [SerializeField] protected TMP_Text playerNameText;
    [SerializeField] protected TMP_Text playerScoreText;
    [SerializeField] protected Image[] playerCardsImages;
    [SerializeField] protected TMP_Text[] playerCardsTexts;
    [SerializeField] protected Image[] playerCoinsImages;
    [SerializeField] protected TMP_Text[] playerCoinsTexts;
    
    [SerializeField] protected Image[] playerReservedCardsImages;

    public virtual void Init(BasePlayerInfo playerInfo)
    {
        BasePlayerInfo = playerInfo;

        BasePlayerInfo.OnPlayerNameChanged += UpdatePlayerNickNameUI;
        BasePlayerInfo.OnPlayerScoreChanged += UpdateScoreUI;
        BasePlayerInfo.OnPlayerCoinChanged += UpdateCoinsUI;
        BasePlayerInfo.OnPlayerCardChanged += UpdateCardsUI;
        BasePlayerInfo.OnPlayerReservedCardChanged += UpdateReservedCardsUI;
        UpdateUI();
    }
    
    
    public virtual void UpdateUI()
    {
        UpdatePlayerNickNameUI();
        UpdateScoreUI();
        UpdateCoinsUI();
        UpdateCardsUI();
    }

    protected virtual void UpdateScoreUI()
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = BasePlayerInfo.Score.ToString();
        }
    }

    protected virtual void UpdateCoinsUI()
    {
        for (int i = 0; i < playerCoinsTexts.Length; i++)
        {
            if (i < BasePlayerInfo.OwnedCoins.Length)
            {
                playerCoinsTexts[i].text = BasePlayerInfo.OwnedCoins[i] > 0 ? BasePlayerInfo.OwnedCoins[i].ToString() : "";
                playerCoinsImages[i].enabled = BasePlayerInfo.OwnedCoins[i] > 0;
            }
            else
            {
                playerCoinsImages[i].enabled = false;
            }
        }
    }

    protected virtual void UpdateCardsUI()
    {
        for (int i = 0; i < playerCardsTexts.Length; i++)
        {
            if (i < BasePlayerInfo.OwnedCards.Length && BasePlayerInfo.OwnedCards[i] != 0)
            {
                playerCardsTexts[i].text = BasePlayerInfo.OwnedCards[i].ToString();
                playerCardsImages[i].enabled = true;
            }
            else
            {
                playerCardsTexts[i].text = "";
                playerCardsImages[i].enabled = false;
            }
        }
    }

    protected virtual void UpdateReservedCardsUI()
    {
        for (int i = 0; i < BasePlayerInfo.ReservedCards.Count; i++)
        {
            if (BasePlayerInfo.ReservedCards[i] != -1)
            {
                playerReservedCardsImages[i].enabled = true;
                
            }
        }
    }

    protected virtual void UpdatePlayerNickNameUI()
    {
        playerNameText.text = BasePlayerInfo.name;
    }
}
