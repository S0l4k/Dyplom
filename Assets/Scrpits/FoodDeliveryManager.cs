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
        // ✅ ODBLOKUJ DIALOG Z KURIEREM (od razu po naciśnięciu przycisku)
        GameState.CourierArrived = true;
        Debug.Log("[FoodDelivery] 🚪 Kurier dostępny do rozmowy przy drzwiach");

        // ✅ UKOŃCZ QUEST
        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest("Order Food");

        // ❌ BRAK Instantiate – NIC się nie respawnowało
    }
}