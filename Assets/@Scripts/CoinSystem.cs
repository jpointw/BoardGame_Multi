// using Fusion;
// using System.Collections.Generic;
// using UnityEngine;
// using static Define;
//
// public class CoinSystem : NetworkBehaviour
// {
//     // 코인의 종류와 개수를 동기화
//     [Networked] public NetworkDictionary<CoinType, int> Coins { get; private set; }
//
//     private void Awake()
//     {
//     }
//
//     public void InitializeCoins()
//     {
//         if (Object.HasStateAuthority)
//         {
//             Coins.Clear();
//
//             // 초기 코인 설정 (예: 7개씩)
//             foreach (CoinType type in System.Enum.GetValues(typeof(CoinType)))
//             {
//                 Coins.Add(type, 7);
//             }
//
//             Debug.Log("Coins initialized.");
//         }
//     }
//
//     public void TakeCoins(PlayerSystem player, Dictionary<CoinType, int> requestedCoins)
//     {
//         if (!Object.HasStateAuthority) return;
//
//         foreach (var coin in requestedCoins)
//         {
//             if (!Coins.ContainsKey(coin.Key)) continue;
//
//             int currentAmount = Coins[coin.Key];
//             if (currentAmount >= coin.Value)
//             {
//                 // 코인 개수 감소
//                 Coins[coin.Key] = currentAmount - coin.Value;
//
//                 // 플레이어에게 코인 추가
//                 player.AddCoins(coin.Key, coin.Value);
//
//                 Debug.Log($"Player {player.PlayerRef.PlayerId} took {coin.Value} {coin.Key} coins.");
//             }
//             else
//             {
//                 Debug.LogWarning($"Not enough {coin.Key} coins available.");
//             }
//         }
//     }
//
//     public void ReturnCoins(PlayerSystem player, Dictionary<CoinType, int> returnedCoins)
//     {
//         if (!Object.HasStateAuthority) return;
//
//         foreach (var coin in returnedCoins)
//         {
//             if (!Coins.ContainsKey(coin.Key)) continue;
//
//             // 코인 개수 증가
//             Coins[coin.Key] += coin.Value;
//
//             // 플레이어에게서 코인 제거
//             player.RemoveCoins(coin.Key, coin.Value);
//
//             Debug.Log($"Player {player.PlayerRef.PlayerId} returned {coin.Value} {coin.Key} coins.");
//         }
//     }
// }
