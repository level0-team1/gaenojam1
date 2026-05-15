using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public KeyCode interactKey; // P1: F, P2: L 설정
    private Inventory myInventory;
    private Basket targetBasket; // 현재 범위 내 바구니

    void Start() => myInventory = GetComponent<Inventory>();

    void Update()
    {
        if (targetBasket != null && Input.GetKeyDown(interactKey))
        {
            targetBasket.Interact(myInventory);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Basket")) targetBasket = other.GetComponent<Basket>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Basket")) targetBasket = null;
    }
}