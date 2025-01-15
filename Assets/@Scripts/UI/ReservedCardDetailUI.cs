using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ReservedCardDetailUI : MonoBehaviour
{
    public Button[] closeButtons;

    /// <summary>
    /// Max Count 3
    /// </summary>
    public Image[] reservedCardImages;
    public Button[] purchaseButtons;
    void Start()
    {
        foreach (var closeButton in closeButtons)
        {
            closeButton.onClick.AddListener(() => { this.gameObject.SetActive(false); });
        }
    }

    public void Open()
    {
        
    }

    // public void Init()
    // {
    //     for (int i = 0; i < 3; i++)
    //     {
    //         reservedCardImages
    //     }
    // }
}
