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
    public TMP_Text timerText;

    [Header("결과 처리 관련")]
    public ScoringSystem scoringSystem;
    public Inventory player1Inventory;
    public Inventory player2Inventory;
    public TMP_Text resultScoreText;

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
        {
            UpdateFarmingTimer();
        }
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
                break;
            case GamePhase.Tutorial:
                tutorialPanel.SetActive(true);
                break;
            case GamePhase.MenuSelection:
                selectionPanel.SetActive(true);
                RecipeManager.Instance.SelectRandomRecipe();
                UpdateSelectionUI(); // 💡 여기서 큰 화면과 양쪽 HUD를 동시에 세팅합니다.
                break;
            case GamePhase.Farming:
                farmingHUD.SetActive(true);
                timer = farmingTime;
                if (RecipeManager.Instance.SelectedRecipe == null)
                {
                    RecipeManager.Instance.SelectRandomRecipe();
                }
                break;
            case GamePhase.Cooking:
                cookingPanel.SetActive(true);
                StartCoroutine(CookingSequence());
                break;
            case GamePhase.Result:
                resultPanel.SetActive(true);
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

    private IEnumerator CookingSequence()
    {
        Debug.Log("조리를 시작합니다...");
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.0f);

        RecipeSO targetRecipe = RecipeManager.Instance.SelectedRecipe;
        int p1Score = 0;
        int p2Score = 0;

        if (targetRecipe != null)
        {
            p1Score = scoringSystem.CalculateCookingScore(player1Inventory.GetOwnedCards(), targetRecipe);
            p2Score = scoringSystem.CalculateCookingScore(player2Inventory.GetOwnedCards(), targetRecipe);
        }

        string resultMessage = targetRecipe != null ? $"오늘의 요리: {targetRecipe.recipeName}\n\n" : "오늘의 요리: 알 수 없음\n\n";
        resultMessage += $"P1 점수: {p1Score}점\n";
        resultMessage += $"P2 점수: {p2Score}점\n\n";

        if (p1Score > p2Score)
            resultMessage += "<color=blue>플레이어 1 승리!</color>";
        else if (p2Score > p1Score)
            resultMessage += "<color=red>플레이어 2 승리!</color>";
        else
            resultMessage += "무승부!";

        if (resultScoreText != null)
        {
            resultMessage = resultMessage.Replace("\n", " ");
            resultScoreText.text = resultMessage;
        }

        ChangePhase(GamePhase.Result);
    }
}