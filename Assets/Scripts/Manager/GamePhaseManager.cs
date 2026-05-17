using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GamePhase
{
    Start,
    Tutorial,
    MenuSelection,
    Farming,
    Cooking,
    Result
}

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }

    [Header("현재 게임 단계")]
    public GamePhase currentPhase;

    [Header("UI 패널들")]
    public GameObject startPanel;
    public GameObject tutorialPanel;
    public GameObject selectionPanel;
    public GameObject farmingHUD;
    public GameObject cookingPanel;
    public GameObject resultPanel;

    [Header("파밍 설정")]
    public float farmingTime = 60f;
    private float timer;
    private int lastP1Score;
    private int lastP2Score;
    private RecipeSO lastRecipe;
    private string lastScoreLine;
    private List<IngredientSO> lastP1Cards;
    private List<IngredientSO> lastP2Cards;
    private bool _advanceDialogue;
    public TMP_Text timerText;

    [Header("결과 처리 관련")]
    public ScoringSystem scoringSystem;
    public Inventory player1Inventory;
    public Inventory player2Inventory;
    public TMP_Text resultScoreText;

    [Header("쿠킹/결과 패널 VN UI")]
    public Image cookingCharImage;
    public Image resultCharImage;
    public TMP_Text resultNameText;
    public Image cookingFillImage;
    public TMP_Text cookingStatusText;
    public float cookingBarDuration = 3f;
    public GameObject resultNextIndicator;
    public GameObject resultRestartButtonGO;

    [Header("손님 설정")]
    public List<GuestSO> guestList;
    public GuestSO SelectedGuest { get; private set; }

    [Header("카메라")]
    public CameraFollow player1Cam;
    public CameraFollow player2Cam;
    public Transform startCameraAnchor;
    public Camera startCamera;

    [Header("메뉴 선정 UI (큰 화면)")]
    public TMP_Text recipeNameText;
    public Image recipeFoodImage;
    public List<Image> hintImages;
    public Sprite unknownIcon;

    // 💡 NEW: 인게임 양쪽 상단 UI를 위한 변수들 추가
    [Header("파밍 HUD - Player 1 (좌측 상단)")]
    public TMP_Text p1FarmingRecipeName;
    public Image p1FarmingFoodImage;
    public List<Image> p1FarmingHintImages;

    [Header("파밍 HUD - Player 2 (우측 상단)")]
    public TMP_Text p2FarmingRecipeName;
    public Image p2FarmingFoodImage;
    public List<Image> p2FarmingHintImages;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ChangePhase(GamePhase.Start);
    }

    private void Update()
    {
        if (currentPhase == GamePhase.Farming)
            UpdateFarmingTimer();
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            _advanceDialogue = true;
    }

    public void ChangePhase(GamePhase newPhase)
    {
        currentPhase = newPhase;

        startPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        selectionPanel.SetActive(false);
        farmingHUD.SetActive(false);
        cookingPanel.SetActive(false);
        resultPanel.SetActive(false);

        switch (currentPhase)
        {
            case GamePhase.Start:
                startPanel.SetActive(true);
                SetStartCamera();
                break;
            case GamePhase.Tutorial:
                tutorialPanel.SetActive(true);
                SetStartCamera();
                if (guestList != null && guestList.Count > 0)
                    SelectedGuest = guestList[Random.Range(0, guestList.Count)];
                DialogueManager.Instance.StartDialogueWithGuest(SelectedGuest);
                break;
            case GamePhase.MenuSelection:
                selectionPanel.SetActive(true);
                SetStartCamera();
                RecipeManager.Instance.SelectRandomRecipe();
                UpdateSelectionUI();
                break;
            case GamePhase.Farming:
                farmingHUD.SetActive(true);
                SetFarmingCamera();
                timer = farmingTime;
                if (RecipeManager.Instance.SelectedRecipe == null)
                {
                    RecipeManager.Instance.SelectRandomRecipe();
                }
                break;
            case GamePhase.Cooking:
                cookingPanel.SetActive(true);
                SetStartCamera();
                if (SelectedGuest != null && cookingCharImage != null)
                {
                    cookingCharImage.sprite = SelectedGuest.characterSprite;
                    cookingCharImage.color = SelectedGuest.characterSprite != null ? Color.white : Color.clear;
                }
                StartCoroutine(CookingSequence());
                break;
            case GamePhase.Result:
                resultPanel.SetActive(true);
                SetStartCamera();
                StartCoroutine(ResultDialogueSequence());
                break;
        }
    }

    private void UpdateSelectionUI()
    {
        if (RecipeManager.Instance.SelectedRecipe == null) return;

        RecipeSO currentRecipe = RecipeManager.Instance.SelectedRecipe;
        List<IngredientSO> hints = RecipeManager.Instance.revealedHints;
        int totalIngredientsCount = currentRecipe.requiredIngredients.Count;

        // ==========================================
        // 1. 기존 큰 주문서 (SelectionPanel) 업데이트
        // ==========================================
        if (recipeNameText != null) recipeNameText.text = $"{currentRecipe.recipeName}";

        if (recipeFoodImage != null)
        {
            if (currentRecipe.resultImage != null)
            {
                recipeFoodImage.sprite = currentRecipe.resultImage;
                recipeFoodImage.gameObject.SetActive(true);
            }
            else recipeFoodImage.gameObject.SetActive(false);
        }

        for (int i = 0; i < hintImages.Count; i++)
        {
            if (hintImages[i] == null) continue;
            if (i < totalIngredientsCount)
            {
                hintImages[i].gameObject.SetActive(true);
                hintImages[i].color = Color.white;
                hintImages[i].sprite = (i < hints.Count) ? hints[i].icon : unknownIcon;
            }
            else hintImages[i].gameObject.SetActive(false);
        }

        // ==========================================
        // 💡 2. 파밍 HUD 양쪽 (P1, P2) 동시 업데이트
        // ==========================================

        // 텍스트 이름 동기화
        if (p1FarmingRecipeName != null) p1FarmingRecipeName.text = $"{currentRecipe.recipeName}";
        if (p2FarmingRecipeName != null) p2FarmingRecipeName.text = $"{currentRecipe.recipeName}";

        // 메인 음식 이미지 동기화
        if (p1FarmingFoodImage != null && currentRecipe.resultImage != null)
        {
            p1FarmingFoodImage.sprite = currentRecipe.resultImage;
            p1FarmingFoodImage.gameObject.SetActive(true);
        }
        if (p2FarmingFoodImage != null && currentRecipe.resultImage != null)
        {
            p2FarmingFoodImage.sprite = currentRecipe.resultImage;
            p2FarmingFoodImage.gameObject.SetActive(true);
        }

        // 힌트 이미지들 동기화 (P1)
        if (p1FarmingHintImages != null)
        {
            for (int i = 0; i < p1FarmingHintImages.Count; i++)
            {
                if (p1FarmingHintImages[i] == null) continue;
                if (i < totalIngredientsCount)
                {
                    p1FarmingHintImages[i].gameObject.SetActive(true);
                    p1FarmingHintImages[i].color = Color.white;
                    p1FarmingHintImages[i].sprite = (i < hints.Count) ? hints[i].icon : unknownIcon;
                }
                else p1FarmingHintImages[i].gameObject.SetActive(false);
            }
        }

        // 힌트 이미지들 동기화 (P2)
        if (p2FarmingHintImages != null)
        {
            for (int i = 0; i < p2FarmingHintImages.Count; i++)
            {
                if (p2FarmingHintImages[i] == null) continue;
                if (i < totalIngredientsCount)
                {
                    p2FarmingHintImages[i].gameObject.SetActive(true);
                    p2FarmingHintImages[i].color = Color.white;
                    p2FarmingHintImages[i].sprite = (i < hints.Count) ? hints[i].icon : unknownIcon;
                }
                else p2FarmingHintImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateFarmingTimer()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            DisplayTime(timer);
        }
        else
        {
            timer = 0;
            ChangePhase(GamePhase.Cooking);
        }
    }

    private void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void OnClickStart() => ChangePhase(GamePhase.Tutorial);
    public void OnClickPlay() => ChangePhase(GamePhase.MenuSelection);
    public void OnSelectionComplete() => ChangePhase(GamePhase.Farming);
    public void OnClickRestart() => ChangePhase(GamePhase.Start);

    private void SetStartCamera()
    {
        if (startCamera != null) startCamera.enabled = true;
        if (player1Cam != null) { player1Cam.enabled = false; player1Cam.GetComponent<Camera>().enabled = false; }
        if (player2Cam != null) { player2Cam.enabled = false; player2Cam.GetComponent<Camera>().enabled = false; }
    }

    private void SetFarmingCamera()
    {
        if (startCamera != null) startCamera.enabled = false;
        if (player1Cam != null) { player1Cam.enabled = true; player1Cam.GetComponent<Camera>().enabled = true; }
        if (player2Cam != null) { player2Cam.enabled = true; player2Cam.GetComponent<Camera>().enabled = true; }
    }

    private IEnumerator TypewriterText(TMP_Text textComp, string fullText, float charDelay = 0.04f)
    {
        textComp.text = "";
        foreach (char c in fullText)
        {
            textComp.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }

    private IEnumerator WaitForAdvance()
    {
        _advanceDialogue = false;
        yield return null;
        yield return new WaitUntil(() => _advanceDialogue);
        _advanceDialogue = false;
    }

    private RecipeSO FindBestMatchRecipe(List<IngredientSO> cards)
    {
        if (cards == null || cards.Count == 0) return null;
        RecipeSO best = null;
        int bestScore = -1;
        foreach (var recipe in RecipeManager.Instance.AllRecipes)
        {
            int s = scoringSystem.CalculateCookingScore(cards, recipe);
            if (s > bestScore) { bestScore = s; best = recipe; }
        }
        return bestScore > 0 ? best : null;
    }

    private IEnumerator ShowLine(string text)
    {
        if (resultScoreText == null) yield break;
        yield return StartCoroutine(TypewriterText(resultScoreText, text));
        if (resultNextIndicator != null) resultNextIndicator.SetActive(true);
        yield return StartCoroutine(WaitForAdvance());
        if (resultNextIndicator != null) resultNextIndicator.SetActive(false);
    }

    private IEnumerator ResultDialogueSequence()
    {
        if (resultRestartButtonGO != null) resultRestartButtonGO.SetActive(false);
        if (resultNextIndicator != null) resultNextIndicator.SetActive(false);
        if (resultScoreText != null) resultScoreText.text = "";

        // Line 1: P1 dish identification
        RecipeSO p1Best = FindBestMatchRecipe(lastP1Cards);
        string p1Dish = p1Best != null ? p1Best.recipeName : "알 수 없는 요리";
        yield return StartCoroutine(ShowLine($"호오... 플레이어1님의 요리는 {p1Dish}이군요!\n{lastP1Score}점입니다."));

        // Line 2: P2 dish identification
        RecipeSO p2Best = FindBestMatchRecipe(lastP2Cards);
        string p2Dish = p2Best != null ? p2Best.recipeName : "알 수 없는 요리";
        yield return StartCoroutine(ShowLine($"그리고 플레이어2님의 요리는 {p2Dish}이군요!\n{lastP2Score}점입니다."));

        // Line 3: Winner reveal
        string winnerLine;
        if (lastP1Score > lastP2Score)
            winnerLine = "두구두구두구...\n제가 손들어주고 싶은 분은 이 분입니다!\n\n<color=blue>플레이어1 승리!</color>";
        else if (lastP2Score > lastP1Score)
            winnerLine = "두구두구두구...\n제가 손들어주고 싶은 분은 이 분입니다!\n\n<color=red>플레이어2 승리!</color>";
        else
            winnerLine = "두구두구두구...\n두 분 모두 훌륭합니다!\n\n무승부!";
        yield return StartCoroutine(ShowLine(winnerLine));

        // Line 4: Guest score reaction
        if (lastScoreLine != null)
            yield return StartCoroutine(ShowLine(lastScoreLine));

        if (resultRestartButtonGO != null) resultRestartButtonGO.SetActive(true);
    }

    private IEnumerator CookingSequence()
    {
        if (cookingFillImage != null) cookingFillImage.fillAmount = 0f;
        if (cookingStatusText != null) cookingStatusText.text = "요리를 준비하는 중...";

        float elapsed = 0f;
        while (elapsed < cookingBarDuration)
        {
            elapsed += Time.deltaTime;
            if (cookingFillImage != null)
                cookingFillImage.fillAmount = Mathf.Clamp01(elapsed / cookingBarDuration);
            yield return null;
        }

        if (cookingFillImage != null) cookingFillImage.fillAmount = 1f;
        if (cookingStatusText != null) cookingStatusText.text = "요리 완성!";
        yield return new WaitForSeconds(0.6f);

        RecipeSO targetRecipe = RecipeManager.Instance.SelectedRecipe;
        int p1Score = 0;
        int p2Score = 0;

        if (targetRecipe != null)
        {
            p1Score = scoringSystem.CalculateCookingScore(player1Inventory.GetOwnedCards(), targetRecipe, player1Inventory);
            p2Score = scoringSystem.CalculateCookingScore(player2Inventory.GetOwnedCards(), targetRecipe, player2Inventory);
        }

        lastP1Score  = p1Score;
        lastP2Score  = p2Score;
        lastRecipe   = targetRecipe;
        lastScoreLine = null;
        lastP1Cards  = player1Inventory != null ? new List<IngredientSO>(player1Inventory.GetOwnedCards()) : new List<IngredientSO>();
        lastP2Cards  = player2Inventory != null ? new List<IngredientSO>(player2Inventory.GetOwnedCards()) : new List<IngredientSO>();

        if (SelectedGuest != null)
        {
            int maxScore = Mathf.Max(p1Score, p2Score);
            var scoreLines = (maxScore >= 50) ? SelectedGuest.highScoreLines : SelectedGuest.lowScoreLines;
            if (scoreLines != null && scoreLines.Count > 0)
                lastScoreLine = scoreLines[Random.Range(0, scoreLines.Count)];

            if (resultCharImage != null)
            {
                resultCharImage.sprite = SelectedGuest.characterSprite;
                resultCharImage.color = SelectedGuest.characterSprite != null ? Color.white : Color.clear;
            }
            if (resultNameText != null)
                resultNameText.text = SelectedGuest.guestName;
        }

        ChangePhase(GamePhase.Result);
    }
}