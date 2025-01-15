using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BasePlayer : NetworkBehaviour
{
    public PlayerRef PlayerRef { get; protected set; }

    [Networked,OnChangedRender(nameof(OnScoreChanged))]
    public int Score { get; protected set; }

    [Networked,OnChangedRender(nameof(OnCoinsChanged))]
    [Capacity(6)]
    public NetworkArray<int> OwnedCoins { get; } = MakeInitializer(new int[6]);

    [Networked,OnChangedRender(nameof(OnCardsChanged))]
    [Capacity(6)]
    public NetworkArray<int> OwnedCards { get; } = MakeInitializer(new int[6]);
    
    [Networked,OnChangedRender(nameof(OnReservedCardsChanged))]
    [Capacity(3)]
    public NetworkArray<int> ReservedCards { get; } = 
        MakeInitializer(new int[3] {-1, -1, -1});

    [SerializeField] protected TMP_Text playerScoreText;
    [SerializeField] protected Image[] playerCardsImages;
    [SerializeField] protected TMP_Text[] playerCardsTexts;
    [SerializeField] protected Image[] playerCoinsImages;
    [SerializeField] protected TMP_Text[] playerCoinsTexts;
    
    [SerializeField] protected Image[] playerReservedCardsImages;
    
    public virtual void Initialize(PlayerRef playerRef)
    {
        PlayerRef = playerRef;
    }

    public virtual void ModifyScore(int amount)
    {
        if (!Object.HasStateAuthority) return;
        Score += amount;
    }

    public virtual void ModifyCoins(int[] coinChanges)
    {
        if (!Object.HasStateAuthority) return;

        for (int i = 0; i < coinChanges.Length; i++)
        {
            OwnedCoins.Set(i, Mathf.Max(0, OwnedCoins[i] + coinChanges[i]));
        }
    }
    
    public virtual void ModifyCoin(int coinType,int coinChanges)
    {
        if (!Object.HasStateAuthority) return;

        OwnedCoins.Set(coinType, Mathf.Max(0, OwnedCoins[coinType] + coinChanges));
    }

    public virtual void AddCard(int cardType)
    {
        if (!Object.HasStateAuthority) return;

        OwnedCards.Set(cardType, OwnedCards[cardType] + 1);
    }

    public virtual void AddReservedeCard(int cardId)
    {
        for (int i = 0; i < ReservedCards.Length; i++)
        {
            if (ReservedCards[i] == -1)
            {
                ReservedCards.Set(i, cardId);
            }
        }
    }

    public virtual void RemoteReservedeCard(int cardId)
    {
        for (int i = 0; i < ReservedCards.Length; i++)
        {
            if (ReservedCards[i] == cardId)
            {
                ReservedCards.Set(i, -1);
            }
        }
    }

    public void OnScoreChanged()
    {
        UpdateScoreUI();
    }

    public void OnCoinsChanged()
    {
        UpdateCoinsUI();
    }

    public void OnCardsChanged()
    {
        UpdateCardsUI();
    }

    public void OnReservedCardsChanged()
    {
        UpdateReservedCardsUI();
    }

    public virtual void UpdateUI()
    {
        UpdateScoreUI();
        UpdateCoinsUI();
        UpdateCardsUI();
    }

    protected virtual void UpdateScoreUI()
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = Score.ToString();
        }
    }

    protected virtual void UpdateCoinsUI()
    {
        for (int i = 0; i < playerCoinsTexts.Length; i++)
        {
            if (i < OwnedCoins.Length)
            {
                playerCoinsTexts[i].text = OwnedCoins[i].ToString();
                playerCoinsImages[i].enabled = OwnedCoins[i] > 0;
            }
            else
            {
                playerCoinsImages[i].enabled = false;
            }
        }
    }

    protected virtual void UpdateCardsUI()
    {
        for (int i = 0; i < playerCardsTexts.Length; i++)
        {
            if (i < OwnedCards.Length && OwnedCards[i] != 0)
            {
                playerCardsTexts[i].text = OwnedCards[i].ToString();
                playerCardsImages[i].enabled = true;
            }
            else
            {
                playerCardsTexts[i].text = "";
                playerCardsImages[i].enabled = false;
            }
        }
    }

    protected virtual void UpdateReservedCardsUI()
    {
        for (int i = 0; i < ReservedCards.Length; i++)
        {
            if (ReservedCards[i] != -1)
            {
                playerReservedCardsImages[i].enabled = true;
                
            }
        }
    }
}
