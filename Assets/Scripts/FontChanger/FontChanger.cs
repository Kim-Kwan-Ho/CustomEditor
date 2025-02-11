using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
#if UNITY_EDITOR
public class FontChanger : EditorWindow
{
    // ������ ��Ʈ
    private Font _normalFont; 
    private TMP_FontAsset _tmpFont; 


    // ���� ��Ʈ (���� ��������)
    private Font _compareNormalFont;
    private TMP_FontAsset _compareTMPFont; 

    [MenuItem("KKH/Font Changer")]
    public static void ShowWindow()
    {
        GetWindow<FontChanger>("Font Changer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Font Changer", EditorStyles.boldLabel);

        _normalFont = (Font)EditorGUILayout.ObjectField("Target UI Font", _normalFont, typeof(Font), false);
        _tmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Target TMP Font", _tmpFont, typeof(TMP_FontAsset), false);


        GUILayout.Space(10);

        if (GUILayout.Button("Change All Scene Fonts"))
        {
            ChangeSceneFonts(false); // �� �� ��� ��Ʈ�� ���� ������Ʈ ����
        }


        if (GUILayout.Button("Change All Prefab Fonts"))
        {
            ChangePrefabFonts(false); // ������Ʈ �� ��Ʈ�� ���� ��� ������ ����
        }

        GUILayout.Space(10);

        GUILayout.Label("Font Changer", EditorStyles.boldLabel);

        _compareNormalFont = (Font)EditorGUILayout.ObjectField("Compare UI Font", _compareNormalFont, typeof(Font), false);
        _compareTMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Compare TMP Font", _compareTMPFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Change Scene Fonts (Only Matching)"))
        {
            ChangeSceneFonts(true); // �� �� Ư�� ��Ʈ�� ���� ������Ʈ�� ����
        }
        

        if (GUILayout.Button("Change Prefab Fonts (Only Matching)"))
        {
            ChangePrefabFonts(true); // ������Ʈ �� Ư�� ��Ʈ�� ���� �����ո� ����
        }
    }
  
    private void ChangeSceneFonts(bool hasTarget)  // �� �� ��Ʈ ����
    {
        foreach (var text in FindObjectsOfType<Text>(true)) // UnityEngine.UI.Text
        {
            if (hasTarget == false || text.font == _compareNormalFont)
            {
                text.font = _normalFont;
                EditorUtility.SetDirty(text);
            }
        }

        foreach (var tmpText in FindObjectsOfType<TextMeshProUGUI>(true))
        {
            if (hasTarget == false || tmpText.font == _compareTMPFont)
            {
                tmpText.font = _tmpFont;
                EditorUtility.SetDirty(tmpText);
            }
        }
    }

    private void ChangePrefabFonts(bool hasTarget) // ������Ʈ �� ��Ʈ ����
    {
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);

            if (prefab != null)
            {
                bool changed = false;

                foreach (var text in prefab.GetComponentsInChildren<Text>(true))
                {
                    if (hasTarget == false || text.font == _compareNormalFont)
                    {
                        text.font = _normalFont;
                        changed = true;
                    }
                }

                foreach (var tmpText in prefab.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (hasTarget == false || tmpText.font == _compareTMPFont)

                    {
                        tmpText.font = _tmpFont;
                        changed = true;
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(prefab);
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif