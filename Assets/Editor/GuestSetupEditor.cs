using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public static class GuestSetupEditor
{
    const string VN_FONT_PATH = "Assets/TextMesh Pro/Fonts/Maplestory Bold SDF.asset";
    const string VN_NAMEBOX_SPRITE_PATH = "Assets/Sprites/17837.png";

    [MenuItem("Tools/Setup Cooking & Result Panels (VN Style)")]
    public static void SetupVNPanels()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(VN_FONT_PATH);
        if (font == null) Debug.LogWarning("[VNSetup] Font not found: " + VN_FONT_PATH);

        var nameboxSprite = AssetDatabase.LoadAssetAtPath<Sprite>(VN_NAMEBOX_SPRITE_PATH);
        if (nameboxSprite == null) Debug.LogWarning("[VNSetup] NameBox sprite not found: " + VN_NAMEBOX_SPRITE_PATH);

        var cookingPanel = FindInactiveGO("CookingPanel");
        var resultPanel  = FindInactiveGO("ResultPanel");

        if (cookingPanel == null) { Debug.LogError("[VNSetup] CookingPanel not found"); return; }
        if (resultPanel  == null) { Debug.LogError("[VNSetup] ResultPanel not found");  return; }

        VNClearChildren(cookingPanel);
        VNClearChildren(resultPanel);

        var (cookingCharImg, cookingFillImg, cookingStatusTxt) = VNSetupCookingPanel(cookingPanel, font);
        var (resultCharImg, resultScoreTMP, resultNameTMP, resultNextIndicatorGO, resultRestartBtnGO) = VNSetupResultPanel(resultPanel, font, nameboxSprite);

        var gpm = Object.FindAnyObjectByType<GamePhaseManager>(FindObjectsInactive.Include);
        if (gpm != null)
        {
            gpm.resultScoreText       = resultScoreTMP;
            gpm.cookingCharImage      = cookingCharImg;
            gpm.resultCharImage       = resultCharImg;
            gpm.resultNameText        = resultNameTMP;
            gpm.cookingFillImage      = cookingFillImg;
            gpm.cookingStatusText     = cookingStatusTxt;
            gpm.resultNextIndicator   = resultNextIndicatorGO;
            gpm.resultRestartButtonGO = resultRestartBtnGO;
            EditorUtility.SetDirty(gpm);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[VNSetup] 완료! cooking={cookingCharImg?.name}, result={resultCharImg?.name}, score={resultScoreTMP?.name}");
    }

    static (Image charImg, Image fillImg, TextMeshProUGUI statusText)
        VNSetupCookingPanel(GameObject panel, TMP_FontAsset font)
    {
        const string BAR_BG_PATH   = "Assets/Skyden_Games/Free_Casual_GUI/Demo/Sprites/Bars/Bar.png";
        const string BAR_FILL_PATH = "Assets/Skyden_Games/Free_Casual_GUI/Demo/Sprites/Bars/Bar_Fill_01.png";

        var barBgSprite   = AssetDatabase.LoadAssetAtPath<Sprite>(BAR_BG_PATH);
        var barFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BAR_FILL_PATH);

        // 1. Background overlay
        var bg = VNMakeChild(panel, "Background");
        VNAddStretchImage(bg, new Color(0f, 0f, 0f, 0.75f));

        // 2. Guest character image (left)
        var charGO  = VNMakeChild(panel, "CookingCharacterImage");
        var charImg = charGO.AddComponent<Image>();
        charImg.preserveAspect = true;
        charImg.color = Color.clear;
        VNSetRT(charGO, new Vector2(0f,0f), new Vector2(0f,0f), new Vector2(280f,420f), new Vector2(450f,680f));

        // 3. Dialogue box (bottom) — status text
        var boxGO = VNMakeChild(panel, "CookingDialogueBox");
        VNAddStretchImage(boxGO, new Color(0.05f,0.05f,0.05f,0.88f));
        VNSetRT(boxGO, new Vector2(0f,0f), new Vector2(1f,0f), new Vector2(0f,90f), new Vector2(0f,180f));

        var statusGO  = VNMakeChild(boxGO, "CookingStatusText");
        var statusTMP = statusGO.AddComponent<TextMeshProUGUI>();
        statusTMP.text      = "요리를 준비하는 중...";
        statusTMP.fontSize  = 38f;
        statusTMP.color     = Color.white;
        statusTMP.alignment = TextAlignmentOptions.Center;
        if (font != null) statusTMP.font = font;
        VNSetRT(statusGO, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-80f,-30f));

        // 4. Progress bar (above dialogue box)
        var barContainerGO = VNMakeChild(panel, "CookingBarContainer");
        VNSetRT(barContainerGO, new Vector2(0.5f,0f), new Vector2(0.5f,0f),
                new Vector2(0f, 220f), new Vector2(800f, 60f));

        // 4-1. Bar background
        var barBgGO  = VNMakeChild(barContainerGO, "BarBG");
        var barBgImg = barBgGO.AddComponent<Image>();
        if (barBgSprite != null) { barBgImg.sprite = barBgSprite; barBgImg.type = Image.Type.Sliced; }
        else barBgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        VNSetRT(barBgGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // 4-2. Bar fill (Image.Filled → fillAmount animated at runtime)
        var barFillGO  = VNMakeChild(barContainerGO, "BarFill");
        var barFillImg = barFillGO.AddComponent<Image>();
        if (barFillSprite != null) barFillImg.sprite = barFillSprite;
        else barFillImg.color = new Color(0.2f, 0.8f, 0.3f, 1f);
        barFillImg.type       = Image.Type.Filled;
        barFillImg.fillMethod = Image.FillMethod.Horizontal;
        barFillImg.fillOrigin = 0; // left to right
        barFillImg.fillAmount = 0f;
        VNSetRT(barFillGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        return (charImg, barFillImg, statusTMP);
    }

    static (Image charImg, TextMeshProUGUI scoreText, TextMeshProUGUI nameText, GameObject nextIndicator, GameObject restartBtn)
        VNSetupResultPanel(GameObject panel, TMP_FontAsset font, Sprite nameboxSprite)
    {
        var bg = VNMakeChild(panel, "Background");
        VNAddStretchImage(bg, new Color(0f, 0f, 0f, 0.75f));

        var charGO  = VNMakeChild(panel, "ResultCharacterImage");
        var charImg = charGO.AddComponent<Image>();
        charImg.preserveAspect = true;
        charImg.color = Color.clear;
        VNSetRT(charGO, new Vector2(0f,0f), new Vector2(0f,0f), new Vector2(280f,420f), new Vector2(450f,680f));

        var boxGO = VNMakeChild(panel, "ResultDialogueBox");
        VNAddStretchImage(boxGO, new Color(0.05f,0.05f,0.05f,0.88f));
        VNSetRT(boxGO, new Vector2(0f,0f), new Vector2(1f,0f), new Vector2(0f,170f), new Vector2(0f,340f));

        TextMeshProUGUI nameTMP = null;
        if (nameboxSprite != null)
        {
            var nameBoxGO  = VNMakeChild(boxGO, "ResultNameBox");
            var nameBoxImg = nameBoxGO.AddComponent<Image>();
            nameBoxImg.sprite = nameboxSprite;
            nameBoxImg.color  = new Color(1f,1f,1f,0.95f);
            VNSetRT(nameBoxGO, new Vector2(0f,1f), new Vector2(0f,1f), new Vector2(170f,0f), new Vector2(396f,127f));

            var ntGO  = VNMakeChild(nameBoxGO, "ResultNameText");
            nameTMP   = ntGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text      = "";
            nameTMP.fontSize  = 36f;
            nameTMP.color     = Color.black;
            nameTMP.alignment = TextAlignmentOptions.Center;
            if (font != null) nameTMP.font = font;
            VNSetRT(ntGO, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f,-10f));
        }

        var scoreGO  = VNMakeChild(boxGO, "ResultScoreText");
        var scoreTMP = scoreGO.AddComponent<TextMeshProUGUI>();
        scoreTMP.text              = "";
        scoreTMP.fontSize          = 34f;
        scoreTMP.color             = Color.white;
        scoreTMP.alignment         = TextAlignmentOptions.Center;
        scoreTMP.enableWordWrapping = true;
        if (font != null) scoreTMP.font = font;
        VNSetRT(scoreGO, new Vector2(0f,0f), new Vector2(1f,1f), Vector2.zero, new Vector2(-100f,-140f));

        // "▼" advance indicator — hidden by default, shown between dialogue lines
        var nextGO  = VNMakeChild(boxGO, "ResultNextIndicator");
        var nextTMP = nextGO.AddComponent<TextMeshProUGUI>();
        nextTMP.text      = "▼";
        nextTMP.fontSize  = 28f;
        nextTMP.color     = Color.yellow;
        nextTMP.alignment = TextAlignmentOptions.BottomRight;
        if (font != null) nextTMP.font = font;
        VNSetRT(nextGO, Vector2.zero, Vector2.one, new Vector2(-20f, 15f), Vector2.zero);
        nextGO.SetActive(false);

        var btnGO  = VNMakeChild(panel, "RestartButton");
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.95f,0.75f,0.15f,1f);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        VNSetRT(btnGO, new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(0f,40f), new Vector2(300f,70f));
        btnGO.SetActive(false); // hidden until dialogue completes

        var labelGO  = VNMakeChild(btnGO, "ButtonLabel");
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = "다시 시작";
        labelTMP.fontSize  = 36f;
        labelTMP.color     = Color.black;
        labelTMP.alignment = TextAlignmentOptions.Center;
        if (font != null) labelTMP.font = font;
        VNSetRT(labelGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var gpm = Object.FindAnyObjectByType<GamePhaseManager>(FindObjectsInactive.Include);
        if (gpm != null)
            UnityEventTools.AddVoidPersistentListener(btn.onClick, gpm.OnClickRestart);

        return (charImg, scoreTMP, nameTMP, nextGO, btnGO);
    }

    static void VNSetRT(GameObject go, Vector2 anchorMin, Vector2 anchorMax,
                        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        if (!go.TryGetComponent<RectTransform>(out var rt))
            rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;
    }

    static void VNAddStretchImage(GameObject go, Color color)
    {
        if (!go.TryGetComponent<Image>(out var img)) img = go.AddComponent<Image>();
        img.color = color;
        VNSetRT(go, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    static GameObject VNMakeChild(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static void VNClearChildren(GameObject parent)
    {
        for (int i = parent.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.transform.GetChild(i).gameObject);
    }

    static GameObject FindInactiveGO(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.scene.isLoaded && go.name == name) return go;
        return null;
    }

    private const string DIALOGUE_PATH = "Assets/Data/Dialogues";
    private const string GUEST_PATH = "Assets/Data/Guests";

    [MenuItem("Tools/Setup Guests and Dialogues")]
    public static void SetupGuestsAndDialogues()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(DIALOGUE_PATH))
            AssetDatabase.CreateFolder("Assets/Data", "Dialogues");
        if (!AssetDatabase.IsValidFolder(GUEST_PATH))
            AssetDatabase.CreateFolder("Assets/Data", "Guests");

        Sprite p1Sprite   = LoadSprite("Assets/Sprites/Character/player1character.png", "IMG_4749_0");
        Sprite p2Sprite   = LoadSprite("Assets/Sprites/Character/player2character.png", "IMG_4754_0");
        Sprite owlSpr     = LoadSprite("Assets/Sprites/Character/owlman.png",           "IMG_4770_1");
        Sprite armorSpr   = LoadSprite("Assets/Sprites/Character/armorwoman.png",       "IMG_4766_0");
        Sprite baguniSpr  = LoadSprite("Assets/Sprites/Character/baguniwoman.png",      "IMG_4758 (1)_0");
        Sprite gentleSpr  = LoadSprite("Assets/Sprites/Character/gentleman.png",        "IMG_4775_0");

        Sprite owlUI    = LoadSprite("Assets/Sprites/UI/owlmanchatui.png",    "KakaoTalk_20260516_222558684_0");
        Sprite armorUI  = LoadSprite("Assets/Sprites/UI/armorwomanchatui.png","KakaoTalk_20260516_222558684_01_0");
        Sprite baguniUI = LoadSprite("Assets/Sprites/UI/baguniwomanui.png",   "KakaoTalk_20260516_222558684_02_0");
        Sprite gentleUI = LoadSprite("Assets/Sprites/UI/gentlemanchatui.png", "KakaoTalk_20260516_222558684_03_0");

        // 1. 부엉이 정장 심사위원
        var owlDialogue = CreateDialogue("Dialogue_owlman", new List<DialogueLine>
        {
            L("손님",             "호오... 숲의 소문대로군. 여기가 그 유명하다는 음식점인가?", owlSpr),
            L("플레이어 1",       "어서오세요! 최고의 음식점!", p1Sprite),
            L("플레이어 2",       "\"한세유희진\"에 어서오세요.", p2Sprite),
            L("손님",             "반갑네 젊은이들. 내가 여기 근처 밤나무 아래에서 백발의 아주 묘한 아우라를 풍기는 할아버지 셰프를 만났는데 말이지. 내 까다로운 부리를 만족시켜 줄 뛰어난 젊은 요리사 2명이 이 둥지에 있다고 들었네.", owlSpr),
            L("플레이어 1",       "물론이죠! 먹고 싶은걸 말만 하세요!", p1Sprite),
            L("플레이어 2",       "딱 보니 스승님이구만..", p2Sprite),
            L("플레이어 1",       "최고로 신선한 식자재 트럭만 오면 바로 요리해드릴게요!", p1Sprite),
            L("손님",             "흠, 식자재 트럭이라...? 설마 깃털처럼 하얀색 바탕에 '신선물산'이라고 적힌 그 트럭을 말하는 건가?", owlSpr),
            L("플레이어 1",       "네!", p1Sprite),
            L("손님",             "그거 참 안타깝게 되었군. 내가 밤눈이 밝아 야간비행 중에 똑똑히 보았는데... 그 트럭이 코너를 돌다가 시원하게 엎어져서 숲길에 처박혀버렸다네?", owlSpr),
            L2("플레이어 1 & 플레이어 2", "예?????????????????", p1Sprite, p2Sprite),
        });
        var owlGuest = CreateGuest("Guest_owlman", "부엉이 정장 심사위원", owlSpr, owlUI, owlDialogue,
            new List<string>
            {
                "흠… 부엉이 인생 42년, 드디어 눈을 감고도 맛이 보이는군.",
                "이건 요리가 아니라 야간비행이다. 아주 우아해.",
                "맛이 너무 정확해서 내 깃털이 정장을 다시 다렸네.",
                "훌륭하다. 지금 내 눈이 원래 큰 건지 감동한 건지 모르겠군.",
                "이 한입으로 오늘의 숲 회의는 취소다. 다들 먹어야 하니까.",
                "간이 완벽하다. 누가 내 부리를 고급 레스토랑으로 만들었지?",
                "이건 손님한테 내도 된다. 아니, 손님을 이리로 불러라.",
                "음… 맛있군. 너무 맛있어서 의심스럽다.",
                "이 요리는 밤에도 빛난다. 부엉이 인증이다.",
            },
            new List<string>
            {
                "이건 요리가 아니라 바구니 안에서 일어난 사고다.",
                "한입 먹는데 내 날개가 퇴사 신청서를 냈다.",
                "음… 맛이 너무 어두워서 나도 못 보겠군.",
                "부엉이는 야행성인데, 이 맛은 피하고 싶다.",
                "이건 음식이 아니라 새장 바닥의 기억이다.",
                "누가 갈비찜에 절망을 넣었나?",
                "내 부리가 방금 '다시는 열지 마'라고 했다.",
                "정중하게 말하지. 이건 숲으로 돌려보내라.",
                "맛 평가를 해야 하는데 생존 평가부터 해야겠군.",
            });

        // 2. 해골투구 기사
        var armorDialogue = CreateDialogue("Dialogue_armorwoman", new List<DialogueLine>
        {
            L("손님",             "크크크... 피와 전장의 냄새를 따라왔거늘, 여기가 그 전설적인 전장의 대피소... 음식점인가!", armorSpr),
            L("플레이어 1",       "어서오세요! 최고의 음식점!", p1Sprite),
            L("플레이어 2",       "\"한세유희진\"에 어서오세요.", p2Sprite),
            L("손님",             "반갑다, 주방의 전사들이여! 내 이 근처 황야에서 백발의 아주 강력한 패기를 뿜어내는 베테랑 셰프를 만났다. 그 자가 내 검붉은 미각의 전투력을 충족시켜 줄 전력의 젊은 요리사 2명이 이 요새에 상주하고 있다고 하더군!", armorSpr),
            L("플레이어 1",       "물론이죠! 먹고 싶은걸 말만 하세요!", p1Sprite),
            L("플레이어 2",       "딱 보니 스승님이구만..", p2Sprite),
            L("플레이어 1",       "최고로 신선한 식자재 트럭만 오면 바로 요리해드릴게요!", p1Sprite),
            L("손님",             "크윽... 보급 수송 마차(트럭)라고? 혹시 그 깃발의 이름이... '신선물산'이었나?", armorSpr),
            L("플레이어 1",       "네!", p1Sprite),
            L("손님",             "어둠의 저주인가... 그 트럭, 내가 이 성채로 진격하던 도중 마왕의 함정에 빠진 것처럼 도로 위에서 시원하게 엎어져 궤멸한 것을 목격했다!", armorSpr),
            L2("플레이어 1 & 플레이어 2", "예?????????????????", p1Sprite, p2Sprite),
        });
        var armorGuest = CreateGuest("Guest_armorwoman", "해골투구 기사", armorSpr, armorUI, armorDialogue,
            new List<string>
            {
                "훌륭하다. 이 요리는 전쟁을 멈추고 밥상을 세운다.",
                "한입 먹는 순간 내 갑옷이 박수쳤다.",
                "이 맛… 왕국 하나를 바칠 가치가 있다.",
                "좋다. 오늘부터 너는 주방의 기사다.",
                "내 검은 내려놓겠다. 숟가락이 더 강하다.",
                "이건 요리가 아니다. 승리 선언문이다.",
                "불맛이 살아있군. 드래곤도 부러워할 것이다.",
                "이 한 접시라면 마왕도 예약하고 온다.",
                "맛의 방패, 향의 검, 완벽한 전투식량이다.",
            },
            new List<string>
            {
                "이 요리는 적군에게도 주면 안 된다.",
                "한입 먹었더니 내 투구가 닫혔다. 본능적으로.",
                "이건 저주인가, 반찬인가.",
                "전투 중에도 이런 건 안 먹는다.",
                "내 갑옷이 녹슬기 시작했다. 맛 때문인 듯하다.",
                "요리라기보단 패배 조건에 가깝군.",
                "이걸 먹느니 방패를 삶아 먹겠다.",
                "왕국이 멸망한 이유를 알 것 같다.",
                "맛이 칼을 들고 나를 공격했다.",
            });

        // 3. 꽃바구니 머리 캐릭터
        var baguniDialogue = CreateDialogue("Dialogue_baguniwoman", new List<DialogueLine>
        {
            L("손님",             "우와아...! 내 머리 위 민들레들이 춤을 추고 있어! 여기가 소문으로 들은 엄청 예쁜 음식점이구나!", baguniSpr),
            L("플레이어 1",       "어서오세요! 최고의 음식점!", p1Sprite),
            L("플레이어 2",       "\"한세유희진\"에 어서오세요.", p2Sprite),
            L("손님",             "안녕! 내 꽃들이 너희를 보고 반갑대! 실은 여기 오는 길에 백발의 아주 은은한 향기가 나는 할아버지 셰프님을 만났거든? 내 머리 위 꽃들이 꼭 먹어보고 싶어하는 요리를 마법처럼 맞춰주는 비밀의 요리사 2명이 요 앞에 살고 있대서 놀러 왔어!", baguniSpr),
            L("플레이어 1",       "물론이죠! 먹고 싶은걸 말만 하세요!", p1Sprite),
            L("플레이어 2",       "딱 보니 스승님이구만..", p2Sprite),
            L("플레이어 1",       "최고로 신선한 식자재 트럭만 오면 바로 요리해드릴게요!", p1Sprite),
            L("손님",             "웅? 식자재 트럭...? 혹시 알록달록 무지개꽃 같은 '신선물산' 트럭 말하는 거야?", baguniSpr),
            L("플레이어 1",       "네!", p1Sprite),
            L("손님",             "어쩌지... 내 새싹들이 방금 슬픈 소식을 들었어. 그 트럭, 코너 돌다가 대굴대굴 굴러서 시원하게 엎어져 버렸대! 완전 봄이 취소된 분위기야...!", baguniSpr),
            L2("플레이어 1 & 플레이어 2", "예?????????????????", p1Sprite, p2Sprite),
        });
        var baguniGuest = CreateGuest("Guest_baguniwoman", "꽃바구니 머리 캐릭터", baguniSpr, baguniUI, baguniDialogue,
            new List<string>
            {
                "와… 내 머리 꽃들이 지금 단체로 기립박수 중이야.",
                "맛있다! 방금 바구니 안에서 봄이 열렸어.",
                "이거 먹고 내 꽃들이 전부 만개했어. 합격.",
                "음… 이건 요리라기보다 꽃밭에 누워서 갈비 먹는 기분.",
                "내 머리에서 꿀벌 예약 문의 들어왔다.",
                "맛이 너무 좋아서 바구니가 살짝 커진 것 같아.",
                "이건 손님한테 주면 웃으면서 두 그릇 먹어.",
                "방금 데이지가 '미쳤다'고 했어.",
                "맛있어서 내 꽃들이 계절을 바꿨어.",
            },
            new List<string>
            {
                "한입 먹자마자 내 꽃들이 조용해졌어.",
                "이거 먹고 바구니 안에 바람이 안 불어.",
                "내 머리 꽃들이 지금 회의 중이야. 안 좋은 쪽으로.",
                "음… 맛이 흙이랑 친한 것 같아.",
                "이건 요리야? 아니면 화분 분갈이야?",
                "내 꽃들이 방금 '다음 판 가자'고 했어.",
                "먹었는데 봄이 취소됐어.",
                "방금 튤립 하나가 눈치 보면서 시들었어.",
                "이 맛은 꽃말로 '도망'이야.",
            });

        // 4. 연기상자 머리 신사
        var gentleDialogue = CreateDialogue("Dialogue_gentleman", new List<DialogueLine>
        {
            L("손님",             "안녕하십니까. 제 내부 레이더가 가리키는 미스터리한 장소가 바로 여기군요. 유명한 음식점이 확실해 보입니다.", gentleSpr),
            L("플레이어 1",       "어서오세요! 최고의 음식점!", p1Sprite),
            L("플레이어 2",       "\"한세유희진\"에 어서오세요.", p2Sprite),
            L("손님",             "처음 뵙겠습니다, 젊은 조리 장인 여러분. 제가 방금 전 철학적인 골목길에서 백발의 아주 정중하고 기묘한 아우라가 넘치는 마스터 셰프를 접견했습니다. 제 연기 배출구를 만족시켜 줄 천재적인 젊은 요리사 2명이 이 좌표에 근무 중이라고 정중히 안내받았습니다.", gentleSpr),
            L("플레이어 1",       "물론이죠! 먹고 싶은걸 말만 하세요!", p1Sprite),
            L("플레이어 2",       "딱 보니 스승님이구만..", p2Sprite),
            L("플레이어 1",       "최고로 신선한 식자재 트럭만 오면 바로 요리해드릴게요!", p1Sprite),
            L("손님",             "흠... 식자재를 운송하는 디바이스(트럭) 말씀이십니까? 혹시 그 상호명이 '신선물산' 기체입니까?", gentleSpr),
            L("플레이어 1",       "네!", p1Sprite),
            L("손님",             "대단히 유감스러운 연기를 뿜어야겠군요. 제 회로가 방금 확인한 사건 지평선에 따르면, 그 트럭은 이미 중력 제어에 실패하여 도로변에 시원하게 엎어져 있는 상태입니다.", gentleSpr),
            L2("플레이어 1 & 플레이어 2", "예?????????????????", p1Sprite, p2Sprite),
        });
        var gentleGuest = CreateGuest("Guest_gentleman", "연기상자 머리 신사", gentleSpr, gentleUI, gentleDialogue,
            new List<string>
            {
                "훌륭합니다. 제 머리에서 고급 연기가 나오는군요.",
                "이 요리는 입 안에서 회의가 열렸고, 만장일치로 통과했습니다.",
                "맛이 아주 정중하군요. 혀에게 명함을 건넸습니다.",
                "한입 먹었더니 제 안개가 향기로 바뀌었습니다.",
                "이건 요리라기보다 잘 차려진 미스터리입니다.",
                "제 머리 속 연기가 박수를 치고 있습니다. 이상하지만 사실입니다.",
                "완벽합니다. 이 접시는 예의를 갖춘 폭발입니다.",
                "손님이 원한 것보다 더 손님 같은 맛입니다.",
                "맛이 너무 고급스러워서 제가 잠시 가전제품임을 잊었습니다.",
                "100점 드리겠습니다. 연기가 숫자 모양으로 나왔거든요.",
            },
            new List<string>
            {
                "죄송합니다. 제 머리에서 비상 연기가 나고 있습니다.",
                "이 요리는 입장이 불분명합니다. 음식인지 사건인지.",
                "한입 먹었더니 내부 회로가 예의를 잃었습니다.",
                "맛이 너무 복잡해서 제 안개가 길을 잃었습니다.",
                "이건 손님에게 내기 전에 사과문부터 준비해야 합니다.",
                "조심스럽게 말하겠습니다. 접시가 제일 맛있어 보입니다.",
                "방금 제 머리 속에서 경고음이 정중하게 울렸습니다.",
                "요리의 방향성은 있습니다. 문제는 그 방향이 출구입니다.",
                "제 연기가 검게 변했습니다. 불만족의 색입니다.",
            });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        WireGuestsToManager();
        Debug.Log("[GuestSetup] 완료! GuestSO x4 / DialogueData x4 에셋 생성 및 스프라이트 연결 완료.");
    }

    [MenuItem("Tools/Wire Guests to GamePhaseManager")]
    public static void WireGuestsToManager()
    {
        GamePhaseManager gpm = Object.FindAnyObjectByType<GamePhaseManager>(FindObjectsInactive.Include);
        if (gpm == null) { Debug.LogError("GamePhaseManager를 찾을 수 없습니다."); return; }

        gpm.guestList = new List<GuestSO>
        {
            AssetDatabase.LoadAssetAtPath<GuestSO>($"{GUEST_PATH}/Guest_owlman.asset"),
            AssetDatabase.LoadAssetAtPath<GuestSO>($"{GUEST_PATH}/Guest_armorwoman.asset"),
            AssetDatabase.LoadAssetAtPath<GuestSO>($"{GUEST_PATH}/Guest_baguniwoman.asset"),
            AssetDatabase.LoadAssetAtPath<GuestSO>($"{GUEST_PATH}/Guest_gentleman.asset"),
        };
        EditorUtility.SetDirty(gpm);
        Debug.Log($"[GuestSetup] GamePhaseManager guestList 연결 완료! ({gpm.guestList.Count}명)");
    }

    [MenuItem("Tools/Fix StartMap Material (Unlit)")]
    public static void FixStartMapMaterial()
    {
        var unlitMat = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
        if (unlitMat == null)
        {
            Debug.LogError("Sprite-Unlit-Default.mat을 찾을 수 없습니다.");
            return;
        }

        int count = 0;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.isLoaded) continue;
            // Grid(start) 자식 Tilemap 렌더러만 변경
            var tr = go.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>();
            if (tr == null) continue;
            // 부모가 Grid(start)인지 확인
            Transform p = go.transform.parent;
            if (p == null || p.name != "Grid(start)") continue;
            tr.material = unlitMat;
            EditorUtility.SetDirty(tr);
            count++;
        }
        Debug.Log($"[StartMap] Grid(start) TilemapRenderer {count}개 → Sprite-Unlit-Default 변경 완료");
    }

    [MenuItem("Tools/Setup GamePhase Camera References")]
    public static void SetupCameraReferences()
    {
        GamePhaseManager gpm = Object.FindAnyObjectByType<GamePhaseManager>(FindObjectsInactive.Include);
        if (gpm == null) { Debug.LogError("GamePhaseManager를 찾을 수 없습니다."); return; }

        // CameraFollow 컴포넌트를 가진 카메라 탐색 (viewport x 위치로 구분)
        CameraFollow cam1 = null, cam2 = null;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.isLoaded) continue;
            var cf = go.GetComponent<CameraFollow>();
            if (cf == null) continue;
            var cam = go.GetComponent<Camera>();
            if (cam == null) continue;
            if (cam.rect.x < 0.1f) cam1 = cf;
            else cam2 = cf;
        }

        if (cam1 != null) gpm.player1Cam = cam1;
        if (cam2 != null) gpm.player2Cam = cam2;

        // StartCameraAnchor 탐색
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.scene.isLoaded && go.name == "StartCameraAnchor")
            {
                gpm.startCameraAnchor = go.transform;
                break;
            }
        }

        // StartCamera 연결
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.scene.isLoaded && go.name == "StartCamera")
            {
                gpm.startCamera = go.GetComponent<Camera>();
                break;
            }
        }

        EditorUtility.SetDirty(gpm);
        Debug.Log($"[GamePhaseSetup] 완료! p1Cam={gpm.player1Cam}, p2Cam={gpm.player2Cam}, anchor={gpm.startCameraAnchor}, startCam={gpm.startCamera}");
    }

    // ── CardDisplaySettings 씬 배치 + 연결 ──────────────────────────────────

    [MenuItem("Tools/Setup Card Display Settings")]
    public static void SetupCardDisplaySettings()
    {
        // CardDisplaySettings 오브젝트 생성 또는 찾기
        CardDisplaySettings cds = Object.FindAnyObjectByType<CardDisplaySettings>(FindObjectsInactive.Include);
        if (cds == null)
        {
            var go = new GameObject("CardDisplaySettings");
            cds = go.AddComponent<CardDisplaySettings>();
        }

        // Bazzi SDF 폰트 연결
        var font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Fonts/Bazzi SDF.asset");
        if (font != null) cds.bazziFont = font;
        else Debug.LogWarning("[CardDisplay] Bazzi SDF.asset 못 찾음");

        // SpecialCard 프리팹 연결
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UIprefabs/SpecialCard.prefab");
        if (prefab != null) cds.specialCardNotifyPrefab = prefab;
        else Debug.LogWarning("[CardDisplay] SpecialCard.prefab 못 찾음");

        // IngreCard 프리팹 연결
        var ingrePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UIprefabs/IngreCard.prefab");
        if (ingrePrefab != null) cds.ingreCardNotifyPrefab = ingrePrefab;
        else Debug.LogWarning("[CardDisplay] IngreCard.prefab 못 찾음");

        EditorUtility.SetDirty(cds);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[CardDisplay] CardDisplaySettings 설정 완료!");
    }

    [MenuItem("Tools/Wire Player Canvases to PlayerStatus")]
    public static void WirePlayerCanvases()
    {
        WirePlayerCanvas("Player1 Camera", "player1(cook)");
        WirePlayerCanvas("Player2 Camera", "player2(Boy)");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[PlayerCanvas] playerCanvas 연결 완료!");
    }

    private static void WirePlayerCanvas(string cameraName, string playerName)
    {
        GameObject camGO = null, playerGO = null;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.isLoaded) continue;
            if (go.name == cameraName) camGO = go;
            if (go.name == playerName) playerGO = go;
        }
        if (camGO == null || playerGO == null) return;

        var canvasTr = camGO.transform.Find("Canvas");
        if (canvasTr == null) return;
        var canvas = canvasTr.GetComponent<UnityEngine.Canvas>();
        if (canvas == null) return;

        var status = playerGO.GetComponent<PlayerStatus>();
        if (status == null) return;

        status.playerCanvas = canvas;
        EditorUtility.SetDirty(status);
        Debug.Log($"[PlayerCanvas] {playerName}.playerCanvas = {cameraName}/Canvas");
    }

    [MenuItem("Tools/Revert Inventory Scroll")]
    public static void RevertInventoryScroll()
    {
        RevertScrollForPopup("P1_InventoryPopup");
        RevertScrollForPopup("P2_InventoryPopup");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[InvRevert] 인벤토리 스크롤 되돌리기 완료!");
    }

    private static void RevertScrollForPopup(string popupName)
    {
        GameObject popupGO = null;
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var found = FindDeepChild(root.transform, popupName);
            if (found != null) { popupGO = found.gameObject; break; }
        }
        if (popupGO == null) { Debug.LogWarning($"[InvRevert] {popupName} 못 찾음"); return; }

        // CardViewport 찾기
        Transform vpTr = popupGO.transform.Find("CardViewport");
        if (vpTr == null) { Debug.Log($"[InvRevert] {popupName}: CardViewport 없음 — 이미 되돌려져 있음"); return; }

        // CardContainer 찾기 (CardViewport 아래)
        Transform ccTr = FindDeepChild(vpTr, "CardContainer");
        if (ccTr == null)
        {
            // CardViewport 아래 없으면 popup 아래에서 찾기
            ccTr = FindDeepChild(popupGO.transform, "CardContainer");
        }

        if (ccTr != null)
        {
            // CardContainer의 ContentSizeFitter 제거
            var csf = ccTr.GetComponent<ContentSizeFitter>();
            if (csf != null)
            {
                Object.DestroyImmediate(csf);
                Debug.Log($"[InvRevert] {popupName}/CardContainer ContentSizeFitter 제거");
            }

            // CardContainer를 popup 직속 자식으로 이동
            ccTr.SetParent(popupGO.transform, false);
            var ccRt = ccTr.GetComponent<RectTransform>();
            if (ccRt != null)
            {
                // CardViewport와 동일한 위치/크기로 복원
                var vpRt = vpTr.GetComponent<RectTransform>();
                if (vpRt != null)
                {
                    ccRt.anchorMin        = vpRt.anchorMin;
                    ccRt.anchorMax        = vpRt.anchorMax;
                    ccRt.anchoredPosition = vpRt.anchoredPosition;
                    ccRt.sizeDelta        = vpRt.sizeDelta;
                }
            }
            EditorUtility.SetDirty(ccTr.gameObject);
            Debug.Log($"[InvRevert] {popupName} CardContainer → {popupName} 직속 자식으로 복원");
        }

        // CardViewport 삭제
        Object.DestroyImmediate(vpTr.gameObject);
        Debug.Log($"[InvRevert] {popupName}/CardViewport 삭제");

        // InventoryUI의 cardScrollRect 초기화
        foreach (var ui in Resources.FindObjectsOfTypeAll<InventoryUI>())
        {
            if (ui.uiPanel == popupGO)
            {
                ui.cardScrollRect = null;
                EditorUtility.SetDirty(ui);
                Debug.Log($"[InvRevert] {ui.gameObject.name} cardScrollRect 초기화");
                break;
            }
        }
    }

    [MenuItem("Tools/Setup Inventory Scroll")]
    public static void SetupInventoryScroll()
    {
        SetupScrollForPopup("P1_InventoryPopup", KeyCode.W, KeyCode.S);
        SetupScrollForPopup("P2_InventoryPopup", KeyCode.UpArrow, KeyCode.DownArrow);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[InvScroll] 인벤토리 스크롤 설정 완료!");
    }

    private static Transform FindDeepChild(Transform root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform child in root)
        {
            var found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private static void SetupScrollForPopup(string popupName, KeyCode altPrev, KeyCode altNext)
    {
        // 활성 씬의 루트부터 재귀 탐색 (에셋/프리팹 오브젝트 제외)
        GameObject popupGO = null;
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var found = FindDeepChild(root.transform, popupName);
            if (found != null) { popupGO = found.gameObject; break; }
        }
        if (popupGO == null) { Debug.LogWarning($"[InvScroll] {popupName} 못 찾음"); return; }
        // InventoryUI 먼저 찾고, cardIcons 레퍼런스로 CardContainer 역추적
        InventoryUI invUIEarly = null;
        foreach (var ui in Resources.FindObjectsOfTypeAll<InventoryUI>())
        {
            if (ui.uiPanel == popupGO) { invUIEarly = ui; break; }
        }

        Transform ccTr = null;
        if (invUIEarly != null && invUIEarly.cardIcons != null && invUIEarly.cardIcons.Count > 0
            && invUIEarly.cardIcons[0] != null)
        {
            // cardIcons[0] = ItemCan Image  →  parent = IngreCard  →  parent = CardContainer
            Transform iconTr = invUIEarly.cardIcons[0].transform;
            Transform ingreCard = iconTr.parent;
            if (ingreCard != null) ccTr = ingreCard.parent;
        }

        // 위 방법 실패 시 직접 탐색
        if (ccTr == null) ccTr = FindDeepChild(popupGO.transform, "CardContainer");
        if (ccTr == null) { Debug.LogWarning($"[InvScroll] {popupName}/CardContainer 못 찾음"); return; }
        var ccRt = ccTr.GetComponent<RectTransform>();

        // CardViewport 찾기 또는 생성
        Transform vpTr = popupGO.transform.Find("CardViewport");
        if (vpTr == null)
        {
            var vp = new GameObject("CardViewport");
            vp.transform.SetParent(popupGO.transform, false);
            vpTr = vp.transform;

            var vpRt = vp.AddComponent<RectTransform>();
            vpRt.anchorMin      = ccRt.anchorMin;
            vpRt.anchorMax      = ccRt.anchorMax;
            vpRt.anchoredPosition = ccRt.anchoredPosition;
            vpRt.sizeDelta      = ccRt.sizeDelta;
            vp.AddComponent<RectMask2D>();

            // CardContainer를 Viewport 자식으로 이동
            ccTr.SetParent(vpTr, false);
            ccRt.anchorMin = Vector2.zero;
            ccRt.anchorMax = new Vector2(1f, 1f);
            ccRt.anchoredPosition = Vector2.zero;
            ccRt.sizeDelta = Vector2.zero;

            // GridLayoutGroup이 없을 때만 HorizontalLayoutGroup 추가
            if (ccTr.GetComponent<HorizontalLayoutGroup>() == null &&
                ccTr.GetComponent<GridLayoutGroup>() == null)
            {
                var hlg = ccTr.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childForceExpandWidth  = false;
                hlg.childForceExpandHeight = true;
                hlg.spacing = 8f;
            }
            if (ccTr.GetComponent<ContentSizeFitter>() == null)
            {
                var csf = ccTr.gameObject.AddComponent<ContentSizeFitter>();
                // GridLayoutGroup이면 가로, 아니면 PreferredSize
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            // ScrollRect
            var sr = vp.AddComponent<ScrollRect>();
            sr.content    = ccRt;
            sr.viewport   = vpRt;
            sr.horizontal = true;
            sr.vertical   = false;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 10f;

            // InventoryUI 전체 검색 (popupName.uiPanel 매칭)
            InventoryUI invUI = null;
            foreach (var ui in Resources.FindObjectsOfTypeAll<InventoryUI>())
            {
                if (ui.uiPanel == popupGO) { invUI = ui; break; }
            }
            if (invUI != null)
            {
                invUI.cardScrollRect = sr;
                invUI.altPrevKey = altPrev;
                invUI.altNextKey = altNext;
                EditorUtility.SetDirty(invUI);
                Debug.Log($"[InvScroll] InventoryUI({invUI.gameObject.name}) altPrev={altPrev} altNext={altNext} 연결");
            }
            else Debug.LogWarning($"[InvScroll] uiPanel={popupName}인 InventoryUI를 찾을 수 없음");

            EditorUtility.SetDirty(vp);
            Debug.Log($"[InvScroll] {popupName} CardViewport + ScrollRect 추가 완료");
        }
        else
        {
            var vpRt = vpTr.GetComponent<RectTransform>();

            // CardContainer가 Viewport 아래에 없으면 재부착
            if (ccTr.parent != vpTr)
            {
                ccTr.SetParent(vpTr, false);
                ccRt.anchorMin = Vector2.zero;
                ccRt.anchorMax = Vector2.one;
                ccRt.anchoredPosition = Vector2.zero;
                ccRt.sizeDelta = Vector2.zero;
                EditorUtility.SetDirty(ccTr.gameObject);
                Debug.Log($"[InvScroll] {popupName} CardContainer → CardViewport 재부착");
            }

            // ContentSizeFitter
            if (ccTr.GetComponent<ContentSizeFitter>() == null)
            {
                var csf = ccTr.gameObject.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                EditorUtility.SetDirty(ccTr.gameObject);
            }

            // ScrollRect (없으면 추가)
            var sr = vpTr.GetComponent<ScrollRect>();
            if (sr == null)
            {
                sr = vpTr.gameObject.AddComponent<ScrollRect>();
                sr.content    = ccRt;
                sr.viewport   = vpRt;
                sr.horizontal = true;
                sr.vertical   = false;
                sr.movementType = ScrollRect.MovementType.Clamped;
                sr.scrollSensitivity = 10f;
                EditorUtility.SetDirty(vpTr.gameObject);
            }

            // InventoryUI 연결
            InventoryUI invUI = null;
            foreach (var ui in Resources.FindObjectsOfTypeAll<InventoryUI>())
            {
                if (ui.uiPanel == popupGO) { invUI = ui; break; }
            }
            if (invUI != null)
            {
                invUI.cardScrollRect = sr;
                invUI.altPrevKey = altPrev;
                invUI.altNextKey = altNext;
                EditorUtility.SetDirty(invUI);
                Debug.Log($"[InvScroll] {popupName} 기존 Viewport에 InventoryUI 연결 완료 altPrev={altPrev}");
            }
        }
    }

    // ── 특수카드 SO 에셋 생성 ─────────────────────────────────────────────────

    private const string SPECIAL_PATH = "Assets/Data/SpecialCards";
    private const string ROTTEN_PATH  = "Assets/Data/Ingredients";

    [MenuItem("Tools/Wire Blackout Overlays")]
    public static void WireBlackoutOverlays()
    {
        // Player1 Camera/Canvas/Image → player1(cook) PlayerStatus
        WireBlackout("Player1 Camera", "player1(cook)");
        // Player2 Camera/Canvas/Image → player2(Boy) PlayerStatus
        WireBlackout("Player2 Camera", "player2(Boy)");

        // Canvas를 ScreenSpaceCamera 모드로 변경 (분할화면 각자 영역만 암전)
        FixBlackoutCanvasRenderMode("Player1 Camera");
        FixBlackoutCanvasRenderMode("Player2 Camera");

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[Blackout] 양쪽 플레이어 암전 오버레이 연결 완료!");
    }

    private static void FixBlackoutCanvasRenderMode(string cameraName)
    {
        GameObject camGO = null;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.scene.isLoaded && go.name == cameraName) { camGO = go; break; }
        }
        if (camGO == null) { Debug.LogWarning($"[Blackout] {cameraName} 못 찾음"); return; }

        Camera cam = camGO.GetComponent<Camera>();
        if (cam == null) { Debug.LogWarning($"[Blackout] {cameraName}에 Camera 컴포넌트 없음"); return; }

        Transform canvasTr = camGO.transform.Find("Canvas");
        if (canvasTr == null) { Debug.LogWarning($"[Blackout] {cameraName}/Canvas 없음"); return; }

        Canvas canvas = canvasTr.GetComponent<Canvas>();
        if (canvas == null) return;

        canvas.renderMode  = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 1f;
        EditorUtility.SetDirty(canvas);
        Debug.Log($"[Blackout] {cameraName}/Canvas → ScreenSpaceCamera 모드로 변경");
    }

    private static void WireBlackout(string cameraName, string playerName)
    {
        GameObject camGO = null;
        GameObject playerGO = null;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.isLoaded) continue;
            if (go.name == cameraName) camGO = go;
            if (go.name == playerName) playerGO = go;
        }
        if (camGO == null) { Debug.LogWarning($"[Blackout] {cameraName} 못 찾음"); return; }
        if (playerGO == null) { Debug.LogWarning($"[Blackout] {playerName} 못 찾음"); return; }

        // Find Canvas child → Image child → CanvasGroup
        var canvas = camGO.transform.Find("Canvas");
        if (canvas == null) { Debug.LogWarning($"[Blackout] {cameraName}/Canvas 없음"); return; }
        var imgTr = canvas.Find("Image");
        if (imgTr == null) { Debug.LogWarning($"[Blackout] {cameraName}/Canvas/Image 없음"); return; }
        var cg = imgTr.GetComponent<UnityEngine.CanvasGroup>();
        if (cg == null) { Debug.LogWarning($"[Blackout] Image에 CanvasGroup 없음"); return; }

        var status = playerGO.GetComponent<PlayerStatus>();
        if (status == null) { Debug.LogWarning($"[Blackout] {playerName}에 PlayerStatus 없음"); return; }

        status.blackoutOverlay = cg;
        EditorUtility.SetDirty(status);
        Debug.Log($"[Blackout] {playerName}.blackoutOverlay → {cameraName}/Canvas/Image");
    }

    [MenuItem("Tools/Create Special Card Assets")]
    public static void CreateSpecialCardAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(SPECIAL_PATH))
            AssetDatabase.CreateFolder("Assets/Data", "SpecialCards");
        if (!AssetDatabase.IsValidFolder(ROTTEN_PATH))
            AssetDatabase.CreateFolder("Assets/Data", "Ingredients");

        // 즉시 발동 카드
        MakeSpecialCard("SC_RocketBasket",   "로켓 바구니",    SpecialCardType.RocketBasket,   true);
        MakeSpecialCard("SC_SlowBasket",     "느림보 바구니",  SpecialCardType.SlowBasket,     true);
        MakeSpecialCard("SC_Blackout",       "암전 카드",      SpecialCardType.Blackout,       true);
        MakeSpecialCard("SC_ControlReverse", "조작 반전 카드", SpecialCardType.ControlReverse, true);
        MakeSpecialCard("SC_Stun",           "멍 때리기 카드", SpecialCardType.Stun,           true);
        MakeSpecialCard("SC_DropIngredient", "재료 흘리기",    SpecialCardType.DropIngredient, true);
        MakeSpecialCard("SC_Mold",           "곰팡이",         SpecialCardType.Mold,           true);
        // 보관 가능 카드
        MakeSpecialCard("SC_FreshShield",    "신선 보호막",    SpecialCardType.FreshShield,    false);
        MakeSpecialCard("SC_Wild",           "만능 재료",      SpecialCardType.WildIngredient, false);
        MakeSpecialCard("SC_MSG",            "MSG",            SpecialCardType.MSG,            false);

        // 썩은 재료 IngredientSO
        string rottenPath = $"{ROTTEN_PATH}/RottenIngredient.asset";
        IngredientSO rotten = AssetDatabase.LoadAssetAtPath<IngredientSO>(rottenPath);
        if (rotten == null)
        {
            rotten = ScriptableObject.CreateInstance<IngredientSO>();
            AssetDatabase.CreateAsset(rotten, rottenPath);
        }
        rotten.itemName = "썩은 재료";
        rotten.isRotten = true;
        EditorUtility.SetDirty(rotten);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SpecialCards] 특수카드 10종 + 썩은 재료 에셋 생성 완료!");
    }

    private static SpecialCardSO MakeSpecialCard(string assetName, string cardName,
        SpecialCardType type, bool isInstant)
    {
        string path = $"{SPECIAL_PATH}/{assetName}.asset";
        SpecialCardSO card = AssetDatabase.LoadAssetAtPath<SpecialCardSO>(path);
        if (card == null)
        {
            card = ScriptableObject.CreateInstance<SpecialCardSO>();
            AssetDatabase.CreateAsset(card, path);
        }
        card.cardName = cardName;
        card.cardType = type;
        card.isInstant = isInstant;
        EditorUtility.SetDirty(card);
        return card;
    }

    // ── 바구니 특수 애니메이터 설정 ──────────────────────────────────────────

    [MenuItem("Tools/Setup Basket Special Animator")]
    public static void SetupBasketSpecialAnimator()
    {
        // basket.controller 경로 탐색
        string[] guids = AssetDatabase.FindAssets("basket t:AnimatorController");
        if (guids.Length == 0) { Debug.LogError("[BasketAnim] basket.controller를 찾을 수 없습니다."); return; }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null) { Debug.LogError($"[BasketAnim] AnimatorController 로드 실패: {path}"); return; }

        // OpenSpecial 트리거 파라미터 추가 (중복 방지)
        bool hasParam = false;
        foreach (var p in controller.parameters)
            if (p.name == "OpenSpecial") { hasParam = true; break; }
        if (!hasParam)
        {
            controller.AddParameter("OpenSpecial", AnimatorControllerParameterType.Trigger);
            Debug.Log("[BasketAnim] OpenSpecial 트리거 파라미터 추가");
        }

        // specialbasket 상태 찾기
        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        AnimatorState specialState = null;
        foreach (var cs in sm.states)
            if (cs.state.name == "specialbasket") { specialState = cs.state; break; }

        if (specialState == null) { Debug.LogError("[BasketAnim] 'specialbasket' 상태를 찾을 수 없습니다."); return; }

        // AnyState → specialbasket 트랜지션 중복 확인
        bool hasTransition = false;
        foreach (var t in sm.anyStateTransitions)
            if (t.destinationState == specialState && t.conditions.Length > 0 && t.conditions[0].parameter == "OpenSpecial")
            { hasTransition = true; break; }

        if (!hasTransition)
        {
            var transition = sm.AddAnyStateTransition(specialState);
            transition.AddCondition(AnimatorConditionMode.If, 0, "OpenSpecial");
            transition.hasExitTime = false;
            transition.duration = 0f;
            Debug.Log("[BasketAnim] AnyState → specialbasket (OpenSpecial) 트랜지션 추가");
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log($"[BasketAnim] 완료! ({path})");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static Sprite LoadSprite(string path, string spriteName)
    {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            if (asset is Sprite spr && spr.name == spriteName) return spr;
        Debug.LogWarning($"[GuestSetup] 스프라이트를 찾지 못했습니다: {path} / {spriteName}");
        return null;
    }

    private static DialogueLine L(string name, string text, Sprite spr) =>
        new DialogueLine { characterName = name, text = text, characterSprite = spr };

    private static DialogueLine L2(string name, string text, Sprite spr, Sprite spr2) =>
        new DialogueLine { characterName = name, text = text, characterSprite = spr, characterSprite2 = spr2 };

    private static DialogueData CreateDialogue(string assetName, List<DialogueLine> lines)
    {
        string path = $"{DIALOGUE_PATH}/{assetName}.asset";
        AssetDatabase.DeleteAsset(path);
        var data = ScriptableObject.CreateInstance<DialogueData>();
        data.lines = lines;
        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    private static GuestSO CreateGuest(string assetName, string guestName,
        Sprite charSpr, Sprite boxSpr, DialogueData dialogue,
        List<string> highLines, List<string> lowLines)
    {
        string path = $"{GUEST_PATH}/{assetName}.asset";
        GuestSO guest = AssetDatabase.LoadAssetAtPath<GuestSO>(path);
        if (guest == null)
        {
            guest = ScriptableObject.CreateInstance<GuestSO>();
            AssetDatabase.CreateAsset(guest, path);
        }
        guest.guestName        = guestName;
        guest.characterSprite  = charSpr;
        guest.dialogueBoxSprite = boxSpr;
        guest.introDialogue    = dialogue;
        guest.highScoreLines   = highLines;
        guest.lowScoreLines    = lowLines;
        EditorUtility.SetDirty(guest);
        return guest;
    }
}
