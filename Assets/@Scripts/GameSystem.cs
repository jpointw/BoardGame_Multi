using System;
using Fusion;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class GameSystem : NetworkBehaviour
{
    private CardModelData _cardDatas;

    [Networked]
    [Capacity(4)]
    public NetworkDictionary<int, int> PlayerPoint { get; }
        = MakeInitializer(new Dictionary<int, int>()
        {
            { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 },
        });
    
    [Networked]
    [Capacity(16)]
    public NetworkLinkedList<int> FieldCards { get; }
        = MakeInitializer(new int[16]);
    [Networked]
    [Capacity(40)]
    public NetworkLinkedList<int> Level1Heap { get; }
        = MakeInitializer(new int[40]);
    
    [Networked]
    [Capacity(30)]
    public NetworkLinkedList<int> Level2Heap { get; }
        = MakeInitializer(new int[30]);
    [Networked]
    [Capacity(20)]
    public NetworkLinkedList<int> Level3Heap { get; }
        = MakeInitializer(new int[20]);

    [Networked] [Capacity(6)] public NetworkDictionary<int, int> CentralCoins { get; }
    = MakeInitializer(new Dictionary<int, int>(6));
    
    public PlayerRef CurrentPlayer;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;

        var A = new NetworkLinkedList<int>();
    }

    public void InitializeGame(List<PlayerRef> activePlayers)
    {
        if (!Object.HasStateAuthority) return;

        InitializeCards();
    }

    private void InitializeCards()
    {
        foreach (var cardInfo in _cardDatas.Get1LevelCardInfos())
            Level1Heap.Add(cardInfo.uniqueId);

        foreach (var cardInfo in _cardDatas.Get2LevelCardInfos())
            Level2Heap.Add(cardInfo.uniqueId);

        foreach (var cardInfo in _cardDatas.Get3LevelCardInfos())
            Level3Heap.Add(cardInfo.uniqueId);

        Shuffle(Level1Heap);
        Shuffle(Level2Heap);
        Shuffle(Level3Heap);

        for (int i = 0; i < 4; i++)
        {
            AddFirstCardToField(Level1Heap);
            AddFirstCardToField(Level2Heap);
            AddFirstCardToField(Level3Heap);
        }
    }

    private void AddFirstCardToField(NetworkLinkedList<int> heap)
    {
        foreach (var id in heap)
        {
            FieldCards.Add(id);
            heap.Remove(id);
            break;
        }
    }

    private void Shuffle(NetworkLinkedList<int> cards)
    {
        var list = new List<int>(cards);
        cards.Clear();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }

        foreach (var card in list)
        {
            cards.Add(card);
        }
    }

    // public void AddCardToPlayer(PlayerRef playerRef, int cardId, bool isOwned)
    // {
    //     if (!Object.HasStateAuthority) return;
    //
    //     if (isOwned)
    //     {
    //         PlayerOwnedCards[playerRef].Add(cardId);
    //     }
    //     else
    //     {
    //         ReservedCards[playerRef].Add(cardId);
    //     }
    // }

    public void PurchaseCard(PlayerRef playerRef, int cardUniqueId, int level)
    {
        if (!Object.HasStateAuthority) return;

        FieldCards.Remove(cardUniqueId);

        int newCardId = 0;
        switch (level)
        {
            case 1:
                foreach (var id in Level1Heap)
                {
                    newCardId = id;
                    Level1Heap.Remove(id);
                    break;
                }
                break;
            case 2:
                foreach (var id in Level2Heap)
                {
                    newCardId = id;
                    Level2Heap.Remove(id);
                    break;
                }
                break;
            case 3:
                foreach (var id in Level3Heap)
                {
                    newCardId = id;
                    Level3Heap.Remove(id);
                    break;
                }
                break;
        }

        if (newCardId != 0)
        {
            FieldCards.Add(newCardId);
        }

        // AddCardToPlayer(playerRef, cardUniqueId, true);
        //
        // PlayerPoint.Set(playerRef, _cardDatas.GetCardInfo(cardUniqueId).Points);
    }

    public void ModifyCoin(PlayerRef playerRef, CardInfo cardInfo)
    {
        if (!Object.HasStateAuthority) return;
    
        foreach (var cost in cardInfo.cost)
        {
            int type = cost;
            int requiredAmount = cardInfo.cost[cost];
    
            if (CentralCoins[type] < requiredAmount)
            {
                Debug.LogWarning($"Not enough coins of type {type}.");
                return;
            }
    
            CentralCoins.Set(type, CentralCoins[type] - requiredAmount);
        }
    }
}
