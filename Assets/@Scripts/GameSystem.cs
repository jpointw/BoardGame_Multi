using System;
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
    
    public InGameUI InGameUI { get; private set; }

    private bool isGameEnding = false;
    
    /// <summary>
    /// Actions
    /// </summary>
    public Action<int[]> OnCoinChanged;
    public Action OnGameEnd;
    public Action OnCardChanged;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            TurnSystem = GetComponentInChildren<TurnSystem>();
            CoinSystem = GetComponentInChildren<CoinSystem>();
            CardSystem = GetComponentInChildren<CardSystem>();
            InGameUI = FindFirstObjectByType<InGameUI>();

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

        TurnSystem.InitializeTurns(RemoteBoardPlayers);
        CoinSystem.InitializeCentralCoins();
        InGameUI.InitializeUI();
        Debug.Log("Game initialized.");
    }

    private void SpawnRemoteBoardPlayer(PlayerRef playerRef)
    {
        var playerObject = Runner.Spawn(remoteBoardPlayerPrefab, transform.position, Quaternion.identity, playerRef);
        var remotePlayer = playerObject.GetComponent<RemoteBoardPlayer>();

        remotePlayer.Initialize(playerRef);
        RemoteBoardPlayers.Add(remotePlayer);
    }

    public void ModifyCentralCoins(int[] coinChanges)
    {
        for (int i = 0; i < coinChanges.Length; i++)
        {
            int newAmount = Mathf.Max(0, CoinSystem.CentralCoins[i] + coinChanges[i]);
            CoinSystem.CentralCoins.Set(i, newAmount);
        }

        Debug.Log($"CentralCoins updated: {string.Join(", ", CoinSystem.CentralCoins)}");
        RPC_UpdateCentralCoins(coinChanges);
        OnCoinChanged.Invoke(coinChanges);
    }

    public void HandlePurchaseRequest(PlayerRef playerRef, CardInfo card)
    {
        if (!Object.HasStateAuthority) return;

        int[] coinChanges = new int[6];
        for (int i = 0; i < coinChanges.Length; i++)
        {
            coinChanges[i] = -card.cost[i];
        }

        ModifyCentralCoins(coinChanges);

        // Update player state
        var player = RemoteBoardPlayers.Find(p => p.PlayerRef == playerRef);
        if (player != null)
        {
            player.AddCard(card.uniqueId);
            player.ModifyScore(card.points);
        }

        Debug.Log($"Player {playerRef.PlayerId} purchased card {card.uniqueId}.");

        RPC_SyncCardPurchase(playerRef, card.uniqueId, card.points);
    }

    public void CheckForVictory(PlayerRef currentPlayer)
    {
        if (!Object.HasStateAuthority) return;

        var player = RemoteBoardPlayers.Find(p => p.PlayerRef == currentPlayer);
        if (player == null) return;

        // Check victory conditions
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
        // Display victory UI on clients
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestPurchaseCard(PlayerRef playerRef, int cardId)
    {
        if (!Object.HasStateAuthority) return;

        var card = CardSystem.GetCardInfo(cardId);
        HandlePurchaseRequest(playerRef, card);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SyncCardPurchase(PlayerRef playerRef, int cardId, int points)
    {
        var player = RemoteBoardPlayers.Find(p => p.PlayerRef == playerRef);
        if (player != null)
        {
            player.AddCard(cardId);
            player.ModifyScore(points);
            player.UpdateUI();
        }

        Debug.Log($"[Client] Player {playerRef.PlayerId} purchased card {cardId}.");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateCentralCoins(int[] coinChanges)
    {
        for (int i = 0; i < coinChanges.Length; i++)
        {
            int newAmount = Mathf.Max(0, CoinSystem.CentralCoins[i] + coinChanges[i]);
            CoinSystem.CentralCoins.Set(i, newAmount);
        }

        Debug.Log($"[Client] Central Coins updated: {string.Join(", ", CoinSystem.CentralCoins)}");
        // Update client-side UI
    }
}
