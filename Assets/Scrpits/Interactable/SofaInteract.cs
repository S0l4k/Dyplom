using UnityEngine;
using TMPro;
using System.Collections;
using FMODUnity;
using UnityEngine.SceneManagement;
using Studio = FMOD.Studio;

public class SofaInteract : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform sofaViewPoint;        // ✅ Punkt widoku LEŻĄCEGO na kanapie
    public TMP_Text interactText;
    public ScreenFader screenFader;        // ✅ Do fade to black
    public TMP_Text thoughtText;           // ✅ Tekst myśli (ten sam co w GameNarrativeManager)

    [Header("Audio")]
    public EventReference demonVoiceEvent; // ✅ Głos demona "You won't free yourself..."

    [Header("Visual Styling")]
    [SerializeField] private string demonMarkerColor = "#FF000080"; // ✅ Czerwony marker

    [Header("Settings")]
    public float useRange = 3f;
    public float moveSpeed = 2.5f;         // ✅ Wolniejsze przejście niż komputer
    public float typeSpeed = 0.07f;        // ✅ Wolniejsze pisanie dla dramatyzmu

    private bool canUse = false;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    private PlayerController playerController;
    private PlayerCam playerCam;
    private Studio.EventInstance demonVoiceInstance; // ✅ Instancja głosu

    void Start()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false);

        playerController = FindObjectOfType<PlayerController>();
        playerCam = FindObjectOfType<PlayerCam>();
        demonVoiceInstance = new Studio.EventInstance();
    }

    void Update()
    {
        // ✅ Aktywuj Tylko PO podniesieniu leków (gdy InteractionsLocked = true)
        if (!GameState.InteractionsLocked)
        {
            canUse = false;
            if (interactText != null) interactText.gameObject.SetActive(false);
            return;
        }

        // ✅ Już w trakcie sekwencji – nie pozwalaj na ponowną interakcję
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

        // ✅ ZABLOKUJ GRACZA
        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;

        originalCamPosition = playerCamera.position;
        originalCamRotation = playerCamera.rotation;

        // ✅ PŁYNNE PRZEJŚCIE DO POZYCJI NA KANAPIE
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

        // ✅ PO 2 SEKUNDACH: ODPOWIEDŹ DEMONA (TEKST + GŁOS ZSYNCHRONIZOWANE)
        yield return new WaitForSeconds(2f);

        // ✅ POKAŻ MYŚL DEMONA Z GŁOSEM (jak w Dialog.cs)
        string demonLine = "You won't free yourself that easy...";
        yield return StartCoroutine(TypeDemonThought(demonLine));

        // ✅ DODATKOWA PAUZA PO TEKŚCIE (głos może dokończyć)
        yield return new WaitForSeconds(0.8f);

        // ✅ ZATRZYMAJ GŁOS (na wszelki wypadek)
        if (demonVoiceInstance.isValid())
        {
            demonVoiceInstance.stop(Studio.STOP_MODE.ALLOWFADEOUT);
            demonVoiceInstance.release();
        }

        // ✅ FADE TO BLACK
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeOut(1.2f));
        }
        else
        {
            // ✅ Fallback: ręczny fade
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

        // ✅ PO 1 SEKUNDZIE CZARNOŚCI: WRÓĆ DO MAIN MENU
        yield return new WaitForSeconds(1f);

        Debug.Log("[SofaInteract] 🎬 Ładowanie Main Menu...");
        SceneManager.LoadScene("MainMenu");
    }

    // ✅ SYNCHRONIZOWANE PISANIE TEKSTU Z GŁOSEM (jak w Dialog.cs)
    private IEnumerator TypeDemonThought(string text)
    {
        if (thoughtText == null)
        {
            Debug.LogError("[SofaInteract] ❌ thoughtText nie przypisany!");
            yield break;
        }

        // ✅ AKTYWUJ TEKST
        thoughtText.gameObject.SetActive(true);
        thoughtText.color = Color.white;
        thoughtText.text = "";

        // ✅ ROZPOCZNIJ GŁOS DEMONA (jak w Dialog.cs)
        if (!demonVoiceEvent.IsNull)
        {
            demonVoiceInstance = RuntimeManager.CreateInstance(demonVoiceEvent);
            demonVoiceInstance.start();
            Debug.Log("[SofaInteract] 🔊 Głos demona rozpoczęty RAZEM z tekstem");
        }

        // ✅ PISZ TEKST Z MARKEREM (czerwony)
        string openTag = $"<mark={demonMarkerColor}>";
        string closeTag = "</mark>";

        for (int i = 0; i < text.Length; i++)
        {
            thoughtText.text = openTag + text.Substring(0, i + 1) + closeTag;
            yield return new WaitForSeconds(typeSpeed);
        }

        // ✅ ZATRZYMAJ GŁOS PO ZAKOŃCZENIU TEKSTU (jak w Dialog.cs)
        if (demonVoiceInstance.isValid())
        {
            demonVoiceInstance.stop(Studio.STOP_MODE.IMMEDIATE);
            demonVoiceInstance.release();
            Debug.Log("[SofaInteract] 🔇 Głos demona zatrzymany po zakończeniu tekstu");
        }

        // ✅ PAUZA PO TEKŚCIE
        yield return new WaitForSeconds(1.5f);

        // ✅ UKRYJ TEKST
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
        // ✅ ZAWSZE ZATRZYMAJ GŁOS przy niszczeniu obiektu
        if (demonVoiceInstance.isValid())
        {
            demonVoiceInstance.stop(Studio.STOP_MODE.IMMEDIATE);
            demonVoiceInstance.release();
        }
    }
}