// 파일 이름: IInteractable.cs
public interface IInteractable
{
    // "이 인터페이스를 쓰는 클래스는 무조건 이 함수를 구현해라"라는 약속
    void OnInteract(Inventory playerInventory);
}