using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI; 

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

    [Header("🎮 Controls Panel")]
    [Tooltip("Panel z grafiką sterowania (pokazuje się NAJPIERW)")]
    public GameObject controlsPanel;

    [Tooltip("Przycisk 'Start' / 'Continue' na panelu")]
    public Button startButton;

    [Tooltip("Opcjonalnie: dźwięk przy kliknięciu przycisku")]
    public FMODUnity.EventReference buttonClickSFX;

    private RectTransform textRect;
    private float startY;
    private float targetY;
    private bool hasStartedCrawl = false; 

    void Start()
    {
        textRect = storyText.rectTransform;
        startY = textRect.anchoredPosition.y;
        targetY = startY + 1500f;
        SetupControlsPanel();
        if (controlsPanel == null || !controlsPanel.activeInHierarchy)
        {
            StartStoryCrawl();
        }
        else
        {
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 1f;
                fadeGroup.interactable = false;
                fadeGroup.blocksRaycasts = false;
            }
        }
    }
    private void SetupControlsPanel()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true); 
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            StartCoroutine(AutoStartFallback());
        }
    }

    private IEnumerator AutoStartFallback()
    {
        yield return new WaitForSeconds(3f);
        if (!hasStartedCrawl)
            StartStoryCrawl();
    }

    private void OnStartButtonClicked()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (!buttonClickSFX.IsNull && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSFX, null);
        }

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }

        if (startButton != null)
        {
            startButton.interactable = false;
        }
        StartStoryCrawl();
    }

    private void StartStoryCrawl()
    {
        if (hasStartedCrawl) return; 
        hasStartedCrawl = true;

        if (fadeGroup != null)
        {
            StartCoroutine(FadeIn());
        }

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
        if (hasStartedCrawl && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            StopAllCoroutines();
            SceneManager.LoadScene(nextScene);
        }
    }
}