using System;
using Doozy.Runtime.UIManager.Components;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CoinElement : MonoBehaviour
{
    private LocalBoardPlayer _localBoardPlayer = null;
    public int CoinType = 0;

    public UIButton ThisButton;

    public void InitializeCoin()
    {
        if (CoinType == 5) return;
        ThisButton ??= GetComponent<UIButton>();
        ThisButton.onClickEvent.AddListener(OnCoinElementClicked);

        if (_localBoardPlayer == null)
        {
            _localBoardPlayer = FindFirstObjectByType<LocalBoardPlayer>();
        }
    }

    public void OnCoinElementClicked()
    {
        if (!_localBoardPlayer.interactEnabled) return;
        if (GameSystem.Instance.CoinSystem.CentralCoins[CoinType] > 0)
        {
            _localBoardPlayer.SelectCoin(CoinType);
        }
    }
}