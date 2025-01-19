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
    [Capacity(5)]
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
    
    public void InitializeDecks()
    {
        FieldCards.Clear();
        FieldSpecialCards.Clear();
        
        InitializeDeck(Level3Deck, 2);
        InitializeDeck(Level2Deck, 1);
        InitializeDeck(Level1Deck, 0);
        
        InitializeSpecialCards(FieldSpecialCards);

        
        for (var index = 0; index < CardModelData.Instance.specialCardInfos.Length; index++)
        {
            var specialCard = CardModelData.Instance.specialCardInfos[index];
            FieldSpecialCards.Add(specialCard.uniqueId);
        }
        
        ShuffleDeck(FieldSpecialCards);

        for (int i = FieldSpecialCards.Count; i > GameSharedData.PlayerCount + 1; i--)
        {
            FieldSpecialCards.Remove(i);
        }

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

    private void InitializeSpecialCards(NetworkLinkedList<int> specialCards)
    {
        var cards = CardModelData.Instance.specialCardInfos;
        for (int index = 0; index < cards.Length; index++)
        {
            specialCards.Set(index, cards[index].uniqueId);
        }
        ShuffleDeck(specialCards);
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
                FieldCards.Set(i, targetDeck[0]);
                targetDeck.Remove(targetDeck[0]);
            }
        }
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
