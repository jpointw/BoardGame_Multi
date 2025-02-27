using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class SpecialCardElement : MonoBehaviour
{
    public SpecialCardInfo SpecialCardInfo { get; private set; }
    public bool IsPurchased { get; private set; }
    
    public TMP_Text cardPointText;
    
    public Image[] requireCardImages;

    public Image cardImage;
    
    public void InitializeCard(SpecialCardInfo cardInfo)
    {
        SpecialCardInfo = cardInfo;
        IsPurchased = false;
        cardPointText.text = cardInfo.points.ToString();
        cardImage.sprite = UIDataBase.Instance.GetSprite(cardInfo.illustration);
        SetRequireCoinUIs();
    }

    public void SetRequireCoinUIs()
    {
        for (int i = 0; i < requireCardImages.Length; i++)
        {
            requireCardImages[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < SpecialCardInfo.cost.Length; i++)
        {
            if (SpecialCardInfo.cost[i] > 0)
            {
                requireCardImages[i].color = UIDataBase.Instance.ownAssetColors[i];
                requireCardImages[i].GetComponentInChildren<TMP_Text>().text = SpecialCardInfo.cost[i].ToString();
                requireCardImages[i].gameObject.SetActive(true);
            }
        }
    }
}