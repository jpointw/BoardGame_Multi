using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class BoardPlayer : NetworkBehaviour
{
    public PlayerRef PlayerRef { get; private set; }

    [Networked] public int Score { get; private set; }

    [Networked][Capacity(6)]
    public NetworkArray<int> OwnedCoins { get; }
        = MakeInitializer(new int[6]);

    [Networked][Capacity(6)]
    public NetworkArray<int> OwnedCards { get; }
        = MakeInitializer(new int[6]);

    public string PlayerName;

    public void Initialize(PlayerRef playerRef, string playerName)
    {
        PlayerRef = playerRef;
        PlayerName = playerName;
    }

    public void TakeCoins(int[] coins)
    {
        if (!HasInputAuthority) return;

        for (int i = 0; i < coins.Length; i++)
        {
            OwnedCoins.Set(i, OwnedCoins[i] + coins[i]);
        }

        GameSystem.Instance.ModifyCentralCoins(coins);
    }

    public void RequestPurchaseCard(int cardId)
    {
        if (!HasInputAuthority) return;

        GameSystem.Instance.RPC_RequestPurchaseCard(Object.InputAuthority, cardId);
        Debug.Log($"Requested to purchase card {cardId}.");
    }
}