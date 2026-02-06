using UnityEngine;
using TMPro;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class ItemCheck : MonoBehaviour
{
    [Header("Check Settings")]
    public string itemName = "Object";
    public string playerThought = "You examine the object closely..."; // Myśl gracza

    [Header("Demon Response")]
    public string demonResponse = ""; // Tekst demona (pozostaw pusty aby nie wyświetlać)
    public EventReference demonVoiceEvent; // Dźwięk głosu demona

    [Header("UI Elements")]
    public TMP_Text interactionText;      // Tekst "Press E to check..."
    public GameObject thoughtBubble;      // Rodzic dla obu tekstów

    [Header("Text Components")]
    public TMP_Text playerThoughtText;    // Szara, subtelna czcionka
    public TMP_Text demonResponseText;    // Czerwona, demonicznna czcionka

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.07f;
    [SerializeField] private float playerThoughtStayTime = 1.2f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float delayBeforeDemon = 0.2f;

    [Header("Visual Styling")]
    [SerializeField] private string playerMarkerColor = "#00000080";
    [SerializeField] private string demonMarkerColor = "#FF000080";

    private Camera playerCamera;
    private bool canInteract = false;
    private bool isChecking = false;
    private EventInstance demonVoiceInstance;
    private Color playerOriginalColor = Color.white; // Zapamiętaj oryginalny kolor

    void Start()
    {
        playerCamera = Camera.main;

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        if (thoughtBubble != null)
            thoughtBubble.SetActive(false);

        // Zapamiętaj oryginalny kolor tekstu gracza
        if (playerThoughtText != null)
            playerOriginalColor = playerThoughtText.color;

        // Fallback – jeśli nie przypisano oddzielnych tekstów
        if (playerThoughtText == null && thoughtBubble != null)
            playerThoughtText = thoughtBubble.GetComponentInChildren<TMP_Text>();

        if (demonResponseText == null)
            demonResponseText = playerThoughtText;
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
        if (!playerCamera || interactionText == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                ShowInteractionText();
                canInteract = true;
                return;
            }
        }

        HideInteractionText();
        canInteract = false;
    }

    void ShowInteractionText()
    {
        interactionText.gameObject.SetActive(true);
        interactionText.text = $"Press E to check {itemName}";
    }

    void HideInteractionText()
    {
        interactionText.gameObject.SetActive(false);
    }

    IEnumerator CheckItem()
    {
        isChecking = true;
        HideInteractionText();

        // ✅ KLUCZOWA NAPRAWA: Zresetuj cały stan UI przed użyciem
        ResetUITexts();

        if (thoughtBubble != null)
            thoughtBubble.SetActive(true);
        else
        {
            Debug.LogError("[ItemCheck] thoughtBubble is not assigned!");
            isChecking = false;
            yield break;
        }

        // === ETAP 1: Myśl gracza ===
        if (playerThoughtText != null)
        {
            playerThoughtText.gameObject.SetActive(true);
            yield return StartCoroutine(TypeTextWithMarker(
                playerThoughtText,
                playerThought,
                playerMarkerColor
            ));

            yield return new WaitForSeconds(playerThoughtStayTime);

            yield return StartCoroutine(FadeOutText(playerThoughtText, fadeDuration));
        }

        yield return new WaitForSeconds(delayBeforeDemon);

        // === ETAP 2: Odpowiedź demona ===
        if (!string.IsNullOrWhiteSpace(demonResponse) && demonResponseText != null)
        {
            // ✅ Zresetuj tekst demona przed użyciem
            demonResponseText.gameObject.SetActive(true);
            demonResponseText.color = Color.white; // Reset koloru

            demonVoiceInstance = RuntimeManager.CreateInstance(demonVoiceEvent);
            demonVoiceInstance.start();

            yield return StartCoroutine(TypeTextWithMarker(
                demonResponseText,
                demonResponse,
                demonMarkerColor
            ));

            demonVoiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            demonVoiceInstance.release();
        }

        yield return new WaitForSeconds(playerThoughtStayTime);

        // ✅ Wyczyść UI po zakończeniu
        CleanupUITexts();

        thoughtBubble.SetActive(false);
        isChecking = false;
    }

    // ✅ NOWA METODA: Resetuje stan UI przed każdą interakcją
    void ResetUITexts()
    {
        if (playerThoughtText != null)
        {
            playerThoughtText.gameObject.SetActive(true);
            playerThoughtText.color = playerOriginalColor; // Przywróć oryginalny kolor
            playerThoughtText.text = "";
        }

        if (demonResponseText != null && demonResponseText != playerThoughtText)
        {
            demonResponseText.gameObject.SetActive(false); // Demon zaczyna ukryty
            demonResponseText.color = Color.white;
            demonResponseText.text = "";
        }
    }

    // ✅ NOWA METODA: Czyści UI po zakończeniu
    void CleanupUITexts()
    {
        if (playerThoughtText != null)
        {
            playerThoughtText.text = "";
            playerThoughtText.gameObject.SetActive(false);
        }

        if (demonResponseText != null && demonResponseText != playerThoughtText)
        {
            demonResponseText.text = "";
            demonResponseText.gameObject.SetActive(false);
        }
    }

    IEnumerator TypeTextWithMarker(TMP_Text textObj, string text, string markerColor)
    {
        if (textObj == null) yield break;

        string openTag = $"<mark={markerColor}>";
        string closeTag = "</mark>";
        textObj.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            string visible = text.Substring(0, i + 1);
            textObj.text = openTag + visible + closeTag;
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
            float t = elapsed / duration;
            textObj.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }

        textObj.color = targetColor;
        textObj.gameObject.SetActive(false); // Dezaktywuj PO zaniknięciu
    }

    void OnDestroy()
    {
        if (demonVoiceInstance.isValid())
        {
            demonVoiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            demonVoiceInstance.release();
        }
    }
}