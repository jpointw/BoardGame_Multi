using System;
using System.IO;
using Fusion;
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
                            cardLevel = SafeParseInt(values[0], "CardLevel", i + 1),
                            cardType = SafeParseInt(values[1], "CardType", i + 1),
                            points = SafeParseInt(values[2], "Points", i + 1),
                            cost = ParsePrice(values[3], i + 1),
                            illustration = values.Length > 4 ? values[4].Trim() : ""
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

    private int[] ParsePrice(string price, int lineIndex)
    {
        try
        {
            int[] cost = new int[5];
            string[] parts = price.Split('+');

            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();

                if (string.IsNullOrEmpty(trimmedPart))
                {
                    Debug.LogWarning($"Empty cost segment on line {lineIndex}.");
                    continue;
                }

                char costType = trimmedPart[^1]; // Get the last character
                string costValueStr = trimmedPart[..^1]; // Get everything except the last character

                if (!int.TryParse(costValueStr, out int costValue))
                {
                    Debug.LogWarning($"Invalid cost value '{trimmedPart}' on line {lineIndex}. Defaulting to 0.");
                    costValue = 0;
                }

                switch (costType)
                {
                    case 'w':
                        cost[0] += costValue; // White
                        break;
                    case 'u':
                        cost[1] += costValue; // Blue
                        break;
                    case 'g':
                        cost[2] += costValue; // Green
                        break;
                    case 'r':
                        cost[3] += costValue; // Red
                        break;
                    case 'k':
                        cost[4] += costValue; // Black
                        break;
                    default:
                        Debug.LogWarning($"Unrecognized cost type '{costType}' on line {lineIndex}.");
                        break;
                }
            }

            return cost;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse Price on line {lineIndex}: {ex.Message}");
            return new int[5]; // Default empty cost
        }
    }
}
#endif
