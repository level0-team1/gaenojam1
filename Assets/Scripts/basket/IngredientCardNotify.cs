using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientCardNotify : MonoBehaviour
{
    public static void Show(UnityEngine.Canvas canvas, IngredientSO ingredient)
    {
        if (canvas == null || ingredient == null) return;
        if (CardDisplaySettings.Instance?.ingreCardNotifyPrefab == null) return;

        canvas.sortingOrder = 10;

        var go = Instantiate(CardDisplaySettings.Instance.ingreCardNotifyPrefab, canvas.transform);

        var iconTr = go.transform.Find("ItemCan");
        if (iconTr != null && ingredient.icon != null)
            iconTr.GetComponent<Image>().sprite = ingredient.icon;

        var nameTr = go.transform.Find("ItemNext");
        if (nameTr != null)
            nameTr.GetComponent<TMP_Text>().text = ingredient.itemName;

        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.72f, 0.68f);
            rt.anchorMax = new Vector2(0.72f, 0.68f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.zero;
        }

        go.AddComponent<IngredientCardNotify>();
    }

    IEnumerator Start()
    {
        var cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

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

        yield return new WaitForSeconds(1f);

        for (float t = 0; t < 0.3f; t += Time.deltaTime)
        {
            float ratio = t / 0.3f;
            cg.alpha = 1f - ratio;
            transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.7f, ratio);
            yield return null;
        }

        Destroy(gameObject);
    }
}
