using UnityEngine;
using TMPro;

public class CardDisplaySettings : MonoBehaviour
{
    public static CardDisplaySettings Instance { get; private set; }

    [Header("팝업 폰트")]
    public TMP_FontAsset bazziFont;

    [Header("특수카드 알림 프리팹 (상대 화면에 크게 표시)")]
    public GameObject specialCardNotifyPrefab;

    [Header("식재료 알림 프리팹 (자기 화면에 1초 표시)")]
    public GameObject ingreCardNotifyPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
