using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI; // ✅ Dodane dla Button

public class StoryCrawl : MonoBehaviour
{
    [Header("Tekst")]
    public TextMeshProUGUI storyText;
    public float scrollSpeed = 60f;

    [Header("Przejście")]
    public string nextScene = "Gameplay";
    public float delayAfterFinish = 0.5f;

    [Header("Fade Effect")]
    public CanvasGroup fadeGroup;
    public float fadeInDuration = 0.5f;

    // ✅ NOWE: Panel sterowania
    [Header("🎮 Controls Panel")]
    [Tooltip("Panel z grafiką sterowania (pokazuje się NAJPIERW)")]
    public GameObject controlsPanel;

    [Tooltip("Przycisk 'Start' / 'Continue' na panelu")]
    public Button startButton;

    [Tooltip("Opcjonalnie: dźwięk przy kliknięciu przycisku")]
    public FMODUnity.EventReference buttonClickSFX;

    // Private state
    private RectTransform textRect;
    private float startY;
    private float targetY;
    private bool hasStartedCrawl = false; // ✅ Flaga: czy story crawl już się zaczął?

    void Start()
    {
        // ✅ Inicjalizacja tekstu (ale NIE zaczynamy jeszcze scrollować)
        textRect = storyText.rectTransform;
        startY = textRect.anchoredPosition.y;
        targetY = startY + 1500f;

        // ✅ Setup controls panel
        SetupControlsPanel();

        // ✅ Jeśli nie ma panelu → od razu zaczynamy story crawl
        if (controlsPanel == null || !controlsPanel.activeInHierarchy)
        {
            StartStoryCrawl();
        }
        // ✅ Jeśli panel jest → czekamy na kliknięcie przycisku
        else
        {
            // 🎬 Start z czarnym ekranem (fadeGroup), ale NIE fade-in jeszcze
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 1f;
                fadeGroup.interactable = false;
                fadeGroup.blocksRaycasts = false;
            }
            // Story crawl czeka na StartButton
        }
    }

    // ✅ Setup panelu sterowania
    private void SetupControlsPanel()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true); // ✅ Upewnij się, że panel jest widoczny na start
        }

        if (startButton != null)
        {
            // ✅ Usuń stare listenery (zapobiega duplikatom przy reloadzie)
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogWarning("[StoryCrawl] startButton nie przypisany! Story crawl zacznie się automatycznie po 3s.");
            // Fallback: auto-start po 3 sekundach jeśli nie ma przycisku
            StartCoroutine(AutoStartFallback());
        }
    }

    // ✅ Fallback: auto-start jeśli nie ma przycisku
    private IEnumerator AutoStartFallback()
    {
        yield return new WaitForSeconds(3f);
        if (!hasStartedCrawl)
            StartStoryCrawl();
    }

    // ✅ Callback: przycisk kliknięty
    private void OnStartButtonClicked()
    {
        // 🔊 Odtwórz dźwięk kliknięcia (opcjonalnie)
        if (!buttonClickSFX.IsNull && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSFX, null);
        }

        // ✅ Ukryj panel sterowania
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }

        // ✅ Dezaktywuj przycisk (zapobiega wielokrotnemu kliknięciu)
        if (startButton != null)
        {
            startButton.interactable = false;
        }

        // ✅ Rozpocznij story crawl
        StartStoryCrawl();
    }

    // ✅ GŁÓWNA METODA: Startuje całą sekwencję story crawl
    private void StartStoryCrawl()
    {
        if (hasStartedCrawl) return; // ✅ Zabezpieczenie przed podwójnym startem
        hasStartedCrawl = true;

        Debug.Log("[StoryCrawl] 🎬 Starting story crawl sequence...");

        // 🎬 Fade-in czarnego overlaya
        if (fadeGroup != null)
        {
            StartCoroutine(FadeIn());
        }

        // 📜 Zacznij przewijanie tekstu
        StartCoroutine(ScrollUp());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            yield return null;
        }
        fadeGroup.alpha = 0f;
        fadeGroup.interactable = false;
    }

    private IEnumerator ScrollUp()
    {
        while (textRect.anchoredPosition.y < targetY)
        {
            textRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(delayAfterFinish);
        SceneManager.LoadScene(nextScene);
    }

    void Update()
    {
        // ✅ Skip działa TYLKO po rozpoczęciu story crawl
        if (hasStartedCrawl && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            StopAllCoroutines();
            SceneManager.LoadScene(nextScene);
        }
    }
}