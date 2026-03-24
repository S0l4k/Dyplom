using FMODUnity;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCheck : MonoBehaviour
{
    [Header("Check Settings")]
    public string itemName = "Object";
    public string playerThought = "You examine the object closely...";

    [Header("Demon Response (opcjonalnie)")]
    public string demonResponse = "";
    public EventReference demonVoiceEvent;

    [Header("UI")]
    public TMP_Text interactionText;
    public TMP_Text playerThoughtText;

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.07f;
    [SerializeField] private float playerThoughtStayTime = 1.2f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float delayBeforeDemon = 0.2f;

    [Header("Visual Styling")]
    [SerializeField] private string playerMarkerColor = "#00000080";
    [SerializeField] private string demonMarkerColor = "#FF000080";

    [Header("Outline")]
    public Outline outline;

    private Camera playerCamera;
    private bool canInteract = false;
    private bool isChecking = false;
    private FMOD.Studio.EventInstance demonVoiceInstance;
    private Color playerOriginalColor = Color.white;

    void Start()
    {
        playerCamera = Camera.main;

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        if (playerThoughtText != null)
        {
            playerThoughtText.gameObject.SetActive(false);
            playerOriginalColor = playerThoughtText.color;
        }
        else
        {
            Debug.LogError($"[ItemCheck] playerThoughtText NIE JEST PRZYPISANY na obiekcie {name}!", this);
        }
        if (outline != null) outline.enabled = false;
    }

    void Update()
    {
        if (isChecking) return;

        CheckForInteraction();

        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(CheckItem());
        }
    }

    void CheckForInteraction()
    {
        if (!playerCamera || interactionText == null) return;

        bool wasInteracting = canInteract;
        canInteract = false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f) && hit.collider.gameObject == gameObject)
        {
            canInteract = true;
            if (outline != null) outline.enabled = true;
        }
        else
        {
            if (outline != null) outline.enabled = false;
        }

        if (canInteract != wasInteracting)
        {
            if (canInteract)
                ShowInteractionText();
            else
                HideInteractionText();
        }
    }

    void ShowInteractionText()
    {
        interactionText.gameObject.SetActive(true);
        string newText = $"Press E to check {itemName}";
        if (interactionText.text != newText)
            interactionText.text = newText;
    }

    void HideInteractionText()
    {
        interactionText.gameObject.SetActive(false);
    }

    IEnumerator CheckItem()
    {
        isChecking = true;
        HideInteractionText();

        if (outline != null) outline.enabled = false;

        Debug.Log($"[ItemCheck] Rozpoczęto interakcję z: {itemName}");

        if (playerThoughtText != null)
        {
            playerThoughtText.gameObject.SetActive(true);
            playerThoughtText.color = playerOriginalColor;
            playerThoughtText.text = "";
        }
        else
        {
            Debug.LogError("[ItemCheck] playerThoughtText jest NULL podczas CheckItem!");
            isChecking = false;
            yield break;
        }

        // === ETAP 1: Myśl gracza ===
        yield return StartCoroutine(TypeTextWithMarker(
            playerThoughtText,
            playerThought,
            playerMarkerColor
        ));

        yield return new WaitForSeconds(playerThoughtStayTime);
        yield return StartCoroutine(FadeOutText(playerThoughtText, fadeDuration));
        yield return new WaitForSeconds(delayBeforeDemon);

        // === ETAP 2: Odpowiedź demona (opcjonalnie) ===
        if (!string.IsNullOrWhiteSpace(demonResponse))
        {
            playerThoughtText.gameObject.SetActive(true);
            playerThoughtText.color = Color.white;
            playerThoughtText.text = "";

            // ✅ ZAMIENIONE: RuntimeManager -> AudioManager
            if (!demonVoiceEvent.IsNull)
            {
                demonVoiceInstance = AudioManager.Instance.PlayDialogVoice(demonVoiceEvent);
            }

            yield return StartCoroutine(TypeTextWithMarker(
                playerThoughtText,
                demonResponse,
                demonMarkerColor
            ));

            // ✅ ZAMIENIONE: bezpośrednie stop/release -> AudioManager
            if (demonVoiceInstance.isValid())
            {
                AudioManager.Instance.StopDialogVoice(ref demonVoiceInstance, true);
            }

            yield return new WaitForSeconds(playerThoughtStayTime);
            yield return StartCoroutine(FadeOutText(playerThoughtText, fadeDuration));
        }

        // ✅ QUEST: ukończ po sprawdzeniu talerza
        if (itemName == "Plate" && QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest("Check your fridge");
            Debug.Log("[Narrative] ✅ Quest completed: Check your fridge");
            GameNarrativeManager.Instance?.TriggerFridgeDemon();
        }
        isChecking = false;
        Debug.Log($"[ItemCheck] Interakcja z {itemName} zakończona");
    }

    IEnumerator TypeTextWithMarker(TMP_Text textObj, string text, string markerColor)
    {
        if (textObj == null) yield break;

        string openTag = $"<mark={markerColor}>";
        string closeTag = "</mark>";
        textObj.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            textObj.text = openTag + text.Substring(0, i + 1) + closeTag;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    IEnumerator FadeOutText(TMP_Text textObj, float duration)
    {
        if (textObj == null) yield break;

        Color originalColor = textObj.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textObj.color = Color.Lerp(originalColor, targetColor, elapsed / duration);
            yield return null;
        }

        textObj.color = targetColor;
        textObj.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // ✅ ZAMIENIONE: bezpośrednie stop/release -> AudioManager
        AudioManager.Instance.StopDialogVoice(ref demonVoiceInstance, true);
    }
}