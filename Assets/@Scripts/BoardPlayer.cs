using System.Collections.Generic;
using Fusion;
using static Define;

public class Player : NetworkBehaviour
{
    [Networked] public int Score { get; private set; }
    [Networked] public int MaxTokens { get; private set; } = 10;

    public List<Card> OwnedCards { get; private set; } = new List<Card>();
    public Dictionary<CoinType, int> Gems { get; private set; } = new Dictionary<CoinType, int>();

    public void TakeGems(Dictionary<CoinType, int> gems)
    {
        foreach (var gem in gems)
        {
            if (Gems.ContainsKey(gem.Key))
                Gems[gem.Key] += gem.Value;
        }
    }

    public void BuyCard(Card card)
    {
        // 카드 구매 로직
        foreach (var cost in card.Cost)
        {
            if (Gems[cost.Key] >= cost.Value)
                Gems[cost.Key] -= cost.Value;
        }
        OwnedCards.Add(card);
        Score += card.Points;
    }
}