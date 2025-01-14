using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.tvOS;

public class GameSystem : NetworkBehaviour
{
    public static GameSystem Instance;

    [SerializeField] private LocalBoardPlayer localPlayerPrefab;
    [SerializeField] private RemoteBoardPlayer remotePlayerPrefab;

    public List<BasePlayer> Players { get; private set; } = new List<BasePlayer>();
    public TurnSystem TurnSystem { get; private set; }
    public CoinSystem CoinSystem { get; private set; }
    public CardSystem CardSystem { get; private set; }
    public InGameUI InGameUI { get; private set; }
    
    
    public int VictoryPoint { get; private set; }

    private bool isGameEnding = false;

    public Action OnGameStarted;
    public Action<PlayerRef> OnTurnEnded;
    public Action OnGameEnded;
    public Action<PlayerRef, int> OnCardChanged;
    public Action<int[]> OnCoinChanged;

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
            if (Runner.LocalPlayer == playerRef)
                SpawnLocalPlayer(playerRef);
            else
                SpawnRemotePlayer(playerRef);
        }

        TurnSystem.InitializeTurns(Players);
        CoinSystem.InitializeCentralCoins();
        InGameUI.InitializeUI();
        OnGameStarted?.Invoke();
        Debug.Log("Game initialized.");
    }

    private void SpawnLocalPlayer(PlayerRef playerRef)
    {
        var playerObject = Runner.Spawn(localPlayerPrefab, transform.position, Quaternion.identity, playerRef);
        var player = playerObject.GetComponent<LocalBoardPlayer>();
        player.Initialize(playerRef);
        Players.Add(player);
    }

    private void SpawnRemotePlayer(PlayerRef playerRef)
    {
        var playerObject = Runner.Spawn(remotePlayerPrefab, transform.position, Quaternion.identity, playerRef);
        var player = playerObject.GetComponent<RemoteBoardPlayer>();
        player.Initialize(playerRef);
        Players.Add(player);
    }

    public void ModifyCentralCoins(int[] coinChanges)
    {
        for (int i = 0; i < coinChanges.Length; i++)
        {
            int newAmount = Mathf.Max(0, CoinSystem.CentralCoins[i] + coinChanges[i]);
            CoinSystem.CentralCoins.Set(i, newAmount);
        }
        OnCoinChanged?.Invoke(coinChanges);
    }

    public int CanPlayerPurchaseCard(PlayerRef playerRef, CardInfo card)
    {
        var player = Players.Find(p => p.PlayerRef == playerRef);
        if (player == null)
        {
            Debug.LogError($"Player {playerRef.PlayerId} not found.");
            return -1;
        }

        int requiredSpecialCoins = 0;

        for (int i = 0; i < card.cost.Length; i++)
        {
            requiredSpecialCoins += Math.Max(0, card.cost[i] - player.OwnedCoins[i]);
        }

        return requiredSpecialCoins;
    }

    public void HandlePurchaseRequest(PlayerRef playerRef, CardInfo card)
    {
        if (!Object.HasStateAuthority) return;

        int requiredSpecialCoins = CanPlayerPurchaseCard(playerRef, card);
        if (requiredSpecialCoins < 0)
        {
            Debug.Log($"Player {playerRef.PlayerId} cannot purchase card {card.uniqueId}: Validation failed.");
            return;
        }

        var player = Players.Find(p => p.PlayerRef == playerRef);
        if (player != null)
        {
            if (requiredSpecialCoins > player.OwnedCoins.Last())
            {
                Debug.Log($"Player {playerRef.PlayerId} does not have enough SpecialCoins to purchase card {card.uniqueId}.");
                return;
            }

            player.AddCard(card.cardType);
            player.ModifyScore(card.points);

            var coinChanges = new int[6];
            for (int i = 0; i < card.cost.Length; i++)
            {
                coinChanges[i] = -card.cost[i];
            }
            coinChanges[5] -= requiredSpecialCoins;
            player.ModifyCoins(coinChanges);
            ModifyCentralCoins(coinChanges);
        }

        Debug.Log($"Player {playerRef.PlayerId} purchased card {card.uniqueId}.");
        OnCardChanged?.Invoke(playerRef, card.cardType);
    }

    public void HandleRserveCardRequest(PlayerRef playerRef, CardInfo card)
    {
        if (!Object.HasStateAuthority) return;
        
        var player = Players.Find(p => p.PlayerRef == playerRef);
        if (player != null)
        {
            player.AddReservedeCard(card.uniqueId);
        }
    }

    public void EndTurn(PlayerRef playerRef)
    {
        CheckForVictory(playerRef);
        TurnSystem.EndTurn();
    }

    public void CheckForVictory(PlayerRef currentPlayer)
    {
        if (!Object.HasStateAuthority) return;

        var player = Players.Find(p => p.PlayerRef == currentPlayer);
        if (player == null) return;

        if (player.Score >= VictoryPoint && !isGameEnding)
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
        BasePlayer winner = null;
        int minCards = int.MaxValue;

        foreach (var player in Players)
        {
            if (player.Score > VictoryPoint || (player.Score == VictoryPoint && player.OwnedCards.Length < minCards))
            {
                winner = player;
                VictoryPoint = player.Score;
                minCards = player.OwnedCards.Length;
            }
        }

        if (winner != null)
        {
            Debug.Log($"Player {winner.PlayerRef.PlayerId} wins with {VictoryPoint} points!");
            OnGameEnded?.Invoke();
        }
    }


    // #region RPC Method
    //
    // [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    // public void RPC_RequestPurchaseCard(PlayerRef playerRef, int cardId)
    // {
    //     if (!Object.HasStateAuthority) return;
    //
    //     var card = CardSystem.GetCardInfo(cardId);
    //     HandlePurchaseRequest(playerRef, card);
    // }
    //
    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // private void RPC_SyncCardPurchase(PlayerRef playerRef, int cardId, int points)
    // {
    //     var player = Players.Find(p => p.PlayerRef == playerRef);
    //     if (player != null)
    //     {
    //         player.AddCard(cardId);
    //         player.ModifyScore(points);
    //         player.UpdateUI();
    //     }
    //
    //     OnCardChanged?.Invoke();
    //
    //     Debug.Log($"[Client] Player {playerRef.PlayerId} purchased card {cardId}.");
    // }
    //
    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // private void RPC_UpdateCentralCoins(int[] coinChanges)
    // {
    //     for (int i = 0; i < coinChanges.Length; i++)
    //     {
    //         int newAmount = Mathf.Max(0, CoinSystem.CentralCoins[i] + coinChanges[i]);
    //         CoinSystem.CentralCoins.Set(i, newAmount);
    //     }
    //
    //     OnCoinChanged?.Invoke(coinChanges);
    //
    //     Debug.Log($"[Client] Central Coins updated: {string.Join(", ", CoinSystem.CentralCoins.ToArray())}");
    // }
    //
    // #endregion
}
