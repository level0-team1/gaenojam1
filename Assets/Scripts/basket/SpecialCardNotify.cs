using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpecialCardNotify : MonoBehaviour
{
    // basket.cs의 TriggerInstantCard에서 호출
    public static void ShowOnCanvas(UnityEngine.Canvas canvas, SpecialCardSO card)
    {
        if (canvas == null || card == null) return;
        if (CardDisplaySettings.Instance?.specialCardNotifyPrefab == null) return;

        canvas.sortingOrder = 10;
        var go = Instantiate(CardDisplaySettings.Instance.specialCardNotifyPrefab, canvas.transform);

        // SpecialCard.prefab 구조: 루트 Image(배경), 자식 "ItemCan"(아이콘), 자식 "ItemNext"(텍스트)
        var iconTr = go.transform.Find("ItemCan");
        if (iconTr != null && card.icon != null)
            iconTr.GetComponent<Image>().sprite = card.icon;

        var nameTr = go.transform.Find("ItemNext");
        if (nameTr != null)
            nameTr.GetComponent<TMP_Text>().text = card.cardName;

        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.72f, 0.68f);
            rt.anchorMax = new Vector2(0.72f, 0.68f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.zero;
        }

        go.AddComponent<SpecialCardNotify>();
    }

    IEnumerator Start()
    {
        var cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // 팝인: 0 → 1.1 → 1.0
        for (float t = 0; t < 0.2f; t += Time.deltaTime)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(0f, 1.1f, t / 0.2f);
            cg.alpha = Mathf.Clamp01(t / 0.12f);
            yield return null;
        }
        for (float t = 0; t < 0.1f; t += Time.deltaTime)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(1.1f, 1.0f, t / 0.1f);
            yield return null;
        }
        transform.localScale = Vector3.one;
        cg.alpha = 1f;

        // 유지 2초
        yield return new WaitForSeconds(2f);

        // 페이드아웃 + 축소
        for (float t = 0; t < 0.4f; t += Time.deltaTime)
        {
            float ratio = t / 0.4f;
            cg.alpha = 1f - ratio;
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.7f, ratio);
            yield return null;
        }

        Destroy(gameObject);
    }
}
