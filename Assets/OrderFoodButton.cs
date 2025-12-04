using UnityEngine;

public class OrderFoodButton : MonoBehaviour
{
    public void Order()
    {
        QuestManager.Instance.CompleteQuest("Order Food");
        Debug.Log("Jedzenie zamówione!");
    }
}
