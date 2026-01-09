using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class IntroThoughtController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI thoughtText;

    [Header("Typewriter Settings")]
    [TextArea]
    public string thoughtLine = "It must be a dream...";
    public float typeSpeed = 0.07f;
    public float stayTimeAfterTyping = 1.0f;

    [Header("Mark Settings")]
    [Tooltip("Kolor poświatki (RGBA w hexie)")]
    public string markColor = "#00000080";

    private Coroutine typingCoroutine;

    void Awake()
    {
        if (thoughtText != null)
            thoughtText.text = "";
    }

    // 👉 WYWOŁUJ TO Z TIMELINE (Signal)
    public void ShowThought()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeThought());
    }

    IEnumerator TypeThought()
    {
        thoughtText.text = "";

        string openTag = $"<mark={markColor}>";
        string closeTag = "</mark>";

        for (int i = 0; i < thoughtLine.Length; i++)
        {
            string visible = thoughtLine.Substring(0, i + 1);
            thoughtText.text = openTag + visible + closeTag;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Tekst zostaje chwilę na ekranie
        yield return new WaitForSeconds(stayTimeAfterTyping);

        thoughtText.text = "";
        typingCoroutine = null;
    }

    // Opcjonalnie: gdybyś chciał ręcznie czyścić
    public void ClearThought()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        thoughtText.text = "";
    }
    public PlayerController playerController;
    public PlayerCam playerCam;
    public MoveCamera moveCamera;
    public GameObject cutsceneCanvas;
    public GameObject canvas;
    public PlayableDirector director;

    public void EndIntro()
    {
        if (director != null)
            director.Stop();

        if (playerController != null)
            playerController.enabled = true;

        if (playerCam != null)
        {
            playerCam.enabled = true;
            playerCam.SyncRotationWithCamera();
        }
        if(moveCamera !=null)
            moveCamera.enabled = true;
        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(false);
        if(canvas != null) canvas.SetActive(true);
    }

}
