using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIDataBase", menuName = "Scriptable Objects/UIDataBase")]
public class UIDataBase : ScriptableObject
{
    public static UIDataBase Instance;
    
    public Sprite[] coinSprites;
    public Color[] ownAssetColors;
    public Sprite[] cardSprites;
    public Dictionary<string, Sprite> CardSpriteDictionary = new Dictionary<string, Sprite>();

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = Resources.Load<UIDataBase>("UIDataBase");

            foreach (var sprite in cardSprites)
            {
                CardSpriteDictionary.TryAdd(sprite.name, sprite);
                Debug.LogError(sprite.name);
                
            }
        }
        else
        {
            Destroy(this);
        }
    }


    public Sprite GetSprite(string spriteName)
    {
        return CardSpriteDictionary[spriteName];
    }
}
