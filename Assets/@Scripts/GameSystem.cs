using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.LagCompensation;
using ParrelSync;
using UnityEngine;
using UnityEngine.tvOS;

public class GameSystem : NetworkBehaviour
{
    public static GameSystem Instance;

    [SerializeField] private BasePlayerInfo basePlayerInfoPrefab;
    [SerializeField] private LocalBoardPlayer localPlayerPrefab;
    [SerializeField] private RemoteBoardPlayer remotePlayerPrefab;

    public Dictionary<PlayerRef, BasePlayerInfo> Players = new Dictionary<PlayerRef, BasePlayerInfo>();
   
    public TurnSystem TurnSystem { get; private set; }
    public CoinSystem CoinSystem { get; private set; }
    public CardSystem CardSystem { get; private set; }
    public InGameUI InGameUI { get; private set; }
    
    
    [Networked] public int PlayerCount { get; private set; }
    [Networked] public int VictoryPoint { get; private set; }
    [Networked] public int PlayerTurnTime { get; private set; }
    
    private bool isGameEnding = false;

    public Action OnGameStarted;
    public Action<string> OnGameEnded;
    
    public GameObject playerInfoContainer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void Spawned()
    {
        TurnSystem ??= GetComponentInChildren<TurnSystem>();
        CoinSystem ??= GetComponentInChildren<CoinSystem>();
        CardSystem ??= GetComponentInChildren<CardSystem>();
        if (Object.HasStateAuthority)
        {

            PlayerCount = GameSharedData.PlayerCount;
            VictoryPoint = GameSharedData.GameVictoryPoints;
            PlayerTurnTime = GameSharedData.PlayerTurnTime;
            Debug.Log("GameSystem spawned as Host.");
        }
        InGameUI ??= FindFirstObjectByType<InGameUI>();
        
        InitializeGame();
    }

    public void InitializeGame()
    {
        InitializePlayers();
        if (Object.HasStateAuthority)
        {
            CoinSystem.InitializeCentralCoins();
            CardSystem.InitializeDecks();
            OnGameStarted?.Invoke();
        }
        InGameUI.InitializeUI();


        if (Object.HasStateAuthority) TurnSystem.InitializeTurns(Players);
        VictoryPoint = GameSharedData.GameVictoryPoints;
        
        Debug.Log("Game initialized.");
    }
    
    private void InitializePlayers()
    {
        if (Object.HasStateAuthority)
        {
            foreach (var player in Runner.ActivePlayers)
            {
                var playerInfo = Runner.Spawn(basePlayerInfoPrefab, transform.position, Quaternion.identity);
                Players.TryAdd(player, playerInfo);
                playerInfo.Initialize(player);
            }
        }
        if (!Object.HasStateAuthority)
        {
            Players.Clear();
    
            foreach (var playerInfo in FindObjectsByType<BasePlayerInfo>(FindObjectsSortMode.InstanceID))
            {
                if (playerInfo != null) Players.TryAdd(playerInfo.PlayerRef, playerInfo);
            }
        }
    }

    public void ModifyCentralCoins(int[] coinChanges)
    {
        CoinSystem.ModifyCentralCoins(coinChanges);
    }

    public int CanPlayerPurchaseCard(PlayerRef playerRef, CardInfo card)
    {
        var player = Players[playerRef];
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

    public void HandlePurchaseRequest(PlayerRef playerRef, CardInfo card, bool isReserved = false)
    {
        if (!Object.HasStateAuthority) return;
        
        var requiredSpecialCoins = CanPlayerPurchaseCard(playerRef, card);
        
        var player = Players[playerRef];
        if (player != null)
        {

            player.AddCard(card.cardType);
            player.ModifyScore(card.points);

            var coinChangesForPlayer = new int[6];
            var coinChangesForServer = new int[6];
            
            for (int i = 0; i < card.cost.Length; i++)
            {
                coinChangesForPlayer[i] = -card.cost[i];
                coinChangesForServer[i] = card.cost[i];
            }
            coinChangesForPlayer[5] = -requiredSpecialCoins;
            coinChangesForServer[5] = requiredSpecialCoins;
            player.ModifyCoins(coinChangesForPlayer);
            ModifyCentralCoins(coinChangesForServer);
        }

        if (isReserved)
        {
            player.RemoveReservedCard(card.uniqueId);
        }
        else
        {
            CardSystem.RemoveCardFromField(card);
        }
        
        Debug.Log($"Player {playerRef.PlayerId} purchased card {card.uniqueId}.");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ChangeName(PlayerRef playerRef, string name)
    {
        if (!Object.HasStateAuthority) return;
        Players[playerRef].Name = name;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_HandlePurchaseRequest(PlayerRef playerRef, int cardId, bool isReserved = false)
    {
        if (!Object.HasStateAuthority) return;
        var card = CardModelData.Instance.GetCardInfoById(cardId);
        HandlePurchaseRequest(playerRef, card, isReserved);

    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_HandleTakeCoins(PlayerRef playerRef, int[] selectedCoins)
    {
        if (!Object.HasStateAuthority) return;

        var player = Players[playerRef];
        if (player == null)
        {
            Debug.LogError($"Player {playerRef.PlayerId} not found.");
            return;
        }

        int[] coinChanges = new int[6];
        for (int i = 0; i < selectedCoins.Length; i++)
        {
            coinChanges[i] = -selectedCoins[i];
        }

        ModifyCentralCoins(coinChanges);
        // OnCoinChanged?.Invoke(CoinSystem.CentralCoins.ToArray(), playerRef, false);
        player.ModifyCoins(selectedCoins);

    }
    
    public void HandleReserveCardRequest(PlayerRef playerRef, CardInfo card)
    {
        if (!Object.HasStateAuthority) return;
        
        var player = Players[playerRef];
        if (player != null)
        {
            player.AddReservedCard(card.uniqueId);
            CardSystem.RemoveCardFromField(card);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_HandleReserveCardRequest(PlayerRef playerRef, int cardId)
    {
        if (!Object.HasStateAuthority) return;

        var card = CardModelData.Instance.GetCardInfoById(cardId);
        HandleReserveCardRequest(playerRef, card);

    }

    public void EndTurn(PlayerRef playerRef)
    {
        TurnSystem.RPC_EndTurn(playerRef);
    }
}
