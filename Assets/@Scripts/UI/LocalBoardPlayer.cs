using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LocalBoardPlayer : BasePlayer
{

    public bool interactEnabled = false;

    public int[] selectedCoins = new int[]{0,0,0,0,0,0};

    public Transform selectedCoinHolder;
    
    public Button[] selectedCoinsButtons;
    
    public UIButton ReservedCardButton;
    
    public UIButton EndTurnButton;


    public override void Initialize(PlayerRef playerRef)
    {
        base.Initialize(playerRef);
        EndTurnButton.onClickEvent.AddListener(EndTurn);
        ReservedCardButton.onClickEvent.AddListener(OpenReservedCardPanel);
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
        GameSystem.Instance.HandleTakeCoins(PlayerRef, new []{0,0,0,0,0,1});
    }

    public void TakeSelectedCoins()
    {
        if (!Object.HasInputAuthority) return;
        GameSystem.Instance.HandleTakeCoins(PlayerRef, selectedCoins);
    }

    public void OpenReservedCardPanel()
    {
        if (!Object.HasInputAuthority) return;
        GameSystem.Instance.InGameUI.ShowReservedCardsDetail(ReservedCards.ToArray());
        
    }

    private void EndTurn()
    {
        if (!Object.HasInputAuthority) return;

        interactEnabled = false;
        TakeSelectedCoins();
        GameSystem.Instance.EndTurn(PlayerRef);
    }
    public void SelectCoin(int coinType)
    {
        if (!Object.HasInputAuthority) return;
        if (!CanSelectCoin(coinType)) return;
        if (!CanTakeCoins()) return;
        selectedCoins[coinType]++;
        UpdateSelectedCoinUI();
    }
    
    private bool CanSelectCoin(int coinType)
    {
        int differentCoins = selectedCoins.Count(c => c > 0);
        if (differentCoins >= 3 && selectedCoins[coinType] == 0) return false;

        if (selectedCoins[coinType] == 1)
        {
            int availableCoins = GameSystem.Instance.CoinSystem.CentralCoins[coinType];
            if (availableCoins < 4) return false;
        }

        return true;
    }
    private bool CanTakeCoins()
    {
        int totalCoinsAfterTake = OwnedCoins.Sum() + selectedCoins.Sum();
        
        if (totalCoinsAfterTake > 10) return false;

        return true;
    }

    public void HandleInput()
    {
        interactEnabled = true;
    }
    
    private void UpdateSelectedCoinUI()
    {
        for (int i = 0; i < selectedCoinsButtons.Length; i++)
        {
            if (selectedCoins[i] > 0)
            {
                selectedCoinsButtons[i].GetComponent<Image>().sprite = UIDataBase.Instance.coinSprites[i];
                selectedCoinsButtons[i].onClick.RemoveAllListeners();

                int index = i; // 클로저 개같은 거
                selectedCoinsButtons[i].onClick.AddListener(() =>
                {
                    selectedCoins[index]--;
                    UpdateSelectedCoinUI();
                });

                selectedCoinsButtons[i].gameObject.SetActive(true);
            }
            else
            {
                selectedCoinsButtons[i].gameObject.SetActive(false);
            }
        }
    }

}