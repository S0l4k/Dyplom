using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NarrativeInspectTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject inspectScene;
    public Camera playerCamera;
    public PlayerController playerController;
    public CanvasGroup gameplayUI;

    [Header("UI")]
    public TMP_Text interactionText;
    public Outline outline;

    [Header("Inspect UI")]
    public Button endInspectButton;
    public GameObject inspectUIPanel;

    [Header("Inspect Model")]
    public Transform childModelToEnable;

    [Header("Narrative")]
    [TextArea] public string beforeText = "You examine the object...";
    [TextArea] public string afterText = "Something feels off...";

    [Header("Settings")]
    public float typeSpeed = 0.05f;

    private Camera _inspectCamera;
    private InspectSystem _inspectSystem;
    private bool _canInteract = false;
    private bool _isInInspect = false;

    // ✅ NOWE: Statyczna referencja do AKTYWNEGO inspektora
    private static NarrativeInspectTrigger _activeInspector;

    void Start()
    {
        _inspectCamera = inspectScene?.GetComponentInChildren<Camera>();
        _inspectSystem = inspectScene?.GetComponentInChildren<InspectSystem>();

        if (interactionText != null) interactionText.gameObject.SetActive(false);
        if (outline != null) outline.enabled = false;
        if (inspectScene != null) inspectScene.SetActive(false);

        if (childModelToEnable != null) childModelToEnable.gameObject.SetActive(false);

        // ✅ FIX: Każdy obiekt dodaje listener, ale ExitInspect() reaguje TYLKO dla aktywnego
        if (endInspectButton != null)
        {
            endInspectButton.onClick.AddListener(ExitInspect);
            endInspectButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_isInInspect) return;

        CheckInteraction();

        if (_canInteract && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(EnterInspect());
    }

    void CheckInteraction()
    {
        if (!playerCamera || interactionText == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool wasCan = _canInteract;
        _canInteract = false;

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            if (hit.collider.gameObject == gameObject || hit.transform.IsChildOf(transform))
                _canInteract = true;
        }

        if (_canInteract != wasCan)
        {
            interactionText.gameObject.SetActive(_canInteract);
            interactionText.text = "Press E to examine";
            if (outline != null) outline.enabled = _canInteract;
        }
    }

    IEnumerator EnterInspect()
    {
        _isInInspect = true;
        _activeInspector = this; // ✅ Zarejestruj się jako aktywny inspektor

        interactionText.gameObject.SetActive(false);
        if (outline != null) outline.enabled = false;

        if (!string.IsNullOrEmpty(beforeText) && GameNarrativeManager.Instance != null)
        {
            yield return StartCoroutine(GameNarrativeManager.Instance.ShowThoughtWithStyle(
                beforeText, typeSpeed, 0.5f, "#FFFFFF80"));
        }

        if (playerController != null) playerController.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (gameplayUI != null) gameplayUI.interactable = false;
        if (gameplayUI != null) gameplayUI.blocksRaycasts = false;
        if (gameplayUI != null) StartCoroutine(FadeUI(gameplayUI, 0f));

        playerCamera.gameObject.SetActive(false);
        inspectScene.SetActive(true);

        if (childModelToEnable != null)
        {
            childModelToEnable.gameObject.SetActive(true);
            if (_inspectSystem != null)
                _inspectSystem.SetTarget(childModelToEnable);
        }

        if (endInspectButton != null) endInspectButton.gameObject.SetActive(true);
        if (inspectUIPanel != null) inspectUIPanel.SetActive(true);
    }

    // ✅ Publiczna metoda - wywoływana przez Button.onClick
    public void ExitInspect()
    {
        // ✅ KLUCZOWE: Reaguj TYLKO jeśli to TY jesteś aktywnym inspektorem
        if (_activeInspector != this) return;

        _isInInspect = false;
        _activeInspector = null; // ✅ Wyczyść referencję

        if (!string.IsNullOrEmpty(afterText) && GameNarrativeManager.Instance != null)
        {
            StartCoroutine(GameNarrativeManager.Instance.ShowThoughtWithStyle(
                afterText, typeSpeed, 1.5f, "#AAAAFF80"));
        }

        if (playerController != null) playerController.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (gameplayUI != null) StartCoroutine(FadeUI(gameplayUI, 1f));
        if (gameplayUI != null) gameplayUI.interactable = true;
        if (gameplayUI != null) gameplayUI.blocksRaycasts = true;

        if (childModelToEnable != null)
            childModelToEnable.gameObject.SetActive(false);

        if (endInspectButton != null) endInspectButton.gameObject.SetActive(false);
        if (inspectUIPanel != null) inspectUIPanel.SetActive(false);

        inspectScene.SetActive(false);
        playerCamera.gameObject.SetActive(true);
    }

    IEnumerator FadeUI(CanvasGroup cg, float targetAlpha)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            cg.alpha = Mathf.Lerp(start, targetAlpha, t);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

 
}