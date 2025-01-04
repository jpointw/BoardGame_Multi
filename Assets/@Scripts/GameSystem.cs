using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    [Networked] public PlayerRef CurrentPlayer { get; private set; }
    [Networked] public bool IsGameOver { get; private set; }

    private List<Player> players = new List<Player>();

    public void StartGame()
    {
        // 보석, 카드, 플레이어 초기화
        InitializeGems();
        InitializeCards();
        InitializePlayers();
        CurrentPlayer = players[0].PlayerRef;
    }

    public void EndTurn()
    {
        // 다음 플레이어로 턴 이동
        int currentIndex = players.FindIndex(p => p.PlayerRef == CurrentPlayer);
        CurrentPlayer = players[(currentIndex + 1) % players.Count].PlayerRef;
    }

    public void CheckVictory()
    {
        foreach (var player in players)
        {
            if (player.Score >= 15)
            {
                IsGameOver = true;
                Debug.Log($"Player {player.PlayerRef.PlayerId} wins!");
                break;
            }
        }
    }

    private void InitializeGems()    { /* 보석 초기화 로직 */ }
    private void InitializeCards()   { /* 카드 초기화 로직 */ }
    private void InitializePlayers() { /* 플레이어 초기화 로직 */ }
}