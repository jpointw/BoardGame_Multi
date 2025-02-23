using System;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasePlayerInfo : NetworkBehaviour
{
    public Action OnPlayerNameChanged;
    public Action OnPlayerScoreChanged;
    public Action OnPlayerCoinChanged;
    public Action OnPlayerCardChanged;
    public Action OnPlayerReservedCardChanged;
    
    [Networked]
    public PlayerRef PlayerRef { get; protected set; }

    [Networked, OnChangedRender(nameof(OnNameChanged))]
    public string Name { get; set; }

    [Networked,OnChangedRender(nameof(OnScoreChanged))]
    public int Score { get; protected set; }

    [Networked,OnChangedRender(nameof(OnCoinsChanged))]
    [Capacity(6)]
    public NetworkArray<int> OwnedCoins { get; } = MakeInitializer(new int[6]);

    [Networked,OnChangedRender(nameof(OnCardsChanged))]
    [Capacity(6)]
    public NetworkArray<int> OwnedCards { get; } = MakeInitializer(new int[6]);

    [Networked, OnChangedRender(nameof(OnReservedCardsChanged))]
    [Capacity(3)]
    public NetworkLinkedList<int> ReservedCards { get; } = default;
    
    public virtual void Initialize(PlayerRef playerRef)
    {
        PlayerRef = playerRef;
    }

    public virtual void ModifyScore(int amount)
    {
        if (!Object.HasStateAuthority) return;
        Score += amount;
    }

    public virtual void ModifyCoins(int[] coinChanges)
    {
        if (!Object.HasStateAuthority) return;

        for (int i = 0; i < coinChanges.Length; i++)
        {
            OwnedCoins.Set(i, Mathf.Max(0, OwnedCoins[i] + coinChanges[i]));
        }
    }
    
    public virtual void ModifyCoin(int coinType,int coinChanges)
    {
        if (!Object.HasStateAuthority) return;

        OwnedCoins.Set(coinType, Mathf.Max(0, OwnedCoins[coinType] + coinChanges));
    }

    public virtual void AddCard(int cardType)
    {
        if (!Object.HasStateAuthority) return;

        OwnedCards.Set(cardType, OwnedCards[cardType] + 1);
    }

    public virtual void AddReservedCard(int cardId)
    {
        ReservedCards.Add(cardId);
    }

    public virtual void RemoveReservedCard(int cardId)
    {
        for (int i = 0; i < ReservedCards.Count; i++)
        {
            if (ReservedCards[i] == cardId)
            {
                ReservedCards.Remove(i);
            }
        }
    }

    public void OnNameChanged()
    {
        OnPlayerNameChanged?.Invoke();
    }

    public void OnScoreChanged()
    {
        OnPlayerScoreChanged?.Invoke();
    }

    public void OnCoinsChanged()
    {
        OnPlayerCoinChanged?.Invoke();
    }

    public void OnCardsChanged()
    {
        OnPlayerCardChanged?.Invoke();
    }

    public void OnReservedCardsChanged()
    {
        OnPlayerReservedCardChanged?.Invoke();
    }
}
