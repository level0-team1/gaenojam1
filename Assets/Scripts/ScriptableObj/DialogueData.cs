using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    [TextArea(2, 5)]
    public string text;
    public Sprite characterSprite;
    public Sprite characterSprite2;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "CookingGame/DialogueData")]
public class DialogueData : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}
