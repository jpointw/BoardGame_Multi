using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class TurnSystem : NetworkBehaviour
{
    public static TurnSystem Instance;

    [Networked][Capacity(1)] public PlayerRef CurrentPlayer { get; private set; }
    private List<PlayerRef> PlayerOrder = new List<PlayerRef>();

    public void InitializeTurns(List<RemoteBoardPlayer> remotePlayers)
    {
        if (!Object.HasStateAuthority) return;

        PlayerOrder.Clear();

        foreach (var player in remotePlayers)
        {
            PlayerOrder.Add(player.PlayerRef);
        }

        if (PlayerOrder.Count > 0)
        {
            CurrentPlayer = PlayerOrder[0];
        }

        Debug.Log("Turns initialized.");
    }

    public void EndTurn()
    {
        if (!Object.HasStateAuthority) return;

        GameSystem.Instance.CheckForVictory(CurrentPlayer);

        int currentIndex = PlayerOrder.IndexOf(CurrentPlayer);
        int nextIndex = (currentIndex + 1) % PlayerOrder.Count;

        CurrentPlayer = PlayerOrder[nextIndex];
    }

    public void MarkFinalRound()
    {
        Debug.Log("Final round marked.");
    }

    public bool IsFinalRoundComplete()
    {
        return true;
    }
}