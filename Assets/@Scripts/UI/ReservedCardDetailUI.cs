using Doozy.Runtime.UIManager.Components;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ReservedCardDetailUI : MonoBehaviour
{
    public Canvas thisCanvas;
    
    public UIButton[] closeButtons;
    
    public ReservedCardElement[] reservedCardElements;
    void Start()
    {
        foreach (var closeButton in closeButtons)
        {
            closeButton.onClickEvent.AddListener(Close);
        }

        Close();
    }

    public void Open(int[] cards)
    {
        for (int i = 0; i < reservedCardElements.Length; i++)
        {
            reservedCardElements[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < cards.Length; i++)
        {
            reservedCardElements[i].InitializeCard(cards[i]);
            reservedCardElements[i].gameObject.SetActive(true);
        }
    }

    public void Close()
    {
        thisCanvas.enabled = false;
    }
}
