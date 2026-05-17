using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGuest", menuName = "CookingGame/GuestSO")]
public class GuestSO : ScriptableObject
{
    public string guestName;
    public Sprite characterSprite;
    public Sprite dialogueBoxSprite;
    public DialogueData introDialogue;
    public List<string> highScoreLines = new List<string>();
    public List<string> lowScoreLines = new List<string>();
}
