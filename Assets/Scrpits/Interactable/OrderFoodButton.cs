using UnityEngine;
using UnityEngine.UI;

public class OrderFoodButton : MonoBehaviour
{
    public void Order()
    {
        // ✅ TYLKO WYWOŁANIE BEZ PARAMETRÓW
        FoodDeliveryManager.Instance.OrderFood();

        // Opcjonalny cooldown przycisku
        GetComponent<Button>().interactable = false;
        Invoke(nameof(EnableButton), 1f);
    }

    private void EnableButton()
    {
        GetComponent<Button>().interactable = true;
    }
}