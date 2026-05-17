using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("암전 오버레이 (해당 플레이어 카메라 위 검은 패널)")]
    public CanvasGroup blackoutOverlay;

    [Header("이 플레이어의 카메라 캔버스 (특수카드 알림용)")]
    public UnityEngine.Canvas playerCanvas;

    public float SpeedMultiplier { get; private set; } = 1f;
    public bool  IsStunned       { get; private set; } = false;
    public bool  IsReversed      { get; private set; } = false;

    public void ApplySpeedBoost(float multiplier, float duration)
        => StartCoroutine(SpeedEffect(multiplier, duration));

    public void ApplyStun(float duration)
        => StartCoroutine(StunEffect(duration));

    public void ApplyControlReverse(float duration)
        => StartCoroutine(ReverseEffect(duration));

    public void ApplyBlackout(float duration)
        => StartCoroutine(BlackoutEffect(duration));

    private IEnumerator SpeedEffect(float multiplier, float duration)
    {
        SpeedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        SpeedMultiplier = 1f;
    }

    private IEnumerator StunEffect(float duration)
    {
        IsStunned = true;
        yield return new WaitForSeconds(duration);
        IsStunned = false;
    }

    private IEnumerator ReverseEffect(float duration)
    {
        IsReversed = true;
        yield return new WaitForSeconds(duration);
        IsReversed = false;
    }

    private IEnumerator BlackoutEffect(float duration)
    {
        if (blackoutOverlay != null) blackoutOverlay.alpha = 1f;
        yield return new WaitForSeconds(duration);
        if (blackoutOverlay != null) blackoutOverlay.alpha = 0f;
    }
}
