using UnityEngine;
using UnityEngine.UI;

public class OrderFoodButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        UpdateButtonState();
    }

    private void Update()
    {
        // ✅ Sprawdź co klatkę, czy można już odblokować przycisk
        if (!_button.interactable && GameState.FridgeDemonDialogCompleted)
        {
            UpdateButtonState();
        }
    }

    private void UpdateButtonState()
    {
        bool canOrder = GameState.FridgeDemonDialogCompleted;
        _button.interactable = canOrder;

        // ✅ Opcjonalnie: zmień kolor dla lepszego feedbacku
        ColorBlock colors = _button.colors;
        colors.normalColor = canOrder ? Color.white : Color.gray;
        _button.colors = colors;
    }

    public void Order()
    {
        if (!GameState.FridgeDemonDialogCompleted)
        {
            Debug.Log("[OrderFoodButton] ❌ Blocked – fridge demon dialog not completed");
            return;
        }

        FoodDeliveryManager.Instance.OrderFood();

        // Opcjonalny cooldown przycisku
        _button.interactable = false;
        Invoke(nameof(EnableButton), 1f);
    }

    private void EnableButton()
    {
        // ✅ Przywróć stan na podstawie flagi, nie na sztywno
        _button.interactable = GameState.FridgeDemonDialogCompleted;
    }
}