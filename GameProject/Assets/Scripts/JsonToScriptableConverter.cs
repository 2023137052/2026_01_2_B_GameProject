#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


public enum ConversionType
{
    Items,
    Dialogs
}

[Serializable]

public class DialogRowData
{
    public int? id;
    public string characterName;
    public string text;
    public int? nextId;
    public string portraitPath;
    public string choiceText;
    public int? choiceNextId;
}

public class JsonToScriptableConverter : EditorWindow
{
    private string jsonFilePaht = "";
    private string outputFolder = "Asset/ScriptableObjects";
    private bool createDatabase = true;


    [MenuItem("Tools/JSON to Scriptable Objects")]

    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects");
    }

    void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if(GUILayout.Button("Select JSON File"))
        {
            jsonFilePaht = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Selected File : ", jsonFilePaht);
        EditorGUILayout.Space();

        conversionType = (conversionType)EditorGUILayout.EnumPopup("Conversion Type: ", conversionType);

        if (conversionType == ConversionType.Dialogs && outputFolder == "Asset/ScriptableObjects")
        {
            outputFolder = "Asset/ScriptableObjects/Items";
        }
        else if (conversionType == ConversionType.Dialogs && outputFolder == "Asset/ScriptableObjects")
        {
            outputFolder = "Asset/ScriptableObjects/Dialogs";
        }



            outputFolder = EditorGUILayout.TextField("outputFolder : ", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Database Asset", createDatabase);
        EditorGUILayout.Space();

        if (GUILayout.Button("Convert to Scriptable Objects"))
        {
            if (string.IsNullOrEmpty(jsonFilePaht))
            {
                EditorUtility.DisplayDialog("Error", "Pease Select a JSON file first", "OK");
                return;
            }
            ConvertJsonToScriptableObjects();
        }
    }



    private void ConvertJsonToScriptableObjects()
    {

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string jsonText = File.ReadAllText(jsonFilePaht);

        try
        {

            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO>createdItems = new List<ItemSO>();

            foreach (ItemData itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                if(System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"아이템 {itemData.itemName}의 유효하지 않은 타입 : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");

                    if (itemSO.icon != null)
                    {
                        Debug.LogWarning($"아이템 {itemData.nameEng} 의 아이콘을 찾을 수 없습니다. : {itemData.iconPath}");
                    }
                }

                string assetPath = $"{outputFolder}/Item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";
                AssetDatabase.CreateAsset(itemSO, assetPath);

                itemSO.name = $"Item_{itemData.id.ToString("D4")} + {itemData.nameEng}";
                createdItems.Add(itemSO);

                EditorUtility.SetDirty(itemSO);

                if(createDatabase && createdItems.Count > 0)
                {
                    ItemDataBaseSO dataBase = ScriptableObject.CreateInstance<ItemDataBaseSO>();
                    dataBase.items = createdItems;

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog("Sucess", $"Created {createdItems.Count} scriptable objects!", "OK");
                }
            }

        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }


    private void ConvertJsonToDialogScriptableObjects()
    {

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string JsonText = File.ReadAllText(jsonFilePath);

        try
        {
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(JsonText);

            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialods = new List<DialogSO>();

            foreach (var rowData in rowDataList)
            {
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextId.HasValue)
                {
                    int parentId = -1;

                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }
                    if (parentId == -1)
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText} 의 부모 대화를 찾을 수 없습니다. ");
                    }

                    if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextId = rowData.choiceNextId.Value;

                        string choiceAssetPath = $"{outputFolder}/Choice_{parentId}_{parentDialog.choices.Count + 1}.asset";
                        EditorUtility.SetDirty(choiceSO);
                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"선택지 {rowData.choiceText}를 연결할 대화 (ID : {parentId}를 찾을 수 없습니다. ");
                    }
                }
            }
            //3단계 : 대화 스크립트를 오브젝트로 저장
            foreach (var dialog in createDialogs)
            {
                //스크립트블 오브젝트 저장 - ID 4자리 숫자
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset(dialog, assetPath);

                //에셋 이름 지정
                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";
                EditorUtility.SetDirty(dialog);
            }

            //데이터베이스 생성
            if (createDatabase && createDialogs.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = createDialogs;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.assets");
                EditorUtility.SetDirty(database);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Created {createDialogs.Count} dialog scriptable objects!", "OK");
        }


        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Faild to convert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류: {e}");
        }

        
    }

}

#endif