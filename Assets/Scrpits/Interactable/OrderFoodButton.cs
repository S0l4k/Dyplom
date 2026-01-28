using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OrderFoodButton : MonoBehaviour
{
    [Header("Food Settings")]
    public GameObject foodPrefab;
    public Transform spawnPoint;
    public float deliveryTime = 5f;

    [Header("References in Scene")]
    public Transform playerHand;
    public GameObject pickupTextObject;
    public Button orderButton; // Przycisk w UI

    private bool isOrdering = false;

    public void Order()
    {
        if (isOrdering)
        {
            Debug.Log("[OrderFood] Zamówienie ju¿ w trakcie!");
            return;
        }

        isOrdering = true;

        QuestManager.Instance.CompleteQuest("Order Food");
        Debug.Log("Jedzenie zamówione!");

        if (orderButton != null)
            orderButton.interactable = false;

        StartCoroutine(DeliverFood());
    }

    private IEnumerator DeliverFood()
    {
        yield return new WaitForSeconds(deliveryTime);

        GameObject food = Instantiate(foodPrefab, spawnPoint.position, spawnPoint.rotation);

        ItemPickup pickup = food.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.handPosition = playerHand;
            pickup.pickupText = pickupTextObject.GetComponent<TMPro.TMP_Text>();
            pickup.itemName = "Food";

            pickup.stairLoop = FindObjectOfType<StairLoop>();
        }

        Debug.Log("Jedzenie dostarczone!");

        // Odblokowanie przycisku
        isOrdering = false;
        if (orderButton != null)
            orderButton.interactable = true;
    }
}
