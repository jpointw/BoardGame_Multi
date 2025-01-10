using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UIDataBase", menuName = "Scriptable Objects/UIDataBase")]
public class UIDataBase : ScriptableObject
{
    public static UIDataBase instance;
    
    public Sprite[] coinSprites;
    public Color[] ownAssetColors;

    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

}
