using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 타이머나 텍스트 처리를 위해 필요

public enum GamePhase
{
    Start,          // 화면 1: 시작 화면
    Tutorial,       // 화면 2: 주의사항 및 방법
    MenuSelection,  // 화면 3: 랜덤 메뉴 선정 (룰렛)
    Farming,        // 화면 4: 실제 게임 (파밍)
    Cooking,        // 화면 6: 조리 중 (3, 2, 1)
    Result          // 화면 7: 결과 발표
}

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }

    [Header("현재 게임 단계")]
    public GamePhase currentPhase;

    [Header("UI 패널들")]
    public GameObject startPanel;        // 화면 1 [cite: 1]
    public GameObject tutorialPanel;     // 화면 2 [cite: 12]
    public GameObject selectionPanel;    // 화면 3 [cite: 19]
    public GameObject farmingHUD;        // 화면 4 (인게임 UI) [cite: 46]
    public GameObject cookingPanel;      // 화면 6 [cite: 22]
    public GameObject resultPanel;       // 화면 7 [cite: 36]

    [Header("파밍 설정")]
    public float farmingTime = 60f;      // 1분 (기획안 00:59 기준) [cite: 3, 13]
    private float timer;
    public TMP_Text timerText;           // 화면 상단 타이머 텍스트 [cite: 35, 68]

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 첫 번째 단계인 Start로 시작
        ChangePhase(GamePhase.Start);
    }

    private void Update()
    {
        // 파밍 페이즈일 때만 타이머 작동
        if (currentPhase == GamePhase.Farming)
        {
            UpdateFarmingTimer();
        }
    }

    // 단계 전환 핵심 함수
    public void ChangePhase(GamePhase newPhase)
    {
        currentPhase = newPhase;

        // 모든 패널 일단 끄기
        startPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        selectionPanel.SetActive(false);
        farmingHUD.SetActive(false);
        cookingPanel.SetActive(false);
        resultPanel.SetActive(false);

        // 해당되는 패널만 켜기
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
                // 여기서 RecipeManager.Instance.SelectRandomRecipe() 호출 가능
                break;
            case GamePhase.Farming:
                farmingHUD.SetActive(true);
                timer = farmingTime; // 타이머 초기화
                break;
            case GamePhase.Cooking:
                cookingPanel.SetActive(true);
                // 여기서 조리 카운트다운(3, 2, 1) 연출 시작 
                break;
            case GamePhase.Result:
                resultPanel.SetActive(true);
                break;
        }
    }

    // 파밍 타이머 로직
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
            // 시간이 다 되면 자동으로 조리 단계로 전환 [cite: 13]
            ChangePhase(GamePhase.Cooking);
        }
    }

    private void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // 각 버튼에서 호출할 public 함수들
    public void OnClickStart() => ChangePhase(GamePhase.Tutorial);
    public void OnClickPlay() => ChangePhase(GamePhase.MenuSelection);
    public void OnSelectionComplete() => ChangePhase(GamePhase.Farming);
}