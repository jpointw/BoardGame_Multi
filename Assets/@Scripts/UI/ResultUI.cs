using System;
using DG.Tweening;
using Doozy.Runtime.UIManager.Components;
using TMPro;
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    public Canvas thisCanvas;
    public TMP_Text winnerText;

    public RectTransform fillRectTransform;
    public UIButton exitButton;

    private void Start()
    {
        thisCanvas.enabled = false;
    }

    public void Init()
    {
        exitButton.onClickEvent.AddListener(() => NetworkSystem.Instance.EndGame());
    }

    public void Open(string winnerName)
    {
        winnerText.text = winnerName;

        fillRectTransform.transform.localScale = Vector3.zero;
        thisCanvas.enabled = true;
        fillRectTransform.transform.DOScale(1, 0.5f);
    }
}
