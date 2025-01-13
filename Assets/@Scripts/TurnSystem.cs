using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    private List<BasePlayer> players;
    private int currentPlayerIndex = 0;
    private bool isFinalRound = false;

    public void InitializeTurns(List<BasePlayer> gamePlayers)
    {
        players = gamePlayers;
        currentPlayerIndex = 0;
        Debug.Log("TurnSystem initialized with players.");
    }

    public void StartTurn()
    {
        if (players == null || players.Count == 0) return;

        var currentPlayer = players[currentPlayerIndex];
        Debug.Log($"Starting turn for Player {currentPlayer.PlayerRef.PlayerId}");

        if (currentPlayer is LocalBoardPlayer localPlayer)
        {
            localPlayer.HandleInput();
        }
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for Player {players[currentPlayerIndex].PlayerRef.PlayerId}");

        if (isFinalRound && IsFinalRoundComplete())
        {
            Debug.Log("Final round complete.");
            return;
        }

        NextPlayer();
    }

    private void NextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        if (currentPlayerIndex == 0 && isFinalRound)
        {
            Debug.Log("Final round ended.");
        }

        StartTurn();
    }

    public void MarkFinalRound()
    {
        isFinalRound = true;
        Debug.Log("Final round marked.");
    }

    public bool IsFinalRoundComplete()
    {
        return isFinalRound && currentPlayerIndex == 0;
    }
}