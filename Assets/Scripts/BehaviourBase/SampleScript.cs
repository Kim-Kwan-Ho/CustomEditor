using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleScript : BaseBehaviour
{

    [SerializeField] private SpriteRenderer _thisSpriteRenderer;
    [SerializeField] private SpriteRenderer _childSpriteRenderer;
    [SerializeField] private GameObject _childGameObject;
    [SerializeField] private Camera _findCamera;
    [SerializeField] private List<BaseBehaviour> _behaviourPrefabListInAsset;

    private List<int> _sampleList;


    protected override void Initialize()
    {
        base.Initialize();
        _sampleList = new List<int>();
    }



    private void CheckLogic()
    {
        Debug.Log("Sample Logic");
    }

#if UNITY_EDITOR
    protected override void OnBindField()
    {
        base.OnBindField();
        _thisSpriteRenderer = GetComponent<SpriteRenderer>();
        _childSpriteRenderer = FindGameObjectInChildren<SpriteRenderer>("ChildName");
        _childGameObject = FindGameObjectInChildren("ChildName");
        _findCamera = FindAnyObjectByType<Camera>();
        _behaviourPrefabListInAsset = FindObjectsInAsset<BaseBehaviour>();
    }
    protected override void OnButtonField()
    {
        base.Initialize();
        CheckLogic();
    }
#endif
}
