using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UIDataBase", menuName = "Scriptable Objects/UIDataBase")]
public class UIDataBase : ScriptableObject
{
    public static UIDataBase Instance;
    
    public Sprite[] coinSprites;
    public Color[] ownAssetColors;

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = Resources.Load<UIDataBase>("UIDataBase");
        }
        else
        {
            Destroy(this);
        }
    }

}
