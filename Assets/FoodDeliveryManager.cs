using UnityEngine;
using System.Collections;
using TMPro;

public class FoodDeliveryManager : MonoBehaviour
{
    public static FoodDeliveryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // KLUCZOWE: obiekt przetrwa wy³¹czenie UI i zmianê scen
    }

    public void OrderFood(GameObject foodPrefab, Transform spawnPoint, Transform playerHand,
                         GameObject pickupTextObject, float deliveryTime, string itemName = "Food")
    {
        StartCoroutine(DeliverFoodRoutine(foodPrefab, spawnPoint, playerHand, pickupTextObject, deliveryTime, itemName));
        Debug.Log($"[FoodDelivery] Zamówiono {itemName}, dostawa za {deliveryTime}s");

        // Quest – jeœli istnieje
        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest("Order Food");
    }

    private IEnumerator DeliverFoodRoutine(GameObject foodPrefab, Transform spawnPoint, Transform playerHand,
                                          GameObject pickupTextObject, float deliveryTime, string itemName)
    {
        yield return new WaitForSeconds(deliveryTime);

        GameObject food = Instantiate(foodPrefab, spawnPoint.position, spawnPoint.rotation);
        ItemPickup pickup = food.GetComponent<ItemPickup>();

        if (pickup != null)
        {
            pickup.handPosition = playerHand;
            pickup.pickupText = pickupTextObject.GetComponent<TMP_Text>();
            pickup.itemName = itemName;
            pickup.stairLoop = FindObjectOfType<StairLoop>();
        }

        Debug.Log($"[FoodDelivery] Dostarczono {itemName}!");
    }
}