using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardRevealPopup : MonoBehaviour
{
    public static void Spawn(Vector3 worldPos, Sprite icon, string cardName, bool isSpecial = false)
    {
        var go = new GameObject("CardRevealPopup");
        go.transform.position = worldPos + Vector3.up * 0.3f;
        go.AddComponent<CardRevealPopup>().Build(icon, cardName, isSpecial);
    }

    void Build(Sprite icon, string cardName, bool isSpecial)
    {
        const float W = 160f, H = 195f;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 60;
        var cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        var rootRt = GetComponent<RectTransform>();
        rootRt.sizeDelta = new Vector2(W, H);
        transform.localScale = Vector3.zero;

        // 카드 배경
        Color bgColor = isSpecial
            ? new Color(0.30f, 0.10f, 0.50f, 0.93f)
            : new Color(0.98f, 0.94f, 0.82f, 0.93f);
        var bg = MakeRect("BG", transform, Vector2.zero, new Vector2(W, H));
        bg.AddComponent<Image>().color = bgColor;

        // 상단 라벨 띠 (식재료 / 특수카드 구분)
        var badge = MakeRect("Badge", transform, new Vector2(0, 75f), new Vector2(W, 36f));
        var badgeImg = badge.AddComponent<Image>();
        badgeImg.color = isSpecial ? new Color(0.55f, 0.20f, 0.80f, 1f) : new Color(0.70f, 0.55f, 0.25f, 1f);
        var bazziFont = CardDisplaySettings.Instance?.bazziFont;

        var badgeTxt = MakeRect("BadgeTxt", badge.transform, Vector2.zero, new Vector2(W - 10f, 30f));
        var btmp = badgeTxt.AddComponent<TextMeshProUGUI>();
        if (bazziFont != null) btmp.font = bazziFont;
        btmp.text = isSpecial ? "✦ 특수 카드" : "식재료";
        btmp.fontSize = 15f;
        btmp.alignment = TextAlignmentOptions.Center;
        btmp.color = Color.white;

        // 아이콘
        var iconGO = MakeRect("Icon", transform, new Vector2(0, 18f), new Vector2(95f, 95f));
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.preserveAspect = true;
        if (icon != null) iconImg.sprite = icon;
        else iconImg.color = new Color(0, 0, 0, 0.15f);

        // 이름 텍스트
        var nameGO = MakeRect("Name", transform, new Vector2(0, -72f), new Vector2(W - 16f, 40f));
        var ntmp = nameGO.AddComponent<TextMeshProUGUI>();
        if (bazziFont != null) ntmp.font = bazziFont;
        ntmp.text = cardName;
        ntmp.fontSize = 17f;
        ntmp.fontStyle = FontStyles.Bold;
        ntmp.alignment = TextAlignmentOptions.Center;
        ntmp.color = isSpecial ? new Color(0.95f, 0.85f, 1f) : new Color(0.18f, 0.10f, 0.04f);

        StartCoroutine(Animate(cg, isSpecial));
    }

    static GameObject MakeRect(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }

    IEnumerator Animate(CanvasGroup cg, bool isSpecial)
    {
        const float SCALE = 0.01f;
        const float FLOAT_SPEED = 0.4f;

        // 팝인: 0 → 1.15 → 1.0 (bounce)
        for (float t = 0; t < 0.15f; t += Time.deltaTime)
        {
            float s = Mathf.Lerp(0f, 1.15f, t / 0.15f);
            transform.localScale = Vector3.one * SCALE * s;
            cg.alpha = Mathf.Clamp01(t / 0.08f);
            yield return null;
        }
        for (float t = 0; t < 0.12f; t += Time.deltaTime)
        {
            float s = Mathf.Lerp(1.15f, 1.0f, t / 0.12f);
            transform.localScale = Vector3.one * SCALE * s;
            yield return null;
        }
        transform.localScale = Vector3.one * SCALE;
        cg.alpha = 1f;

        // 특수카드: 짧은 흔들림 효과
        if (isSpecial)
        {
            for (float t = 0; t < 0.25f; t += Time.deltaTime)
            {
                float shake = Mathf.Sin(t * 60f) * 0.06f * (1f - t / 0.25f);
                transform.localScale = Vector3.one * SCALE * (1f + shake);
                yield return null;
            }
            transform.localScale = Vector3.one * SCALE;
        }

        // 유지 + 위로 떠오름
        for (float t = 0; t < 0.75f; t += Time.deltaTime)
        {
            transform.position += Vector3.up * FLOAT_SPEED * Time.deltaTime;
            yield return null;
        }

        // 페이드아웃 + 계속 상승
        for (float t = 0; t < 0.35f; t += Time.deltaTime)
        {
            cg.alpha = 1f - t / 0.35f;
            transform.position += Vector3.up * FLOAT_SPEED * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
