using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemoteBoardPlayer : NetworkBehaviour
{
    public PlayerRef PlayerRef {get; private set; }
    [Networked] public int Score { get; private set; }

    [Networked][Capacity(6)]
    public NetworkArray<int> OwnedCoins { get; }
        = MakeInitializer(new int[6]);

    [Networked][Capacity(6)]
    public NetworkArray<int> OwnedCards { get; }
        = MakeInitializer(new int[6]);

    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private Image[] playerCardsImages;
    [SerializeField] private TMP_Text[] playerCardsTexts;
    [SerializeField] private Image[] playerCoinsImages;
    [SerializeField] private TMP_Text[] playerCoinsTexts;

    public void Initialize(PlayerRef playerRef)
    {
        PlayerRef = playerRef;
    }
    
    public override void Spawned()
    {
        UpdateUI();
    }

    public void ModifyScore(int amount)
    {
        if (!Object.HasStateAuthority) return;

        Score += amount;

        RPC_UpdateScore(Score);
    }

    public void AddCard(int cardId)
    {
        if (!Object.HasStateAuthority) return;

        for (int i = 0; i < OwnedCards.Length; i++)
        {
            if (OwnedCards[i] == 0)
            {
                OwnedCards.Set(i, cardId);
                break;
            }
        }

        RPC_UpdateCard(cardId);
    }

    public void ModifyCoins(int coinType, int amount)
    {
        if (!Object.HasStateAuthority) return;

        OwnedCoins.Set(coinType, OwnedCoins[coinType] + amount);

        RPC_UpdateCoins(coinType, OwnedCoins[coinType]);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateScore(int newScore)
    {
        Score = newScore;
        UpdateScoreUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateCard(int cardId)
    {
        UpdateCardsUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateCoins(int coinType, int newAmount)
    {
        OwnedCoins.Set(coinType, newAmount);
        UpdateCoinsUI();
    }

    public void UpdateUI()
    {
        UpdateScoreUI();
        UpdateCoinsUI();
        UpdateCardsUI();
    }

    private void UpdateScoreUI()
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = $"Score: {Score}";
            Debug.Log($"[UI] Score updated: {Score}");
        }
    }

    private void UpdateCoinsUI()
    {
        for (int i = 0; i < playerCoinsTexts.Length; i++)
        {
            if (i < OwnedCoins.Length)
            {
                playerCoinsTexts[i].text = OwnedCoins[i].ToString();
                playerCoinsImages[i].enabled = OwnedCoins[i] > 0;
            }
            else
            {
                playerCoinsTexts[i].text = "0";
                playerCoinsImages[i].enabled = false;
            }
        }

        Debug.Log($"[UI] Coins updated: {string.Join(", ", OwnedCoins)}");
    }

    private void UpdateCardsUI()
    {
        for (int i = 0; i < playerCardsTexts.Length; i++)
        {
            if (i < OwnedCards.Length && OwnedCards[i] != 0)
            {
                playerCardsTexts[i].text = $"Card {OwnedCards[i]}";
                playerCardsImages[i].enabled = true;
            }
            else
            {
                playerCardsTexts[i].text = "";
                playerCardsImages[i].enabled = false;
            }
        }

        Debug.Log($"[UI] Cards updated: {OwnedCards.Length}");
    }

    
}
