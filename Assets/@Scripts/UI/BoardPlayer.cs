using System.Collections.Generic;
using Fusion;
using static Define;

public class BoardPlayer : NetworkBehaviour
{
    public PlayerRef PlayerRef {get; private set; }
    [Networked] public int Score { get; private set; }

    [Networked][Capacity(6)]
    public NetworkArray<int> OwnedCoins { get; }
        = MakeInitializer(new int[6]);

    [Networked][Capacity(6)]
    public NetworkArray<int> OwnedCards { get; }
        = MakeInitializer(new int[6]);
    
    /// <summary>
    /// New UI Element
    /// </summary>
    public string PlayerName;

    public void TakeGems(int[] coins)
    {
        for (int i = 0; i < 6; i++)
        {
            OwnedCoins.Set(i, i+coins[i]);
        }
    }

    public void BuyCard(CardInfo card)
    {
        for (int i = 0; i < UPPER; i++)
        {
            foreach (var cost in card.cost)
            {
                if (Gems[cost.Key] >= cost.Value)
                    Gems[cost.Key] -= cost.Value;
            }
        }
        OwnedCards.Set()
        Score += card.Points;
    }
}