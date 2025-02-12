using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

public static class XmlDataLoader
{
    public static List<Dictionary<string, string>> LoadFromXML(string filePath, string fileName, string tagName)
    {
        var list = new List<Dictionary<string, string>>();
        var reEntries = new HashSet<string>();


        TextAsset txtAsset = (TextAsset)Resources.Load($"XML/{filePath}/{fileName}");
        if (txtAsset == null)
        {
            Debug.LogError($"XML file not found: {fileName}");
            return null;
        }

        XDocument xmlDoc = XDocument.Parse(txtAsset.text);
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
                    values.Add(value); // 모든 값 추가
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
    public static List<T> ConvertTo<T>(List<Dictionary<string, string>> dataList) where T : new() //지정한 클래스 타입으로 변환 (string -> 해당 필드 타입)
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

    private static object ConvertValue(Type type, string value) // string 데이터를 지정된 타입으로 변환
    {
        if (string.IsNullOrEmpty(value)) return null;

        if (type == typeof(int))
            return int.Parse(value);
        if (type == typeof(float))
            return float.Parse(value, CultureInfo.InvariantCulture);
        if (type == typeof(bool))
            return bool.Parse(value);
        if (type == typeof(string))
            return value;
        if (type.IsEnum)
            return Enum.Parse(type, value);

        return null;
    }

}
