using UnityEngine;
using System.Collections;

public class OrderFoodButton : MonoBehaviour
{
    [Header("Food Settings")]
    public GameObject foodPrefab;
    public Transform spawnPoint;
    public float deliveryTime = 5f;

    [Header("References in Scene")]
    public Transform playerHand;       // Transform gracza, gdzie item bêdzie trzymany
    public GameObject pickupTextObject; // TMP_Text GameObject u¿ywany do pickupu

    public void Order()
    {
        QuestManager.Instance.CompleteQuest("Order Food");
        Debug.Log("Jedzenie zamówione!");

        StartCoroutine(DeliverFood());
    }

    private IEnumerator DeliverFood()
    {
        yield return new WaitForSeconds(deliveryTime);

        // Tworzymy prefab
        GameObject food = Instantiate(foodPrefab, spawnPoint.position, spawnPoint.rotation);

        // Przypisanie referencji w ItemPickup
        ItemPickup pickup = food.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.handPosition = playerHand;
            pickup.pickupText = pickupTextObject.GetComponent<TMPro.TMP_Text>();
            pickup.itemName = "Food";
        }

        Debug.Log("Jedzenie dostarczone!");
    }
}
