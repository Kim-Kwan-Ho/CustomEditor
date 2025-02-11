using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
#if UNITY_EDITOR
public class FontChanger : EditorWindow
{
    // 변경할 폰트
    private Font _normalFont; 
    private TMP_FontAsset _tmpFont; 


    // 비교할 폰트 (현재 적용중인)
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
            ChangeSceneFonts(false); // 씬 내 모든 폰트를 가진 오브젝트 변경
        }


        if (GUILayout.Button("Change All Prefab Fonts"))
        {
            ChangePrefabFonts(false); // 프로젝트 내 폰트를 가진 모든 프리팹 변경
        }

        GUILayout.Space(10);

        GUILayout.Label("Font Changer", EditorStyles.boldLabel);

        _compareNormalFont = (Font)EditorGUILayout.ObjectField("Compare UI Font", _compareNormalFont, typeof(Font), false);
        _compareTMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Compare TMP Font", _compareTMPFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Change Scene Fonts (Only Matching)"))
        {
            ChangeSceneFonts(true); // 씬 내 특정 폰트를 가진 오브젝트만 변경
        }
        

        if (GUILayout.Button("Change Prefab Fonts (Only Matching)"))
        {
            ChangePrefabFonts(true); // 프로젝트 내 특정 폰트를 가진 프리팹만 변경
        }
    }
  
    private void ChangeSceneFonts(bool hasTarget)  // 씬 내 폰트 변경
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

    private void ChangePrefabFonts(bool hasTarget) // 프로젝트 내 폰트 변경
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