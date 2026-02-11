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
    public Vector2 cursorOffset = new Vector2(8, -8); // dostosuj do swojego sprite'a

    private Image cursorImage;
    private RectTransform cursorRectTransform;
    private bool isActive = false;
    private EventSystem eventSystem;
    private Canvas ownCanvas; // cache dla wydajności

    private void Awake()
    {
        eventSystem = EventSystem.current;
        ownCanvas = GetComponent<Canvas>();

        // Stwórz kursor jako dziecko computerCanvas
        GameObject cursorObj = new GameObject("ComputerCursor");
        cursorObj.transform.SetParent(transform, false);
        cursorObj.transform.SetAsLastSibling(); // na wierzchu wszystkiego

        cursorImage = cursorObj.AddComponent<Image>();
        cursorImage.raycastTarget = false; // kursor NIE blokuje interakcji UI
        cursorImage.sprite = defaultCursor;
        cursorImage.color = Color.white;

        cursorRectTransform = cursorImage.rectTransform;
        cursorRectTransform.sizeDelta = new Vector2(32, 32); // rozmiar sprite'a
        cursorRectTransform.pivot = new Vector2(0f, 1f); // pivot w lewym górnym rogu (jak prawdziwy kursor)

        cursorObj.SetActive(false);
    }

    public void Enable()
    {
        isActive = true;
        cursorImage.gameObject.SetActive(true);
        Cursor.visible = false; // ukryj systemowy kursor
        Cursor.lockState = CursorLockMode.None;
    }

    public void Disable()
    {
        isActive = false;
        cursorImage.gameObject.SetActive(false);
        Cursor.visible = true; // przywróć systemowy kursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!isActive || eventSystem == null) return;

        // ✅ POPRAWKA: konwersja Vector3 → Vector2 przed dodaniem offsetu
        Vector2 mousePos = (Vector2)Input.mousePosition + cursorOffset;
        cursorRectTransform.position = mousePos;

        // Wykryj hover na UI elementach W OBREBIE computerCanvas
        if (eventSystem.currentSelectedGameObject != null)
        {
            // Sprawdź czy element należy do tego canvasu (używamy cached ownCanvas)
            Canvas targetCanvas = eventSystem.currentSelectedGameObject.GetComponentInParent<Canvas>();
            if (targetCanvas == ownCanvas)
            {
                // Zmień sprite na hover jeśli to Button/Selectable
                var selectable = eventSystem.currentSelectedGameObject.GetComponent<UnityEngine.UI.Selectable>();
                if (selectable != null && selectable.IsActive() && selectable.IsInteractable())
                {
                    cursorImage.sprite = Input.GetMouseButton(0) ? clickCursor : hoverCursor;
                    return;
                }
            }
        }

        // Domyślny sprite
        cursorImage.sprite = defaultCursor;
    }
}