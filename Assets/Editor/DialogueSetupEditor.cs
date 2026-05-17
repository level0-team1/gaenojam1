using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public static class DialogueSetupEditor
{
    [MenuItem("Tools/Setup Dialogue Manager References")]
    public static void SetupReferences()
    {
        // FindFirstObjectByType(true) searches inactive objects too
        DialogueManager dm = Object.FindAnyObjectByType<DialogueManager>(FindObjectsInactive.Include);
        if (dm == null) { Debug.LogError("DialogueManager를 찾을 수 없습니다."); return; }

        dm.dialoguePanel    = Find("TutorialPanel");
        dm.characterImage   = Find("CharacterImage")?.GetComponent<Image>();
        dm.characterImage2  = Find("CharacterImage2")?.GetComponent<Image>();
        dm.dialogueBoxImage = Find("DialogueBox")?.GetComponent<Image>();
        dm.nameText         = Find("NameText")?.GetComponent<TMP_Text>();
        dm.dialogueText     = Find("DialogueText")?.GetComponent<TMP_Text>();
        dm.nextIndicator    = Find("NextIndicator");

        // NameText Auto Size 설정 (에딧모드에서 적용)
        if (dm.nameText != null)
        {
            var tmp = (TextMeshProUGUI)dm.nameText;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 16f;
            tmp.fontSizeMax = tmp.fontSize > 16f ? tmp.fontSize : 55f;
            EditorUtility.SetDirty(tmp);
        }

        EditorUtility.SetDirty(dm);
        Debug.Log($"[DialogueSetup] 완료! panel={dm.dialoguePanel}, charImg2={dm.characterImage2}, nameAutoSize={((TextMeshProUGUI)dm.nameText)?.enableAutoSizing}");
    }

    // GameObject.Find only works on active objects — search all loaded scene objects instead
    private static GameObject Find(string objName)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.scene.isLoaded && go.name == objName) return go;
        return null;
    }
}
