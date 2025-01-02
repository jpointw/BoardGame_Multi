using System;
using UnityEngine;
using static Define;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public CardInfo[] cardInfos;
}

[Serializable]
public class CardInfo
{
    public CardType CardType;
    public int Level;  
    public int Points; 
    public int[] Cost;
}