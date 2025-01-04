using System.Collections.Generic;
using Fusion;
using static Define;

public class Card : NetworkBehaviour
{
    [Networked] public int CardID { get; private set; }
    [Networked] public int Points { get; private set; }
    [Networked] public Dictionary<CoinType, int> Cost { get; private set; }
    [Networked] public bool IsPurchased { get; private set; }

    public void InitializeCard(int cardID, int points, Dictionary<CoinType, int> cost)
    {
        CardID = cardID;
        Points = points;
        Cost = cost;
        IsPurchased = false;
    }

    public bool CanBePurchased(Player player)
    {
        foreach (var cost in Cost)
        {
            if (player.Gems[cost.Key] < cost.Value)
                return false;
        }
        return true;
    }

    public void Purchase(Player player)
    {
        if (CanBePurchased(player))
        {
            player.BuyCard(this);
            IsPurchased = true;
        }
    }
}