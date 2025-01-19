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
        ThisButton ??= GetComponent<UIButton>();
        ThisButton.onClickEvent.AddListener(OnCoinElementClicked);

        if (_localBoardPlayer == null)
        {
            var localPlayer = GameSystem.Instance.Runner.LocalPlayer;
            _localBoardPlayer ??=
                GameSystem.Instance.Players.Find(p => p.PlayerRef == localPlayer)
                    .GetComponent<LocalBoardPlayer>();
        }
    }

    public void OnCoinElementClicked()
    {
        if (GameSystem.Instance.CoinSystem.CentralCoins[CoinType] <= 0)
            _localBoardPlayer.SelectCoin(CoinType);
    }
}