using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class CardElement : MonoBehaviour
{
    public CardInfo CardInfo { get; private set; }
    public bool IsPurchased { get; private set; }
    
    public Button Button { get; private set; }
    
    public TMP_Text cardPointText;
    public Image cardTypeImage;
    
    public Image[] requireCoinImages;
    
    public void InitializeCard(CardInfo cardInfo)
    {
        CardInfo = cardInfo;
        IsPurchased = false;
        cardPointText.text = cardInfo.points.ToString();
        cardTypeImage.sprite = UIDataBase.instance.coinSprites[cardInfo.cardType];
        SetRequireCoinUIs();
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
                requireCoinImages[i].GetComponentInChildren<TMP_Text>().text = CardInfo.cost[i].ToString();
                requireCoinImages[i].gameObject.SetActive(true);
            }
        }
    }
}