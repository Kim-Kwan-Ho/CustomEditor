using System.Collections.Generic;
using UnityEngine;


#region Enum & Serializable Class

// 외부에서 가지고온 샘플
public enum DialogueState
{
    General, Select, SelectAfter
}
public enum SelectState
{
    NONE, REPEAT
}

public enum SoundType
{
    NONE,
    FADE_IN,
    FADE_OUT,
    CROSS_FADE,
    DELAY
}
[System.Serializable]
public class Dialogue
{
    public int textid;
    public string name;
    public string description;
    public int next;
    public DialogueState category;
    public SelectState selecttype;
    public string synergy;
    public string potrait;
    public string backgroudnimage;
}

[System.Serializable]
public class DialogueSound
{
    public string bgm;
    public string effect;
    public SoundType fade;
    public float fadetime;
    public float delaytime;
}

[System.Serializable]
public class DialogueContainer
{
    public Dialogue Dialogue;
    public DialogueSound DialogueSound;
}

#endregion



public class XmlSample : MonoBehaviour
{
    // 데이터 타입 (클래스 및 enum)은 외부에서 작성된 샘플 사용

    private void Start()
    {
        var temp = LoadDialogueData("Test", "Test1", "conversation");
    }
    private Dictionary<int, DialogueContainer> LoadDialogueData(string folder, string fileName, string tag)
    {
        var dataList = XmlDataLoader.LoadFromXML(folder, fileName, tag);

        List<Dialogue> dialogues = XmlDataLoader.ConvertTo<Dialogue>(dataList);
        List<DialogueSound> sounds = XmlDataLoader.ConvertTo<DialogueSound>(dataList);

        Dictionary<int, DialogueContainer> dialogueDictionary = new Dictionary<int, DialogueContainer>();

        for (int i = 0; i < dialogues.Count; i++)
        {
            DialogueContainer dialogueContainer = new DialogueContainer
            {
                Dialogue = dialogues[i],
                DialogueSound = sounds[i]
            };

            int textid = dialogues[i].textid;
            if (!dialogueDictionary.ContainsKey(textid))
            {
                dialogueDictionary.Add(textid, dialogueContainer);
            }
        }
        Debug.Log("Dialogue Count: " + dialogueDictionary.Count);
        return dialogueDictionary;
    }
}
