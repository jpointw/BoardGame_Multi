using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CardSystem : NetworkBehaviour
{
    private CardModelData _cardDatas;
    
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
    
    
    public void InitializeCards()
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

        void AddFirstCardToField(NetworkLinkedList<int> heap)
        {
            foreach (var id in heap)
            {
                FieldCards.Add(id);
                heap.Remove(id);
                break;
            }
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
}
