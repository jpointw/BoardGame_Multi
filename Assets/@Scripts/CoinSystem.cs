using System;
using Fusion;
using UnityEngine;

public class CoinSystem : NetworkBehaviour
{
    [Networked][Capacity(6)] public NetworkArray<int> CentralCoins { get; } 
        = MakeInitializer(new int[6]);

    public void InitializeCentralCoins(int playerCount = 2)
    {
        for (int i = 0; i < CentralCoins.Length; i++)
        {
            CentralCoins.Set(i, 10); // 초기 코인 설정
        }
        Debug.Log("Central Coins initialized.");
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
}