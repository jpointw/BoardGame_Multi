using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetworkSystem : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    public static NetworkSystem Instance { get; private set; }
    [SerializeField] private string sessionName = "DefaultRoom";
    [SerializeField] private int maxPlayers = 4;

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

    public void StartMatchmaking()
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        Debug.Log("Looking for existing sessions...");
        _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            PlayerCount = maxPlayers,
        }).ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
            {
                Debug.LogWarning("No sessions found. Creating a new session...");
                CreateRoom();
            }
        });
    }

    public void CreateRoom()
    {
        _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            PlayerCount = maxPlayers,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        }).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log("New session created.");
            else
                Debug.LogError("Failed to create session.");
        });
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (sessionList.Count > 0)
        {
            Debug.Log($"Found {sessionList.Count} sessions. Joining the first session...");
            _runner.JoinSessionLobby(SessionLobby.ClientServer).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("Joined session successfully.");
                else
                    Debug.LogError("Failed to join session.");
            });
        }
        else
        {
            Debug.Log("No sessions found. Creating a new session...");
            CreateRoom();
        }
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player joined with PlayerRef ID: {player.PlayerId}");
        // GameSystem.Instance?.InitializePlayer(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player left with PlayerRef ID: {player.PlayerId}");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Game shutdown: {shutdownReason}");
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
}
