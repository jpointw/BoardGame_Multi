using System;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReservedCardElement : MonoBehaviour
{ 
    public CardInfo CardInfo { get; private set; }

    #region ExtraAction Fields

    public UIButton PurchaseButton;
    #endregion
    
    
    public TMP_Text cardPointText;
    public Image cardTypeImage;
    
    public Image[] requireCoinImages;
    
    private Transform _parentTransform;
    private LocalBoardPlayer _localBoardPlayer;

    public void InitializeCard(int cardId)
    {
        if (CardInfo.uniqueId == cardId)
        {
            PurchaseButton.interactable = CheckAvailablePurchase();
            return;
        }
        CardInfo = CardModelData.Instance.GetCardInfoById(cardId);
        cardPointText.text = CardInfo.points.ToString();
        cardTypeImage.sprite = UIDataBase.Instance.coinSprites[CardInfo.cardType];
        SetRequireCoinUIs();
        
        PurchaseButton.onClickEvent.AddListener(OnPurchaseButtonClicked);

        if (_localBoardPlayer == null)
        {
            _localBoardPlayer = FindFirstObjectByType<LocalBoardPlayer>();
        }
    }

    public void OnPurchaseButtonClicked()
    {
        _localBoardPlayer.RequestPurchaseReservedCard(CardInfo.cardType);
    }

    private bool CheckAvailablePurchase()
    {
        return Convert.ToBoolean(_localBoardPlayer.CanCardPurchase(CardInfo));
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
                requireCoinImages[i].sprite = UIDataBase.Instance.coinSprites[i];
                requireCoinImages[i].GetComponentInChildren<TMP_Text>().text = CardInfo.cost[i].ToString();
                requireCoinImages[i].gameObject.SetActive(true);
            }
        }
    }
}