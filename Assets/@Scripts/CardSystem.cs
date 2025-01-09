using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CardSystem : NetworkBehaviour
{
    [SerializeField] private CardModelData cardModelData;
    [Networked][Capacity(16)] public NetworkLinkedList<int> FieldCards { get; }
        = MakeInitializer(new int[16]);

    [Networked][Capacity(40)] public NetworkLinkedList<int> Level1Deck { get; }
        = MakeInitializer(new int[40]);

    [Networked][Capacity(30)] public NetworkLinkedList<int> Level2Deck { get; }
        = MakeInitializer(new int[30]);

    [Networked][Capacity(20)] public NetworkLinkedList<int> Level3Deck { get; }
        = MakeInitializer(new int[20]);

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            InitializeDecks();
            InitializeField();
        }
    }

    public void InitializeDecks()
    {
        foreach (var card in cardModelData.GetCardsArrayByLevel(0))
        {
            Level1Deck.Add(card.uniqueId);
        }
        foreach (var card in cardModelData.GetCardsArrayByLevel(0))
        {
            Level2Deck.Add(card.uniqueId);
        }
        foreach (var card in cardModelData.GetCardsArrayByLevel(0))
        {
            Level3Deck.Add(card.uniqueId);
        }

        ShuffleDeck(Level1Deck);
        ShuffleDeck(Level2Deck);
        ShuffleDeck(Level3Deck);

        Debug.Log("Decks initialized and shuffled.");
    }

    public void InitializeField()
    {
        for (int i = 0; i < 4; i++)
        {
            AddCardToField(Level1Deck);
            AddCardToField(Level2Deck);
            AddCardToField(Level3Deck);
        }
    }

    public void AddCardToField(NetworkLinkedList<int> deck)
    {
        if (deck.Count > 0)
        {
            FieldCards.Add(deck[0]);
            deck.Remove(deck[0]);
        }
    }

    public CardInfo GetCardInfo(int cardId)
    {
        return cardModelData.GetCardInfoById(cardId);
    }

    public bool PurchaseCard(int cardId)
    {
        if (!Object.HasStateAuthority) return false;

        if (FieldCards.Contains(cardId))
        {
            FieldCards.Remove(cardId);
            return true;
        }

        return false;
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
