using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class StoryCrawl : MonoBehaviour
{
    [Header("Tekst")]
    public TextMeshProUGUI storyText;
    public float scrollSpeed = 60f;

    [Header("Przejście")]
    public string nextScene = "Gameplay";
    public float delayAfterFinish = 0.5f;

    [Header("Fade Effect")]
    public CanvasGroup fadeGroup; // ← Przeciągnij tutaj CanvasGroup z overlaya
    public float fadeInDuration = 0.5f;

    private RectTransform textRect;
    private float startY;
    private float targetY;

    void Start()
    {
        // 🎬 Start z czarnym ekranem, potem fade-in
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f; // zaczynamy od czerni
            fadeGroup.interactable = false;
            fadeGroup.blocksRaycasts = false;
            StartCoroutine(FadeIn());
        }

        textRect = storyText.rectTransform;
        startY = textRect.anchoredPosition.y;
        targetY = startY + 1500f;

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
        fadeGroup.interactable = false; // overlay nie blokuje inputu po fade
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
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            SceneManager.LoadScene(nextScene);
        }
    }
}