using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LocalBoardPlayer : BasePlayer2
{

    public bool interactEnabled = false;

    public int[] selectedCoins = new int[]{0,0,0,0,0,0};

    public Transform selectedCoinHolder;
    
    public Button[] selectedCoinsButtons;
    
    public UIButton ReservedCardButton;
    
    public UIButton EndTurnButton;


    public override void Init(BasePlayerInfo basePlayerInfo)
    {
        base.Init(basePlayerInfo);
        EndTurnButton.onClickEvent.AddListener(EndTurn);
        ReservedCardButton.onClickEvent.AddListener(OpenReservedCardPanel);
        for (int i = 0; i < selectedCoinsButtons.Length; i++)
        {
            selectedCoinsButtons[i].gameObject.SetActive(false);
        }
    }

    public int CanCardPurchase(CardInfo card)
    {
        int requireSpecialCoins = 0;

        for (int i = 0; i < BasePlayerInfo.OwnedCoins.Length - 1; i++)
        {
            requireSpecialCoins += Math.Max(0,BasePlayerInfo.OwnedCoins[i] - card.cost[i]);
        }

        return requireSpecialCoins;
    }
    
    public void RequestPurchaseCard(int cardId)
    {
        if (!interactEnabled) return;
        GameSystem.Instance.RPC_HandlePurchaseRequest(BasePlayerInfo.PlayerRef, cardId);
    }

    public void RequestPurchaseReservedCard(int cardId)
    {
        if (!interactEnabled) return;
        GameSystem.Instance.RPC_HandlePurchaseRequest(BasePlayerInfo.PlayerRef, cardId, true);
        GameSystem.Instance.InGameUI.reservedCardDetailUI.Close();
    }

    public void RequestReserveCard(int cardId)
    {
        if (!interactEnabled) return;
        GameSystem.Instance.RPC_HandleReserveCardRequest(BasePlayerInfo.PlayerRef, cardId);
        GameSystem.Instance.RPC_HandleTakeCoins(BasePlayerInfo.PlayerRef, new []{0,0,0,0,0,1});
    }

    public void TakeSelectedCoins()
    {
        GameSystem.Instance.RPC_HandleTakeCoins(BasePlayerInfo.PlayerRef, selectedCoins);
        for (int i = 0; i < selectedCoins.Length; i++)
        {
            selectedCoins[i] = 0;
        }

        for (int i = 0; i < selectedCoinsButtons.Length; i++)
        {
            selectedCoinsButtons[i].gameObject.SetActive(false);
        }
    }

    public void OpenReservedCardPanel()
    {
        GameSystem.Instance.InGameUI.ShowReservedCardsDetail(BasePlayerInfo.ReservedCards.ToArray());
        
    }

    private void EndTurn()
    {
        if (!interactEnabled) return;
        interactEnabled = false;
        TakeSelectedCoins();
        GameSystem.Instance.EndTurn(BasePlayerInfo.PlayerRef);
    }
    public void SelectCoin(int coinType)
    {
        if (!interactEnabled) return;
        if (!CanSelectCoin(coinType))
        {
            Debug.LogError("Can't select coin");
            return;
        }

        if (!CanTakeCoins())
        {
            Debug.LogError("Can't take coins");
            return;
        }
        selectedCoins[coinType]++;
        GameSystem.Instance.InGameUI.UpdateTempCoinText(coinType,selectedCoins[coinType]);
        UpdateSelectedCoinUI(coinType);
    }
    private bool CanSelectCoin(int coinType)
    {
        int differentCoinTypes = selectedCoins.Count(c => c > 0);   
        int selectedTwoCoins = selectedCoins.Count(c => c > 1);     
        int totalSelectedCoins = selectedCoins.Sum();               
        int availableCoins = GameSystem.Instance.CoinSystem.CentralCoins[coinType]; 
        
        bool isMaxTotalCoins = (BasePlayerInfo.OwnedCoins.Sum() + totalSelectedCoins >= 10);
        
        bool cannotAddNewCoin = (selectedCoins[coinType] == 0 && (differentCoinTypes >= 3 
                                                                  || selectedTwoCoins > 0));
        
        bool cannotAddSameCoin = (selectedCoins[coinType] == 1 && (availableCoins <= selectedCoins[coinType]
                                                                   || differentCoinTypes > 1
                                                                   || availableCoins < 4));
        
        bool isCoinLimitExceeded = (selectedCoins[coinType] >= 2);

        return !(isMaxTotalCoins || cannotAddNewCoin || cannotAddSameCoin || isCoinLimitExceeded);
    }
    private bool CanTakeCoins()
    {
        int totalCoinsAfterTake = BasePlayerInfo.OwnedCoins.Sum() + selectedCoins.Sum();
        
        if (totalCoinsAfterTake > 10) return false;

        return true;
    }

    public void HandleInput()
    {
        interactEnabled = true;
    }
    
    private void UpdateSelectedCoinUI(int coinType)
    {
        var button = GetCoinButton();
        button.GetComponent<Image>().sprite = UIDataBase.Instance.coinSprites[coinType];
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            selectedCoins[coinType]--;
            button.gameObject.SetActive(false);
        });
        button.gameObject.SetActive(true);
    }

    private Button GetCoinButton()
    {
        for (int i = 0; i < selectedCoinsButtons.Length; i++)
        {
            if (!selectedCoinsButtons[i].gameObject.activeSelf)
            {
                return selectedCoinsButtons[i]; 
            }
        }

        return null;
    }

}