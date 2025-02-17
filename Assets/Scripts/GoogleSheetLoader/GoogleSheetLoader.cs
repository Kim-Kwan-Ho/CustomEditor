using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
public class GoogleSheetLoader : EditorWindow
{

    private string _sheetURL = "https://docs.google.com/spreadsheets/d/1m7R0Fi3tytwaAbrCrJIUEhtbmxaVNuHoRVziMe-5whk/export?format=csv";
    private string _sheetData; // 시트 데이터
    private string _savePath = "Assets/";
    private MonoScript _soTypeScript; // 스크립터블 오브젝트를 상속받은 스크립트
    private Type _soType;  // 스크립터블 오브젝트 타입

    [MenuItem("KKH/GoogleSheet Loader")]
    public static void ShowWindow()
    {
        GetWindow<GoogleSheetLoader>("GoogleSheet Loader");
    }

    private void OnGUI()
    {
        GUILayout.Label("GoogleSheet Loader", EditorStyles.boldLabel);

        _sheetURL = EditorGUILayout.TextField("Sheet URL", _sheetURL);
        _soTypeScript = (MonoScript)EditorGUILayout.ObjectField("SO Type", _soTypeScript, typeof(MonoScript), false);
        _savePath = EditorGUILayout.TextField("Save Path", _savePath);



        GUILayout.Space(10);
        if (GUILayout.Button("Create Or Edit Scriptable Objects"))
        {
            EditorCoroutineUtility.StartCoroutine(CoCreateEditSos(), this);
        }
    }

    private IEnumerator CoCreateEditSos() // SO Maker 응용
    {
        _soType = _soTypeScript.GetClass();

        using (UnityWebRequest www = UnityWebRequest.Get(_sheetURL))
        {
            yield return www.SendWebRequest();

            if (www.isDone)
            {
                _sheetData = www.downloadHandler.text;
                var parsedData =  LoadFromGoogleSheet(_sheetData);
                int index = 0;
                foreach (var data in parsedData)
                {
                    CreateOrEditScriptableObject(_soType, data, index);
                    index++;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
    private void CreateOrEditScriptableObject(Type soType, Dictionary<string, string> data, int index) // SO Maker 응용
    {
        string assetName = string.Empty;
        string path = string.Empty;

        if (data.ContainsKey("_name"))
        {
            assetName = data["_name"];
            path = $"{_savePath}/{assetName}.asset";
        }
        else
        {
            assetName = soType.Name;
            path = $"{_savePath}/{assetName}{index}.asset";
        }

        ScriptableObject asset;
        if (!Directory.Exists(_savePath))
        {
            Directory.CreateDirectory(_savePath);
        }
        if (Directory.Exists(path))
        {
            asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        }
        else
        {
            asset = ScriptableObject.CreateInstance(soType);
        }

        foreach (var pair in data)
        {
            string key = pair.Key;
            string value = pair.Value;

            FieldInfo field = soType.GetField(key, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo property = soType.GetProperty(key, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(asset, ConvertValue(field.FieldType, value));
            }
            else if (property != null)
            {
                property.SetValue(asset, ConvertValue(property.PropertyType, value));
            }
        }
        AssetDatabase.CreateAsset(asset, path);
    }

    private List<Dictionary<string, string>> LoadFromGoogleSheet(string sheetData) // XML DataLoader 응용
    {
        var list = new List<Dictionary<string, string>>();
        var reEntries = new HashSet<string>();
        sheetData = sheetData.Replace("\r", "");
        string[] rows = sheetData.Split('\n');
        string[] keys = rows[0].Split(",");

        for (int i = 1; i < rows.Length; i++)
        {

            var entry = new Dictionary<string, string>();
            var values = new List<string>();
            string[] columns = rows[i].Split(",");
            for (int j = 0; j < columns.Length; j++)
            {
                string key = keys[j];
                string value = columns[j];
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    entry[key] = value;
                    values.Add(value);
                }
            }
            if (entry.Count == 0) continue;
            string uniqueKey = string.Join("|", values);
            if (reEntries.Contains(uniqueKey))
            {
                continue;
            }
            reEntries.Add(uniqueKey);
            list.Add(entry);
        }
        return list;
    }
    public static List<T> ConvertTo<T>(List<Dictionary<string, string>> dataList) where T : new() // XML DataLoader 응용
    {
        List<T> convertedList = new List<T>();

        foreach (var data in dataList)
        {
            T obj = new T();
            Type type = typeof(T);

            foreach (var pair in data)
            {
                string key = pair.Key;
                string value = pair.Value;


                FieldInfo field = type.GetField(key, BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo property = type.GetProperty(key, BindingFlags.Public | BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(obj, ConvertValue(field.FieldType, value));
                }
                else if (property != null)
                {
                    property.SetValue(obj, ConvertValue(property.PropertyType, value));
                }
            }
            convertedList.Add(obj);
        }
        return convertedList;
    }
    private static object ConvertValue(Type type, string value) // XML DataLoader 응용
    {
        if (string.IsNullOrEmpty(value)) return null;

        if (type == typeof(int))
            return int.Parse(value);
        if (type == typeof(float))
        {
            value = value.Replace(',', '.');
            return float.Parse(value, CultureInfo.InvariantCulture);
        }
        if (type == typeof(bool))
            return bool.Parse(value);
        if (type == typeof(string))
            return value;
        if (type.IsEnum)
            return Enum.Parse(type, value);

        return null;
    }
}
