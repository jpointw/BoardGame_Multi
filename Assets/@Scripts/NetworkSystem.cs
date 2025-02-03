using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSystem : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner Runner {get; private set; }
    private bool isHost = false;
    
    public GameSystem gameSystem;

    public static NetworkSystem Instance { get; private set; }
    [SerializeField] private string sessionName = "default";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public async void CreateRoom()
    {
        Runner ??= gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;

        Dictionary<string, SessionProperty> roomProperties = new Dictionary<string, SessionProperty>
        {
            { "PlayerCount", GameSharedData.PlayerCount },
            { "VictoryPoints", GameSharedData.GameVictoryPoints },
            { "TurnTime", GameSharedData.PlayerTurnTime }
        };

        var startArgs = new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            PlayerCount = GameSharedData.PlayerCount,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            SessionProperties = roomProperties
        };

        var result = await Runner.StartGame(startArgs);

        if (result.Ok)
        {
            isHost = true;
            Debug.Log($"✅ Room created successfully with max players: {GameSharedData.PlayerCount}");
        }
        else
        {
            Debug.LogError($"❌ Room creation failed: {result.ErrorMessage}");
        }
    }

    public async void FindRoom()
    {
        Runner ??= gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;

        var startArgs = new StartGameArgs
        {
            GameMode = GameMode.Client,
        };

        var result = await Runner.StartGame(startArgs);

        if (result.Ok)
        {
            Debug.Log($"✅ Joined room successfully.");
        }
        else
        {
            Debug.LogError($"❌ Room join failed: {result.ErrorMessage}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"👤 Player {player.PlayerId} joined.");

        if (isHost)
        {
            CheckPlayerCount();
        }
        else
        {
            SyncGameSettings();
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"🚪 Player {player.PlayerId} left.");
    }

    private void CheckPlayerCount()
    {
        if (isHost && Runner.ActivePlayers.Count() == GameSharedData.PlayerCount)
        {
            Debug.Log("🎮 All players are connected, starting the game.");
            StartGame();
        }
    }

    private async void StartGame()
    {
        if (Runner.IsSceneAuthority)
        {
            Debug.Log("🚀 Loading Game Scene...");
            await Runner.LoadScene("GameScene", LoadSceneMode.Single);
            Runner.Spawn(gameSystem);
        }
    }

    private void SyncGameSettings()
    {
        if (!isHost)
        {
            GameSharedData.PlayerCount = Runner.SessionInfo.Properties["PlayerCount"];
            GameSharedData.GameVictoryPoints = Runner.SessionInfo.Properties["VictoryPoints"];
            GameSharedData.PlayerTurnTime = Runner.SessionInfo.Properties["TurnTime"];

            Debug.Log($"🔄 Synchronized game settings: Players {GameSharedData.PlayerCount}, Victory {GameSharedData.GameVictoryPoints}, TurnTime {GameSharedData.PlayerTurnTime}");
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"❗ Game ended: {shutdownReason}");
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
