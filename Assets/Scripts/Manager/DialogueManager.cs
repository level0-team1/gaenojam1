using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("대화 데이터")]
    public DialogueData dialogueData;

    [Header("UI 연결")]
    public GameObject dialoguePanel;
    public Image characterImage;
    public Image characterImage2;
    public Image dialogueBoxImage;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public GameObject nextIndicator;

    [Header("설정")]
    public float typeSpeed = 0.04f;

    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine typeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartDialogueWithGuest(GuestSO guest)
    {
        if (guest != null)
        {
            if (guest.introDialogue != null) dialogueData = guest.introDialogue;
            if (dialogueBoxImage != null && guest.dialogueBoxSprite != null)
                dialogueBoxImage.sprite = guest.dialogueBoxSprite;
        }
        StartDialogue();
    }

    public void StartDialogue()
    {
        currentLine = 0;
        dialoguePanel.SetActive(true);

        if (dialogueData == null || dialogueData.lines.Count == 0)
        {
            // 데이터 없으면 패널만 열고 스페이스/클릭으로 넘길 수 있게 대기
            if (nextIndicator != null) nextIndicator.SetActive(true);
            return;
        }

        if (nextIndicator != null) nextIndicator.SetActive(false);
        ShowLine(currentLine);
    }

    private void ShowLine(int index)
    {
        if (index >= dialogueData.lines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueData.lines[index];

        nameText.text = line.characterName;

        if (characterImage != null)
        {
            if (line.characterSprite != null)
            {
                characterImage.sprite = line.characterSprite;
                characterImage.gameObject.SetActive(true);
            }
            else
                characterImage.gameObject.SetActive(false);
        }

        if (characterImage2 != null)
        {
            if (line.characterSprite2 != null)
            {
                characterImage2.sprite = line.characterSprite2;
                characterImage2.gameObject.SetActive(true);
            }
            else
                characterImage2.gameObject.SetActive(false);
        }

        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeText(line.text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        if (nextIndicator != null) nextIndicator.SetActive(false);
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        if (nextIndicator != null) nextIndicator.SetActive(true);
    }

    private void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
        {
            Advance();
        }
    }

    public void Advance()
    {
        if (dialogueData == null || dialogueData.lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        if (isTyping)
        {
            if (typeCoroutine != null) StopCoroutine(typeCoroutine);
            dialogueText.text = dialogueData.lines[currentLine].text;
            isTyping = false;
            if (nextIndicator != null) nextIndicator.SetActive(true);
            return;
        }

        currentLine++;
        ShowLine(currentLine);
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        GamePhaseManager.Instance.ChangePhase(GamePhase.MenuSelection);
    }
}
