using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class ComputerCursor : MonoBehaviour
{
    [Header("Cursor Sprites")]
    public Sprite defaultCursor;
    public Sprite hoverCursor;
    public Sprite clickCursor;

    [Header("Settings")]
    [Tooltip("Offset from cursor pivot point (usually top-left)")]
    public Vector2 cursorOffset = new Vector2(8, -8);

    private Image cursorImage;
    private RectTransform cursorRectTransform;
    private bool isActive = false;
    private EventSystem eventSystem;
    private Canvas ownCanvas;

    private void Awake()
    {
        eventSystem = EventSystem.current;
        ownCanvas = GetComponent<Canvas>();

        GameObject cursorObj = new GameObject("ComputerCursor");
        cursorObj.transform.SetParent(transform, false);
        cursorObj.transform.SetAsLastSibling(); 

        cursorImage = cursorObj.AddComponent<Image>();
        cursorImage.raycastTarget = false; 
        cursorImage.sprite = defaultCursor;
        cursorImage.color = Color.white;

        cursorRectTransform = cursorImage.rectTransform;
        cursorRectTransform.sizeDelta = new Vector2(32, 32);
        cursorRectTransform.pivot = new Vector2(0f, 1f); 

        cursorObj.SetActive(false);
    }

    public void Enable()
    {
        isActive = true;
        cursorImage.gameObject.SetActive(true);
        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.None;
    }

    public void Disable()
    {
        isActive = false;
        cursorImage.gameObject.SetActive(false);
        Cursor.visible = true; 
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!isActive || eventSystem == null) return;

        Vector2 mousePos = (Vector2)Input.mousePosition + cursorOffset;
        cursorRectTransform.position = mousePos;

        if (eventSystem.currentSelectedGameObject != null)
        {
            Canvas targetCanvas = eventSystem.currentSelectedGameObject.GetComponentInParent<Canvas>();
            if (targetCanvas == ownCanvas)
            {
                var selectable = eventSystem.currentSelectedGameObject.GetComponent<UnityEngine.UI.Selectable>();
                if (selectable != null && selectable.IsActive() && selectable.IsInteractable())
                {
                    cursorImage.sprite = Input.GetMouseButton(0) ? clickCursor : hoverCursor;
                    return;
                }
            }
        }

        cursorImage.sprite = defaultCursor;
    }
}