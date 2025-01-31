using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class CardSystem : NetworkBehaviour
{

    [Networked]
    [Capacity(10)]
    public NetworkLinkedList<int> FieldSpecialCards { get; } 
        = default;
    [Networked][Capacity(12)] public NetworkLinkedList<int> FieldCards { get; }
        = default;

    [Networked][Capacity(40)] public NetworkLinkedList<int> Level1Deck { get; }
        = default;

    [Networked][Capacity(30)] public NetworkLinkedList<int> Level2Deck { get; }
        = default;

    [Networked][Capacity(20)] public NetworkLinkedList<int> Level3Deck { get; }
        = default;

    public Action<int,int> OnCardAdded;
    public Action<int,int> OnCardRemoved;
    
    public void InitializeDecks()
    {
        FieldCards.Clear();
        FieldSpecialCards.Clear();
        
        InitializeDeck(Level3Deck, 3);
        InitializeDeck(Level2Deck, 2);
        InitializeDeck(Level1Deck, 1);

        InitializeSpecialCards();

        ShuffleDeck(Level3Deck);
        ShuffleDeck(Level2Deck);
        ShuffleDeck(Level1Deck);

        InitializeField();
        Debug.Log("Decks initialized and shuffled.");
    }

    
    
    private void InitializeDeck(NetworkLinkedList<int> deck, int level)
    {
        var cards = CardModelData.Instance.GetCardsArrayByLevel(level);
        for (int index = 0; index < cards.Length; index++)
        {
            deck.Set(index, cards[index].uniqueId);
        }
    }

    private void InitializeSpecialCards()
    {
        for (var index = 0; index < CardModelData.Instance.specialCardInfos.Length; index++)
        {
            var specialCard = CardModelData.Instance.specialCardInfos[index];
            FieldSpecialCards.Add(specialCard.uniqueId);
        }
        
        ShuffleDeck(FieldSpecialCards);

        for (int i = FieldSpecialCards.Count; i > GameSystem.Instance.PlayerCount + 1; i--)
        {
            FieldSpecialCards.Remove(i);
        }
    }

    public void InitializeField()
    {
        for (int i = 0; i < 4; i++) AddCardToField(Level3Deck);
        for (int i = 0; i < 4; i++) AddCardToField(Level2Deck);
        for (int i = 0; i < 4; i++) AddCardToField(Level1Deck);
    }

    public void AddCardToField(NetworkLinkedList<int> deck)
    {
        if (deck.Count > 0)
        {
            FieldCards.Add(deck[0]);
            deck.Remove(deck[0]);
        }
    }

    public void RemoveCardFromField(CardInfo cardInfo)
    {
        var targetDeck = GetDecks(cardInfo.cardLevel);;

        for (int i = 0; i < FieldCards.Count; i++)
        {
            if (FieldCards[i] == cardInfo.uniqueId)
            {
                RPC_OnCardRemoved(FieldCards[i], i);
                FieldCards.Set(i, targetDeck[0]);
                RPC_OnCardAdded(targetDeck[0],i);
                targetDeck.Remove(targetDeck[0]);
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnCardAdded(int cardId, int slotIndex)
    {
        OnCardAdded?.Invoke(cardId, slotIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnCardRemoved(int cardId, int slotIndex)
    {
        OnCardRemoved?.Invoke(cardId, slotIndex);
    }
    public CardInfo GetCardInfo(int cardId)
    {
        return CardModelData.Instance.GetCardInfoById(cardId);
    }

    public NetworkLinkedList<int> GetDecks(int cardLevel)
    {
        return cardLevel switch
        {
            1 => Level1Deck,
            2 => Level2Deck,
            3 => Level3Deck,
            _ => Level1Deck
        };
    }
    
    public void ShuffleDeck(NetworkLinkedList<int> deck)
    {
        var list = new List<int>(deck);
        deck.Clear();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }

        foreach (var card in list)
        {
            deck.Add(card);
        }
    }
}
