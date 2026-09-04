using UnityEngine;

public enum SpecialCardType
{
    // 즉시 발동
    RocketBasket,    // 자신 이동속도 2배 5초
    SlowBasket,      // 상대 이동속도 0.4배 3초
    Blackout,        // 상대 암전 3초
    ControlReverse,  // 상대 조작 반전 3초
    Stun,            // 상대 1초 정지
    DropIngredient,  // 상대 재료 1개 제거
    Mold,            // 상대 재료 1개 → 썩은 재료
    // 보관 가능 (요리 시 발동)
    FreshShield,     // 다음 곰팡이 효과 1회 방어
    WildIngredient,  // 요리 시 부족한 재료 1개 와일드카드
    MSG,             // 요리 점수 +5점
}

[CreateAssetMenu(fileName = "NewSpecialCard", menuName = "CookingGame/SpecialCard")]
public class SpecialCardSO : ScriptableObject
{
    public string cardName;
    public SpecialCardType cardType;
    public Sprite icon;
    [Tooltip("false이면 인벤토리에 보관 후 요리 시 발동")]
    public bool isInstant = true;
}
