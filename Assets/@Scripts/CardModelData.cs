using System;
using UnityEngine;
using static Define;

[CreateAssetMenu(fileName = "CardModelData", menuName = "Scriptable Objects/CardModelData")]
public class CardModelData : ScriptableObject
{
    public CardInfo[] cardInfos;
}

[Serializable]
public class CardInfo
{
    public CardType CardType;  
    public int CardLevel;
    public int Points;         
    public CardCost Cost;      
    public string Illustration;
}

[Serializable]
public class CardCost
{
    public int White;
    public int Blue; 
    public int Green;
    public int Red;  
    public int Black;
}


