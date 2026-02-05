using UnityEngine;
using UnityEngine.UI;

public class OrderFoodButton : MonoBehaviour
{
    [Header("Food Settings")]
    public GameObject foodPrefab;
    public Transform spawnPoint;
    public float deliveryTime = 5f;

    [Header("References")]
    public Transform playerHand;
    public GameObject pickupTextObject;

    public void Order()
    {
        // Delegujemy WSZYSTKO do mened¿era – UI tylko wywo³uje
        FoodDeliveryManager.Instance.OrderFood(
            foodPrefab,
            spawnPoint,
            playerHand,
            pickupTextObject,
            deliveryTime,
            "Food"
        );

        // Opcjonalnie: krótki cooldown na przycisku (nie blokuj na 5s!)
        GetComponent<Button>().interactable = false;
        Invoke(nameof(EnableButton), 1f);
    }

    private void EnableButton()
    {
        GetComponent<Button>().interactable = true;
    }
}