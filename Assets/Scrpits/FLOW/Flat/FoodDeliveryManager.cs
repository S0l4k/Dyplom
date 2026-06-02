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
        if (!GameState.FridgeDemonDialogCompleted)
        {
            return; 
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest("Order Food");

            if (GameNarrativeManager.Instance != null)
            {
                GameNarrativeManager.Instance.UnlockApartmentExploration();
            }
        }
    }
}