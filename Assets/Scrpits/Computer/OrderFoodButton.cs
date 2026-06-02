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
        if (!_button.interactable && GameState.FridgeDemonDialogCompleted)
        {
            UpdateButtonState();
        }
    }

    private void UpdateButtonState()
    {
        bool canOrder = GameState.FridgeDemonDialogCompleted;
        _button.interactable = canOrder;

        ColorBlock colors = _button.colors;
        colors.normalColor = canOrder ? Color.white : Color.gray;
        _button.colors = colors;
    }

    public void Order()
    {
        if (!GameState.FridgeDemonDialogCompleted)
        {
            return;
        }

        FoodDeliveryManager.Instance.OrderFood();

        _button.interactable = false;
        Invoke(nameof(EnableButton), 1f);
    }

    private void EnableButton()
    {
        _button.interactable = GameState.FridgeDemonDialogCompleted;
    }
}