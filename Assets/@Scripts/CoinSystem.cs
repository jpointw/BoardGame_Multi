using System;
using Fusion;
using UnityEngine;

public class CoinSystem : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnDetectCoinChanged))][Capacity(6)] public NetworkArray<int> CentralCoins { get; }
        = MakeInitializer(new int[6]);
    
    public Action<int[]> OnCoinChanged;
    
    public void InitializeCentralCoins(int playerCount = 2)
    {
        var tokenCount = playerCount switch
        {
            2 => 4,
            3 => 5,
            4 => 7,
            _ => 0
        };
        for (int i = 0; i < CentralCoins.Length - 1; i++)
        {
            CentralCoins.Set(i, tokenCount);
        }
        CentralCoins.Set(5, 5);
        Debug.Log("Central Coins initialized.");
        OnDetectCoinChanged();
    }

    public void ModifyCentralCoins(int[] coinChanges)
    {
        if (!Object.HasStateAuthority) return;

        for (int i = 0; i < coinChanges.Length; i++)
        {
            int newAmount = Mathf.Max(0, CentralCoins[i] + coinChanges[i]);
            CentralCoins.Set(i, newAmount);
        }

        Debug.Log($"CentralCoins updated: {string.Join(", ", CentralCoins.ToArray())}");
    }

    public void OnDetectCoinChanged()
    {
        OnCoinChanged?.Invoke(CentralCoins.ToArray());
    }
}