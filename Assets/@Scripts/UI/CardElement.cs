using System;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class CardElement : MonoBehaviour
{
    private LocalBoardPlayer _localBoardPlayer =null;
    public CardInfo CardInfo { get; private set; }
    public bool IsPurchased { get; private set; }

    public UIToggle ThisToggle;

    #region ExtraAction Fields

    public UIButton PurchaseButton;
    public UIButton ReserveButton;
    #endregion
    
    
    public TMP_Text cardPointText;
    public Image cardTypeImage;
    
    public Image[] requireCoinImages;
    
    public void InitializeCard(CardInfo cardInfo)
    {
        CardInfo = cardInfo;
        IsPurchased = false;
        cardPointText.text = cardInfo.points.ToString();
        cardTypeImage.sprite = UIDataBase.Instance.coinSprites[cardInfo.cardType];
        SetRequireCoinUIs();
        ThisToggle.onClickEvent.AddListener(OnCardElementClicked);
        
        PurchaseButton.onClickEvent.AddListener(OnPurchaseButtonClicked);
        ReserveButton.onClickEvent.AddListener(OnReserveButtonClicked);

        if (_localBoardPlayer == null)
        {
            var localPlayer = GameSystem.Instance.Runner.LocalPlayer;
            _localBoardPlayer ??= 
                GameSystem.Instance.Players.Find(p => p.PlayerRef == localPlayer)
                    .GetComponent<LocalBoardPlayer>();
        }
    }

    public void OnCardElementClicked()
    {
            PurchaseButton.interactable = CheckAvailablePurchase();
            ReserveButton.interactable = CheckAvailableReserve();
    }

    public void OnPurchaseButtonClicked()
    {
        _localBoardPlayer.RequestPurchaseCard(this);
    }

    public void OnReserveButtonClicked()
    {
        _localBoardPlayer.RequestReserveCard(CardInfo);
    }

    private bool CheckAvailablePurchase()
    {
        return Convert.ToBoolean(_localBoardPlayer.CanCardPurchase(CardInfo));
    }

    private bool CheckAvailableReserve()
    {
        return _localBoardPlayer.ReservedCards.Length < 3;
    }
    public void SetRequireCoinUIs()
    {
        for (int i = 0; i < requireCoinImages.Length; i++)
        {
            requireCoinImages[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < CardInfo.cost.Length; i++)
        {
            if (CardInfo.cost[i] > 0)
            {
                requireCoinImages[i].sprite = UIDataBase.Instance.coinSprites[CardInfo.cost[i]];
                requireCoinImages[i].GetComponentInChildren<TMP_Text>().text = CardInfo.cost[i].ToString();
                requireCoinImages[i].gameObject.SetActive(true);
            }
        }
    }
}