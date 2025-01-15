using System;
using Fusion;
using UnityEngine.UI;

public class LocalBoardPlayer : BasePlayer
{

    public bool interactEnabled = false;
    
    public Button EndTurnButton;


    public override void Initialize(PlayerRef playerRef)
    {
        base.Initialize(playerRef);
        EndTurnButton.onClick.AddListener(EndTurn);
    }

    public int CanCardPurchase(CardInfo card)
    {
        int requireSpecialCoins = 0;

        for (int i = 0; i < OwnedCoins.Length; i++)
        {
            requireSpecialCoins += Math.Max(0, card.cost[i] - OwnedCoins[i]);
        }

        return requireSpecialCoins;
    }
    
    public void RequestPurchaseCard(CardElement cardElement)
    {
        if (!Object.HasInputAuthority) return;
        GameSystem.Instance.HandlePurchaseRequest(PlayerRef, cardElement);
    }

    public void RequestReserveCard(CardInfo cardInfo)
    {
        if (!Object.HasInputAuthority) return;
        GameSystem.Instance.HandleReserveCardRequest(PlayerRef, cardInfo);
    }

    private void EndTurn()
    {
        if (!Object.HasInputAuthority) return;

        interactEnabled = false;
        GameSystem.Instance.EndTurn(PlayerRef);
    }

    public void HandleInput()
    {
        interactEnabled = true;
    }
}