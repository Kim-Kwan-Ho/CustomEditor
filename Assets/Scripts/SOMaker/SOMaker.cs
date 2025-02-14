using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using System;

#if UNITY_EDITOR
public class SOMaker : EditorWindow
{

    private TextAsset _soDataXml; 
    private string _tagName = "test"; // 임시태그
    private string _savePath = "Assets/";
    private MonoScript _soTypeScript; // 스크립터블 오브젝트를 상속받은 스크립트
    private Type _soType;  // 스크립터블 오브젝트 타입

    [MenuItem("KKH/SO Maker")]
    public static void ShowWindow()
    {
        GetWindow<SOMaker>("SO Maker");
    }

    private void OnGUI()
    {
        GUILayout.Label("SO Maker", EditorStyles.boldLabel);

        _soDataXml = (TextAsset)EditorGUILayout.ObjectField("XML Target", _soDataXml, typeof(TextAsset), false);
        _tagName = EditorGUILayout.TextField("Tag Name", _tagName);
        _soTypeScript = (MonoScript)EditorGUILayout.ObjectField("SO Type", _soTypeScript, typeof(MonoScript), false);
        _savePath = EditorGUILayout.TextField("Save Path", _savePath);


        GUILayout.Space(10);
        if (GUILayout.Button("Create Or Edit Scriptable Objects"))
        {
            CreateEditSOs();
        }
    }

    private void CreateEditSOs()
    {
        _soType = _soTypeScript.GetClass();

        if (!Directory.Exists(_savePath))
        {
            Directory.CreateDirectory(_savePath);
        }


        var parsedData = LoadFromXML(_soDataXml, _tagName);


        int index = 0;
        foreach (var data in parsedData)
        {
            CreateOrEditScriptableObject(_soType, data, index);
            index++;
        }

        AssetDatabase.SaveAssets(); 
        AssetDatabase.Refresh();
    }

    private void CreateOrEditScriptableObject(Type soType, Dictionary<string, string> data, int index) // 스크립터블 오브젝트 생성 , 만약에 존재중인 스크립터블 오브젝트라면 편집
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
    private List<Dictionary<string, string>> LoadFromXML(TextAsset textAsset, string tagName) // XML DataLoader 응용
    {
        var list = new List<Dictionary<string, string>>();
        var reEntries = new HashSet<string>();

        XDocument xmlDoc = XDocument.Parse(textAsset.text);
        foreach (var node in xmlDoc.Descendants(tagName))
        {
            var entry = new Dictionary<string, string>();
            var values = new List<string>();

            foreach (var childNode in node.Elements())
            {
                string key = childNode.Name.LocalName;
                string value = childNode.Value.Trim();

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
#endif