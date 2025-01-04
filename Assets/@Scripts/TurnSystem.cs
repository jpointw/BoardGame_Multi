using Fusion;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    [Networked] public PlayerRef CurrentPlayer { get; private set; }

    public void StartGame()
    {
        if (Object.HasStateAuthority)
        {
            // CurrentPlayer = Runner.ActivePlayers[0];
            Runner.ActivePlayers[0];
        }
    }

    // public void EndTurn()
    // {
    //     if (Object.HasStateAuthority)
    //     {
    //         int currentIndex = Runner.ActivePlayers.IndexOf(CurrentPlayer);
    //         CurrentPlayer = Runner.ActivePlayers[(currentIndex + 1) % Runner.ActivePlayers.Count];
    //         Debug.Log($"Next turn: Player {CurrentPlayer.PlayerId}");
    //     }
    // }
}