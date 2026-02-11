using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class StoryCrawl : MonoBehaviour
{
    [Header("Tekst")]
    public TextMeshProUGUI storyText;
    public float scrollSpeed = 60f;  // px/s

    [Header("Przejście")]
    public string nextScene = "Gameplay";
    public float delayAfterFinish = 0.5f;

    private RectTransform textRect;
    private float startY;
    private float targetY;

    void Start()
    {
        textRect = storyText.rectTransform;
        startY = textRect.anchoredPosition.y;               // 🔑 Zapamiętaj POZYCJĘ STARTOWĄ (ręcznie ustawioną w edytorze)
        targetY = startY + 1500f;                           // 🔑 Przewiń o stałą wartość (1500px = pewnie poza ekranem)

        StartCoroutine(ScrollUp());
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
        // Skip: dowolny klawisz / kliknięcie
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            SceneManager.LoadScene(nextScene);
        }
    }
}