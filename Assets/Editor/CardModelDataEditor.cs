#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
                EditorUtility.DisplayDialog("Error", "CSV file not found. Ensure 'CardDataCsv.csv' exists in the Resources folder.", "OK");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(csvFilePath);
                CardInfo[] newCardInfos = new CardInfo[90];

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = lines[i].Split(',');

                    if (values.Length < 6)
                    {
                        Debug.LogError($"Invalid data format on line {i + 1}: Expected at least 6 values but found {values.Length}.");
                        continue;
                    }

                    try
                    {
                        int uniqueId = i;

                        CardInfo cardInfo = new CardInfo
                        {
                            uniqueId = uniqueId,
                            cardType = ParseCardType(values[1], i + 1),
                            cardLevel = SafeParseInt(values[0], "CardLevel", i + 1),
                            points = SafeParseInt(values[2], "Points", i + 1),
                            cost = ParseCost(values[3], i + 1),
                            illustration = values[4].Trim()
                        };

                        newCardInfos[uniqueId - 1] = cardInfo;
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

                EditorUtility.DisplayDialog("Success", "Card data successfully imported!", "OK");
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

    private int ParseCardType(string value, int lineIndex)
    {
        value = value.Trim().ToLower();

        return value switch
        {
            "white" => 0,
            "blue" => 1,
            "red" => 2,
            "green" => 3,
            "black" => 4,
            _ => -1
        };
    }

    private int[] ParseCost(string costString, int lineIndex)
    {
        try
        {
            int[] costs = new int[5];
            string[] parts = costString.Split('+');

            foreach (string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;

                char costType = part[^1];
                string costValueStr = part[..^1];

                if (!int.TryParse(costValueStr, out int costValue))
                {
                    Debug.LogWarning($"Invalid cost value '{part}' on line {lineIndex}. Defaulting to 0.");
                    costValue = 0;
                }

                switch (costType)
                {
                    case 'w': costs[0] += costValue; break;
                    case 'u': costs[1] += costValue; break;
                    case 'g': costs[2] += costValue; break;
                    case 'r': costs[3] += costValue; break;
                    case 'k': costs[4] += costValue; break;
                    default:
                        Debug.LogWarning($"Unknown cost type '{costType}' on line {lineIndex}.");
                        break;
                }
            }

            return costs;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse cost on line {lineIndex}: {ex.Message}");
            return new int[5];
        }
    }
}
#endif
