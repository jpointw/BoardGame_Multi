using System;
using System.Linq;
using System.Xml;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using static Define;

[CreateAssetMenu(fileName = "CardModelData", menuName = "Scriptable Objects/CardModelData")]
public class CardModelData : ScriptableObject
{
    public static CardModelData Instance;
    
    public CardInfo[] cardInfos;
    public SpecialCardInfo[] specialCardInfos;

    public CardInfo GetCardInfoById(int id)
    {
        return cardInfos.FirstOrDefault(p => p.uniqueId == id);
    }
    
    public SpecialCardInfo GetSpecialCardInfoById(int id)
    {
        return specialCardInfos.FirstOrDefault(p => p.uniqueId == id);
    }

    public CardInfo[] GetCardsArrayByLevel(int cardLevel)
    {
        return cardInfos.Where(p => p.cardLevel == cardLevel).ToArray();
    }

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = Resources.Load<CardModelData>("CardData");
            Debug.LogError("Initialized CardModelData");
        }
        else
        {
            Destroy(this);
        }
    }
}

[Serializable]
public struct CardInfo
{
    public int uniqueId;
    public int cardType;
    public int cardLevel;
    public int points;         
    public int[] cost;
    public string illustration;
}

[Serializable]
public struct SpecialCardInfo
{
    public int uniqueId;
    public int points;
    public int[] cost;
    public string illustration;
}


