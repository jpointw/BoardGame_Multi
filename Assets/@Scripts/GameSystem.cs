using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GameSystem : NetworkBehaviour
{
    public static GameSystem Instance;

    [SerializeField] private NetworkObject remoteBoardPlayerPrefab;

    public List<RemoteBoardPlayer> RemoteBoardPlayers { get; private set; } = new List<RemoteBoardPlayer>();
    public TurnSystem TurnSystem { get; private set; }
    public CoinSystem CoinSystem { get; private set; }
    public CardSystem CardSystem { get; private set; }

    private bool isGameEnding = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            // 하위 시스템 초기화
            TurnSystem = GetComponentInChildren<TurnSystem>();
            CoinSystem = GetComponentInChildren<CoinSystem>();
            CardSystem = GetComponentInChildren<CardSystem>();

            Debug.Log("GameSystem spawned as Host.");
        }
    }

    public void InitializeGame(List<PlayerRef> activePlayers)
    {
        if (!Object.HasStateAuthority) return;

        foreach (var playerRef in activePlayers)
        {
            SpawnRemoteBoardPlayer(playerRef);
        }

        TurnSystem.InitializeTurns(RemoteBoardPlayers); // TurnSystem 초기화
        CoinSystem.InitializeCentralCoins(); // CoinSystem 초기화
        Debug.Log("Game initialized.");
    }

    private void SpawnRemoteBoardPlayer(PlayerRef playerRef)
    {
        var playerObject = Runner.Spawn(remoteBoardPlayerPrefab, transform.position, Quaternion.identity, playerRef);
        var remotePlayer = playerObject.GetComponent<RemoteBoardPlayer>();

        remotePlayer.Initialize(playerRef);
        RemoteBoardPlayers.Add(remotePlayer);
    }

    public void CheckForVictory(PlayerRef currentPlayer)
    {
        if (!Object.HasStateAuthority) return;

        var player = RemoteBoardPlayers.Find(p => p.PlayerRef == currentPlayer);
        if (player == null) return;

        // 승리 조건 확인
        if (player.Score >= 15 && !isGameEnding)
        {
            isGameEnding = true;
            Debug.Log($"Player {currentPlayer.PlayerId} triggered final round!");
            TurnSystem.MarkFinalRound();
        }

        if (isGameEnding && TurnSystem.IsFinalRoundComplete())
        {
            DetermineWinner();
        }
    }

    private void DetermineWinner()
    {
        RemoteBoardPlayer winner = null;
        int maxScore = 0;
        int minCards = int.MaxValue;

        foreach (var player in RemoteBoardPlayers)
        {
            if (player.Score > maxScore || (player.Score == maxScore && player.OwnedCards.Length < minCards))
            {
                winner = player;
                maxScore = player.Score;
                minCards = player.OwnedCards.Length;
            }
        }

        if (winner != null)
        {
            Debug.Log($"Player {winner.PlayerRef.PlayerId} wins with {maxScore} points!");
            RPC_AnnounceWinner(winner.PlayerRef, maxScore);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_AnnounceWinner(PlayerRef winner, int score)
    {
        Debug.Log($"Player {winner.PlayerId} is the winner with {score} points!");
        // 클라이언트에서 승리 UI 표시
    }
}
