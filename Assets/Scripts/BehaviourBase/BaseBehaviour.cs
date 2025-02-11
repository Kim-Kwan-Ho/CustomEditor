using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


public class BaseBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        Initialize();
    }
    protected virtual void Initialize() { }

#if UNITY_EDITOR // 에디터에서만 작동하도록 설정
  
    protected virtual void OnButtonField() { } // 에디터 내에서 테스트 용도로 사용할 버튼

     
    #region ObjectFinder
    public enum EDataType 
    {
        prefab,
        asset,
        png

    }
    protected T FindObjectInAsset<T>() where T : UnityEngine.Object
    {
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(p);
            if (asset is not null && asset is T)
            {
                return asset;
            }
        }
        Debug.LogWarning($"No asset found with of type {typeof(T)}.");
        return null;
    }
    protected T FindObjectInAsset<T>(string name, EDataType type) where T : UnityEngine.Object
    {
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            if (p.Contains((name + "." + Enum.GetName(typeof(EDataType), type)).Trim(' ')))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(p);
                if (asset == null)
                {
                    Debug.LogWarning($"Asset at path {p} could not be loaded.");
                }
                return asset;
            }
        }
        Debug.LogWarning($"No asset found with name {name} of type {type}.");
        return null;
    }
    // 유효값 검즘

    protected List<T> FindObjectsInAsset<T>() where T : UnityEngine.Object
    {
        List<T> list = new List<T>();
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(p);
            if (asset != null)
            {
                list.Add(asset);
            }
        }
        if (list.Count > 0)
        {
            return list;
        }
        else
        {
            Debug.LogWarning($"No asset found with type.");
            return null;
        }
    }
    protected List<T> FindObjectsInAsset<T>(string name, EDataType type) where T : UnityEngine.Object
    {
        List<T> list = new List<T>();
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            if (p.Contains((name + "." + Enum.GetName(typeof(EDataType), type)).Trim(' ')) && !p.Contains("Packages"))
            {

                T asset = AssetDatabase.LoadAssetAtPath<T>(p);
                if (asset != null)
                {
                    list.Add(asset);
                }
            }
        }
        if (list.Count > 0)
        {
            return list;
        }
        else
        {
            Debug.LogWarning($"No asset found with type.");
            return null;
        }
    }
    #endregion
    
    
    #region Validate

    protected void CheckNullValue(string objectName, UnityEngine.Object obj)
    {
        if (obj == null)
        {
            Debug.Log(objectName + " has null value");
        }
    }
    protected void CheckNullValue(string objectName, IEnumerable objs)
    {
        if (objs == null)
        {
            Debug.Log(objectName + "has null value");
            return;
        }
        foreach (var obj in objs)
        {
            if (obj == null)
            {
                Debug.Log(objectName + "has null value");
            }
        }
    }

    #endregion
    
    #region Binder
    protected virtual void OnBindField() { } // 바인딩 오브젝트를 사용할 버튼
    protected List<T> GetComponentsInChildrenExceptThis<T>() where T : Component
    {
        T[] components = GetComponentsInChildren<T>(true);
        List<T> list = new List<T>();
        foreach (T component in components)
        {
            if (component.gameObject.GetInstanceID() == this.gameObject.GetInstanceID())
            {
                continue;
            }
            else
            {
                list.Add(component);
            }
        }
        return list;
    }
    protected GameObject FindGameObjectInChildren(string name)
    {
        var objects = GetComponentsInChildren<Transform>(true);
        foreach (var obj in objects)
        {
            if (obj.gameObject.name.Equals(name))
                return obj.gameObject;
        }
        return null;
    }
    protected T FindGameObjectInChildren<T>(string name) where T : Component
    {
        T[] objects = GetComponentsInChildren<T>(true);
        foreach (var obj in objects)
        {
            if (obj.gameObject.name.Equals(name))
                return obj;
        }
        return null;
    }
    protected T[] GetComponentsInGameObject<T>(string name) where T : Component
    {
        GameObject gob = GameObject.Find(name);
        return gob.GetComponentsInChildren<T>(true);
    }

    protected T GetComponentInChildrenExceptThis<T>() where T : Component
    {
        T[] components = GetComponentsInChildren<T>(true);
        foreach (T component in components)
        {
            if (component.gameObject.GetInstanceID() == this.gameObject.GetInstanceID())
            {
                continue;
            }
            else
            {
                return component;
            }
        }

        return null;
    }
    #endregion
#endif 
}

#if UNITY_EDITOR
[CustomEditor(typeof(BaseBehaviour), true)]
[CanEditMultipleObjects]
public class BehaviourBaseEditor : Editor
{

    private MethodInfo _bindMethod = (typeof(BaseBehaviour)).GetMethod("OnBindField", BindingFlags.NonPublic | BindingFlags.Instance);
    private MethodInfo _buttonMethod = (typeof(BaseBehaviour)).GetMethod("OnButtonField", BindingFlags.NonPublic | BindingFlags.Instance);

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Active Button"))
        {
            _buttonMethod.Invoke(target, new object[] { });
            EditorUtility.SetDirty(target);
        }
        GUILayout.Space(50);
        if (GUILayout.Button("Bind Objects"))
        {
            _bindMethod.Invoke(target, new object[] { });
            EditorUtility.SetDirty(target);
        }
        GUILayout.Space(20);

        base.OnInspectorGUI();
    }

}
#endif