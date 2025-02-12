using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public static class XmlDataParser
{
    public static List<Dictionary<string, string>> LoadTagsFromXML(string filePath, string fileName, string tagName)
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

}
