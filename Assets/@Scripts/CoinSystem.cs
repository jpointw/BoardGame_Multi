using System;
using Fusion;
using UnityEngine;

public class CoinSystem : NetworkBehaviour
{
    
    [Networked][Capacity(6)] public NetworkArray<int> CentralCoins { get; }
        = MakeInitializer(new int[6]);

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            InitializeCentralCoins();
            Debug.Log("CoinSystem initialized.");
        }
    }
    public void InitializeCentralCoins()
    {
        for (int i = 0; i < CentralCoins.Length; i++)
        {
            CentralCoins.Set(i, 10);
        }
        Debug.Log("Central Coins initialized.");
    }

    public void ModifyCentralCoins(int coinType, int amount)
    {
        if (!Object.HasStateAuthority) return;

        int currentAmount = CentralCoins[coinType];
        int newAmount = Mathf.Max(0, currentAmount + amount);
        CentralCoins.Set(coinType, newAmount);

        Debug.Log($"CentralCoin updated: Type {coinType}, New Amount {newAmount}");
        RPC_UpdateCoin(coinType, newAmount);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestModifyCoins(int coinType, int amount)
    {
        ModifyCentralCoins(coinType, amount);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateCoin(int coinType, int newAmount)
    {
        Debug.Log($"[Client] Coin type {coinType} updated to {newAmount}.");
    }
}
