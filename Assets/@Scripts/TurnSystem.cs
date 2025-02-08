using System.Collections.Generic;
using Fusion;
using System;
using UnityEngine;

public class TurnSystem : NetworkBehaviour
{
    [Networked][Capacity(4)] public NetworkLinkedList<PlayerRef> Players => default;
    [Networked] public int CurrentPlayerIndex { get; private set; }
    [Networked] public bool IsFinalRound { get; private set; }
    [Networked] public bool IsGameEnding { get; private set; }

    public void InitializeTurns(Dictionary<PlayerRef, BasePlayerInfo> gamePlayers)
    {
        Players.Clear();

        foreach (var gamePlayer in gamePlayers.Keys)
        {
            Players.Add(gamePlayer);
        }

        CurrentPlayerIndex = 0;
        IsFinalRound = false;
        StartTurn();
        Debug.Log("✅ TurnSystem initialized with players.");
    }
    private PlayerRef GetCurrentPlayer()
    {
        if (Players.Count == 0) return default;
        return Players[CurrentPlayerIndex];
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_EndTurn(PlayerRef playerRef)
    {
        if (!Object.HasStateAuthority) return;
        CheckForVictory(playerRef);
        Debug.Log($"🔹 Player {GetCurrentPlayer().PlayerId}'s turn end!");

        if (IsFinalRound && IsFinalRoundComplete())
        {
            Debug.Log("Final round complete.");
            return;
        }

        NextPlayer();
    }
    private void NextPlayer()
    {
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;

        if (CurrentPlayerIndex == 0 && IsFinalRound) Debug.Log("Final round ended.");

        StartTurn();
    }
    
    public void StartTurn()
    {
        PlayerRef currentPlayer = GetCurrentPlayer();
        if (currentPlayer == default) return;

        Debug.Log($"🔹 Player {currentPlayer.PlayerId}'s turn start!");
        if (Runner.LocalPlayer == currentPlayer)
        {
            FindFirstObjectByType<LocalBoardPlayer>().HandleInput();
        }
    }
    
    public void MarkFinalRound()
    {
        IsFinalRound = true;
        Debug.Log("Final round started.");
    }

    public bool IsFinalRoundComplete()
    {
        return IsFinalRound && CurrentPlayerIndex == 0;
    }
    
    public void CheckForVictory(PlayerRef currentPlayer)
    {
        var player = GameSystem.Instance.Players[currentPlayer];
        if (player == null) return;

        if (player.Score >= GameSystem.Instance.VictoryPoint && !IsGameEnding)
        {
            IsGameEnding = true;
            Debug.Log($"Player {currentPlayer.PlayerId} triggered final round!");
            MarkFinalRound();
        }

        if (IsGameEnding && IsFinalRoundComplete())
        {
            DetermineWinner();
        }
    }

    private void DetermineWinner()
    {
        BasePlayerInfo winner = null;
        int minCards = int.MaxValue;

        foreach (var playerDic in GameSystem.Instance.Players)
        {
            var player = playerDic.Value;
            if (player.Score > GameSystem.Instance.VictoryPoint ||
                (player.Score == GameSystem.Instance.VictoryPoint &&
                 player.OwnedCards.Length < minCards))
            {
                winner = player;
                minCards = player.OwnedCards.Length;
            }
        }

        if (winner != null)
        {
            Debug.Log($"Player {winner.PlayerRef.PlayerId} wins with {GameSystem.Instance.VictoryPoint} points!");
            GameSystem.Instance.OnGameEnded?.Invoke();
        }
    }
}
