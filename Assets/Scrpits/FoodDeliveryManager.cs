using UnityEngine;

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
        DontDestroyOnLoad(gameObject);
    }

    public void OrderFood()
    {
        // ✅ ODBLOKUJ DIALOG Z KURIEREM
        GameState.CourierArrived = true;
        Debug.Log("[FoodDelivery] 🚪 Kurier dostępny do rozmowy przy drzwiach");

        // ✅ UKOŃCZ QUEST "Order Food"
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest("Order Food");

            // ✅ NOWE: Rozpocznij quest "rozglądnij się po mieszkaniu"
            if (GameNarrativeManager.Instance != null)
            {
                GameNarrativeManager.Instance.UnlockApartmentExploration();
            }
        }

        Debug.Log("[FoodDelivery] 🎯 Apartment exploration quest unlocked!");
    }
}