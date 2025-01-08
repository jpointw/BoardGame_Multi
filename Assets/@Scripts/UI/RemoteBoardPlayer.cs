using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Define;

public class RemoteBoardPlayer : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text playerScoreText;

    public Image[] ownCardImages;
    public TMP_Text ownCardAmountText;
    public Image[] ownCoinImages;
    public TMP_Text ownCoinAmountText;
    
    public List<int> ownCards = new List<int>();
    public Dictionary<int, int> OwnCoinsDictionary = new Dictionary<int, int>();

    public void InitializePlayer(PlayerRef playerRef)
    {
        
    }

    public void UpdateOwnCardsList(CardInfo cardInfo)
    {
        
    }

    public void UpdateOwnCoinsDictionary(int[] coinsArray)
    {
        
    }
}
