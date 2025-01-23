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
    [SerializeField] private LocalBoardPlayer _localBoardPlayer = null;
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
    
    private Transform _parentTransform;
    
    public void InitializeCard(CardInfo cardInfo, Transform parentTransform)
    {
        ThisToggle.AddToToggleGroup(parentTransform.parent.GetComponent<UIToggleGroup>());
        _parentTransform = parentTransform;
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
            _localBoardPlayer = FindFirstObjectByType<LocalBoardPlayer>();

        }
    }

    public void OnCardToggleChanged(bool isOn)
    {
        if (isOn)
        {
            RectTransform rectTransform = transform as RectTransform;
            rectTransform.SetParent(transform.parent.parent);
            rectTransform.SetAsLastSibling();
        }
        else
        {
            transform.SetParent(_parentTransform);
        }
    }

    public void OnCardElementClicked()
    {
        PurchaseButton.interactable = CheckAvailablePurchase();
        ReserveButton.interactable = CheckAvailableReserve();
    }

    public void OnPurchaseButtonClicked()
    {
        _localBoardPlayer.RequestPurchaseCard(CardInfo.uniqueId);
    }

    public void OnReserveButtonClicked()
    {
        _localBoardPlayer.RequestReserveCard(CardInfo.uniqueId);
    }

    private bool CheckAvailablePurchase()
    {
        return Convert.ToBoolean(_localBoardPlayer.CanCardPurchase(CardInfo));
    }

    private bool CheckAvailableReserve()
    {
        return _localBoardPlayer.BasePlayerInfo.ReservedCards.Count < 3;
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