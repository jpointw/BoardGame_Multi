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
    public CardInfo[] cardInfos;
    public SpecialCardInfo[] specialCardInfos;

    public CardInfo GetCardInfo(int id)
    {
        return cardInfos.FirstOrDefault(p => p.uniqueId == id);
    }

    public CardInfo[] Get1LevelCardInfos()
    {
        return cardInfos.Where(p => p.cardLevel == 0).ToArray();
    }
    
    public CardInfo[] Get2LevelCardInfos()
    {
        return cardInfos.Where(p => p.cardLevel == 1).ToArray();
    }
    
    public CardInfo[] Get3LevelCardInfos()
    {
        return cardInfos.Where(p => p.cardLevel == 2).ToArray();
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


