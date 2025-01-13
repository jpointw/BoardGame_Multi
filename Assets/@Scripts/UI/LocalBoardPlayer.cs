using System;
using Fusion;

public class LocalBoardPlayer : BasePlayer
{
    [Networked,OnChangedRender(nameof(OnCardsChanged))]
    [Capacity(3)]
    public NetworkArray<int> ReservedCards { get; } = default;

    public bool interactEnabled = false;
    
    public int CanCardPurchase(CardInfo card)
    {
        int requireSpecialCoins = 0;

        for (int i = 0; i < OwnedCoins.Length; i++)
        {
            requireSpecialCoins += Math.Max(0, card.cost[i] - OwnedCoins[i]);
        }

        return requireSpecialCoins;
    }
    
    
    
    private void RequestPurchaseCard(int cardId)
    {
        if (!Object.HasInputAuthority) return;

    }

    private void EndTurn()
    {
        if (!Object.HasInputAuthority) return;

        interactEnabled = false;
    }

    public void HandleInput()
    {
        interactEnabled = true;
    }
}