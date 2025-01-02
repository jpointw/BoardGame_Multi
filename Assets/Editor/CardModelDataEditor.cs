using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Define;

#if UNITY_EDITOR
[CustomEditor(typeof(CardModelData))]
public class CardModelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CardModelData scriptableObject = (CardModelData)target;

        if (GUILayout.Button("Import CardData CSV"))
        {
            string csvFilePath = Path.Combine(Application.dataPath, "@Resources/CardDataCsv.csv");

            if (!File.Exists(csvFilePath))
            {
                Debug.LogError("CSV file not found in Resources folder. Make sure 'CardDataCsv.csv' exists in the Resources folder.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(csvFilePath);
                CardInfo[] newCardInfos = new CardInfo[lines.Length - 1];

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(',');

                    if (values.Length < 5)
                    {
                        Debug.LogError($"Invalid data format on line {i + 1}: Expected 5+ values but found {values.Length}.");
                        continue;
                    }

                    try
                    {
                        CardInfo cardInfo = new CardInfo
                        {
                            CardLevel = SafeParseInt(values[0], "CardLevel", i + 1),
                            CardType = SafeParseEnum<CardType>(values[1], "CardType", i + 1),
                            Points = SafeParseInt(values[2], "Points", i + 1),
                            Cost = ParsePrice(values[3], i + 1),
                            Illustration = values.Length > 4 ? values[4].Trim() : ""
                        };

                        newCardInfos[i - 1] = cardInfo;
                    }
                    catch (Exception innerEx)
                    {
                        Debug.LogError($"Failed to parse line {i + 1}: {innerEx.Message}");
                    }
                }

                scriptableObject.cardInfos = newCardInfos;

                EditorUtility.SetDirty(scriptableObject);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("CSV data successfully imported from Resources folder into the ScriptableObject.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to import CSV: {ex.Message}");
            }
        }
    }

    private int SafeParseInt(string value, string fieldName, int lineIndex)
    {
        if (int.TryParse(value.Trim(), out int result))
        {
            return result;
        }

        Debug.LogError($"Invalid integer value for {fieldName} on line {lineIndex}: '{value}'. Defaulting to 0.");
        return 0;
    }

    private T SafeParseEnum<T>(string value, string fieldName, int lineIndex) where T : struct
    {
        if (Enum.TryParse(value.Trim(), true, out T result))
        {
            return result;
        }

        Debug.LogError($"Invalid enum value for {fieldName} on line {lineIndex}: '{value}'. Defaulting to first enum value.");
        return default;
    }

    private CardCost ParsePrice(string price, int lineIndex)
    {
        try
        {
            var cost = new CardCost();
            string[] parts = price.Split('+');

            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                if (trimmedPart.EndsWith("w"))
                    cost.White = SafeParseInt(trimmedPart.TrimEnd('w'), "Cost_White", lineIndex);
                else if (trimmedPart.EndsWith("u"))
                    cost.Blue = SafeParseInt(trimmedPart.TrimEnd('u'), "Cost_Blue", lineIndex);
                else if (trimmedPart.EndsWith("g"))
                    cost.Green = SafeParseInt(trimmedPart.TrimEnd('g'), "Cost_Green", lineIndex);
                else if (trimmedPart.EndsWith("r"))
                    cost.Red = SafeParseInt(trimmedPart.TrimEnd('r'), "Cost_Red", lineIndex);
                else if (trimmedPart.EndsWith("k"))
                    cost.Black = SafeParseInt(trimmedPart.TrimEnd('k'), "Cost_Black", lineIndex);
                else
                    Debug.LogWarning($"Unrecognized cost format '{trimmedPart}' on line {lineIndex}");
            }

            return cost;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse Price on line {lineIndex}: {ex.Message}");
            return new CardCost();
        }
    }
}
#endif
