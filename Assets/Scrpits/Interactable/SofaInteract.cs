using UnityEngine;
using TMPro;
using System.Collections;
using FMODUnity;
using UnityEngine.SceneManagement;

public class SofaInteract : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform sofaViewPoint;
    public TMP_Text interactText;
    public ScreenFader screenFader;
    public TMP_Text thoughtText;

    [Header("Audio")]
    public EventReference demonVoiceEvent;

    [Header("Visual Styling")]
    [SerializeField] private string demonMarkerColor = "#FF000080";

    [Header("Settings")]
    public float useRange = 3f;
    public float moveSpeed = 2.5f;
    public float typeSpeed = 0.07f;

    private bool canUse = false;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    private PlayerController playerController;
    private PlayerCam playerCam;
    private FMOD.Studio.EventInstance demonVoiceInstance;

    void Start()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false);

        playerController = FindObjectOfType<PlayerController>();
        playerCam = FindObjectOfType<PlayerCam>();
        demonVoiceInstance = new FMOD.Studio.EventInstance();
    }

    void Update()
    {
        if (!GameState.InteractionsLocked)
        {
            canUse = false;
            if (interactText != null) interactText.gameObject.SetActive(false);
            return;
        }

        if (GameState.SofaSequenceActive)
        {
            canUse = false;
            if (interactText != null) interactText.gameObject.SetActive(false);
            return;
        }

        CheckForSofa();

        if (canUse && Input.GetKeyDown(KeyCode.E) && !GameState.SofaSequenceActive)
        {
            StartCoroutine(UseSofa());
        }
    }

    void CheckForSofa()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, useRange) && hit.collider.gameObject == gameObject)
        {
            interactText.gameObject.SetActive(true);
            interactText.text = "Press E to rest on couch";
            canUse = true;
            return;
        }

        interactText.gameObject.SetActive(false);
        canUse = false;
    }

    IEnumerator UseSofa()
    {
        GameState.SofaSequenceActive = true;
        canUse = false;
        if (interactText != null) interactText.gameObject.SetActive(false);

        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;

        originalCamPosition = playerCamera.position;
        originalCamRotation = playerCamera.rotation;

        float elapsed = 0f;
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * moveSpeed;
            playerCamera.position = Vector3.Lerp(startPos, sofaViewPoint.position, elapsed);
            playerCamera.rotation = Quaternion.Slerp(startRot, sofaViewPoint.rotation, elapsed);
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        string demonLine = "You won't free yourself that easy...";
        yield return StartCoroutine(TypeDemonThought(demonLine));

        yield return new WaitForSeconds(0.8f);

        // ✅ ZAMIENIONE: bezpośrednie stop/release -> AudioManager
        if (demonVoiceInstance.isValid())
        {
            AudioManager.Instance.StopDialogVoice(ref demonVoiceInstance, true);
        }

        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeOut(1.2f));
        }
        else
        {
            GameObject fadeObj = new GameObject("FadeOverlay");
            fadeObj.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = fadeObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            UnityEngine.UI.Image image = fadeObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.sizeDelta = Vector2.zero;

            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.2f)
            {
                image.color = Color.Lerp(new Color(0f, 0f, 0f, 0f), Color.black, t);
                yield return null;
            }

            image.color = Color.black;
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("[SofaInteract] 🎬 Ładowanie Main Menu...");
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator TypeDemonThought(string text)
    {
        if (thoughtText == null)
        {
            Debug.LogError("[SofaInteract] ❌ thoughtText nie przypisany!");
            yield break;
        }

        thoughtText.gameObject.SetActive(true);
        thoughtText.color = Color.white;
        thoughtText.text = "";

        // ✅ ZAMIENIONE: RuntimeManager -> AudioManager
        if (!demonVoiceEvent.IsNull)
        {
            demonVoiceInstance = AudioManager.Instance.PlayDialogVoice(demonVoiceEvent);
            Debug.Log("[SofaInteract] 🔊 Głos demona rozpoczęty RAZEM z tekstem");
        }

        string openTag = $"<mark={demonMarkerColor}>";
        string closeTag = "</mark>";

        for (int i = 0; i < text.Length; i++)
        {
            thoughtText.text = openTag + text.Substring(0, i + 1) + closeTag;
            yield return new WaitForSeconds(typeSpeed);
        }

        // ✅ ZAMIENIONE: bezpośrednie stop/release -> AudioManager
        if (demonVoiceInstance.isValid())
        {
            AudioManager.Instance.StopDialogVoice(ref demonVoiceInstance, true);
            Debug.Log("[SofaInteract] 🔇 Głos demona zatrzymany po zakończeniu tekstu");
        }

        yield return new WaitForSeconds(1.5f);

        Color startCol = thoughtText.color;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            thoughtText.color = Color.Lerp(startCol, new Color(startCol.r, startCol.g, startCol.b, 0), elapsed / 0.3f);
            yield return null;
        }

        thoughtText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // ✅ ZAMIENIONE: bezpośrednie stop/release -> AudioManager
        AudioManager.Instance.StopDialogVoice(ref demonVoiceInstance, true);
    }
}