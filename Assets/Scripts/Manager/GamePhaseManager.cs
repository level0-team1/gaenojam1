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

    // 💡 새로 추가된 메뉴 선정 UI 변수들
    [Header("메뉴 선정 UI")]
    public TMP_Text recipeNameText;
    public List<Image> hintImages;
    public Sprite unknownIcon;            // 💡 추가: 비공개 재료에 띄울 물음표 이미지

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
                // 💡 메뉴 선정 화면이 켜질 때 레시피를 뽑고 UI를 갱신합니다.
                RecipeManager.Instance.SelectRandomRecipe();
                UpdateSelectionUI();
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

    // 💡 UI 갱신 전용 함수 추가
    // 💡 UI 갱신 전용 함수 업데이트 (물음표 처리 추가)
    private void UpdateSelectionUI()
    {
        if (RecipeManager.Instance.SelectedRecipe == null) return;

        // 1. 레시피 이름 띄우기
        if (recipeNameText != null)
        {
            recipeNameText.text = $"오늘의 메뉴:\n<color=yellow>{RecipeManager.Instance.SelectedRecipe.recipeName}</color>";
        }

        // 2. 힌트 및 물음표 아이콘 띄우기
        List<IngredientSO> hints = RecipeManager.Instance.revealedHints;
        int totalIngredientsCount = RecipeManager.Instance.SelectedRecipe.requiredIngredients.Count; // 총 필요한 재료 개수

        for (int i = 0; i < hintImages.Count; i++)
        {
            // 총 재료 개수 안쪽에 있는 슬롯은 일단 켭니다.
            if (i < totalIngredientsCount)
            {
                hintImages[i].gameObject.SetActive(true);
                hintImages[i].color = Color.white;

                if (i < hints.Count)
                {
                    // 힌트로 뽑힌 개수만큼은 진짜 아이콘을 보여줌
                    hintImages[i].sprite = hints[i].icon;
                }
                else
                {
                    // 나머지는 물음표 아이콘으로 덮음
                    hintImages[i].sprite = unknownIcon;
                }
            }
            // 레시피의 총 재료 개수보다 남는 UI 슬롯은 아예 숨깁니다.
            else
            {
                hintImages[i].gameObject.SetActive(false);
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
            resultScoreText.text = resultMessage;
        }

        ChangePhase(GamePhase.Result);
    }
}