using System.Collections.Generic;
using Fusion;
using static Define;

public class Card : NetworkBehaviour
{
    public int CardID { get; private set; }
    public int Points { get; private set; }
    public bool IsPurchased { get; private set; }
    public Dictionary<CoinType, int> Cost { get; private set; }

    public void InitializeCard(CardInfo cardInfo)
    {
        CardID = cardInfo.uniqueId;
        IsPurchased = false;
    }

    public bool CanBePurchased(BoardPlayer player)
    {
        foreach (var cost in Cost)
        {
            if (player.Gems[cost.Key] < cost.Value)
                return false;
        }
        return true;
    }

    public void Purchase(BoardPlayer player)
    {
        if (CanBePurchased(player))
        {
            player.BuyCard(this);
            IsPurchased = true;
        }
    }
}