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
    public Transform playerTransform;

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

    [Header("Flashback")]
    public Transform flashbackLocation;
    public GameObject flashbackScene;
    public bool enableFlashback = true;
    public ScreenFader screenFader;  // ✅ TEN SAM komponent co w GameNarrativeManager!

    [Header("Settings")]
    public float typeSpeed = 0.05f;
    public float fadeSpeed = 0.8f;   // ✅ Domyślny czas fade (jak w VomitSequence)

    private Camera _inspectCamera;
    private InspectSystem _inspectSystem;
    private bool _canInteract = false;
    private bool _isInInspect = false;
    private bool _isInFlashback = false;

    private Vector3 _playerReturnPosition;
    private Quaternion _playerReturnRotation;

    private static NarrativeInspectTrigger _activeInspector;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        _inspectCamera = inspectScene?.GetComponentInChildren<Camera>();
        _inspectSystem = inspectScene?.GetComponentInChildren<InspectSystem>();

        if (interactionText != null) interactionText.gameObject.SetActive(false);
        if (outline != null) outline.enabled = false;
        if (inspectScene != null) inspectScene.SetActive(false);
        if (flashbackScene != null) flashbackScene.SetActive(false);
        if (childModelToEnable != null) childModelToEnable.gameObject.SetActive(false);

        if (endInspectButton != null)
        {
            endInspectButton.onClick.AddListener(ExitInspect);
            endInspectButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_isInInspect || _isInFlashback) return;

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
        _activeInspector = this;

        Transform playerTrans = playerTransform != null ? playerTransform : playerController?.transform;
        if (playerTrans != null)
        {
            _playerReturnPosition = playerTrans.position;
            _playerReturnRotation = playerTrans.rotation;
        }

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

        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (inspectScene != null) inspectScene.SetActive(true);

        if (childModelToEnable != null)
        {
            childModelToEnable.gameObject.SetActive(true);
            if (_inspectSystem != null)
                _inspectSystem.SetTarget(childModelToEnable);
        }

        if (endInspectButton != null) endInspectButton.gameObject.SetActive(true);
        if (inspectUIPanel != null) inspectUIPanel.SetActive(true);
    }

    public void ExitInspect()
    {
        if (_activeInspector != this) return;

        // 🔑 Włącz kamerę gracza NATYCHMIAST
        if (playerCamera != null && !playerCamera.gameObject.activeSelf)
            playerCamera.gameObject.SetActive(true);

        _isInInspect = false;
        _activeInspector = null;

        // Wyłącz inspect UI
        if (inspectScene != null) inspectScene.SetActive(false);
        if (childModelToEnable != null) childModelToEnable.gameObject.SetActive(false);
        if (endInspectButton != null) endInspectButton.gameObject.SetActive(false);
        if (inspectUIPanel != null) inspectUIPanel.SetActive(false);

        if (gameplayUI != null)
        {
            gameplayUI.alpha = 1f;
            gameplayUI.interactable = true;
            gameplayUI.blocksRaycasts = true;
        }

        // 🔤 Pokaż tekst po inspekcji
        if (!string.IsNullOrEmpty(afterText) && GameNarrativeManager.Instance != null)
        {
            StartCoroutine(ShowAfterTextAndContinue());
        }
        else
        {
            ContinueAfterInspect();
        }
    }

    private IEnumerator ShowAfterTextAndContinue()
    {
        yield return StartCoroutine(GameNarrativeManager.Instance.ShowThoughtWithStyle(
            afterText, typeSpeed, 0.8f, "#AAAAFF80"));

        yield return new WaitForSeconds(0.3f);
        ContinueAfterInspect();
    }

    private void ContinueAfterInspect()
    {
        if (enableFlashback && flashbackLocation != null)
        {
            StartFlashback();
        }
        else
        {
            RestorePlayerControl();
        }
    }

    // ✅ PROSTY TELEPORT – VOMIT SEQUENCE STYLE
    private void StartFlashback()
    {
        Debug.Log($"[Flashback] 🌀 Starting flashback for {gameObject.name}");
        _isInFlashback = true;

        StartCoroutine(FlashbackTeleportSequence());
    }

    // ✅ KORUTINA TELEPORTU – dokładna kopia logiki z VomitSequence
    private IEnumerator FlashbackTeleportSequence()
    {
        // 🔒 Zablokuj gracza (jak w VomitSequence)
        if (playerController != null) playerController.enabled = false;
        if (playerCamera != null) playerCamera.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        // 🌑 Fade OUT
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeOut(fadeSpeed));
        }
        else
        {
            // Fallback: prosta pauza jeśli nie ma ScreenFader
            yield return new WaitForSeconds(fadeSpeed);
        }

        // 🌀 TELEPORT GRACZA (z CharacterController handling jak w VomitSequence)
        if (flashbackLocation != null)
        {
            Transform playerTrans = playerTransform != null ? playerTransform : playerController?.transform;

            if (playerTrans != null)
            {
                CharacterController cc = playerController?.GetComponent<CharacterController>();
                Vector3 targetPos = flashbackLocation.position;
                Quaternion targetRot = flashbackLocation.rotation;

                // ✅ Raycast do podłogi (jak w VomitSequence) – żeby gracz nie wisiał w powietrzu
                if (cc != null && Physics.Raycast(
                    flashbackLocation.position + Vector3.up * 2f,
                    Vector3.down,
                    out RaycastHit hit,
                    5f,
                    LayerMask.GetMask("Default", "Floor", "Environment")))
                {
                    targetPos = hit.point + Vector3.up * (cc.height * 0.5f) - Vector3.up * 1f;
                }

                // ✅ Wykonaj teleport
                playerTrans.position = targetPos;
                playerTrans.rotation = targetRot;

                // ✅ Jeśli kamera jest childem gracza – zaktualizuj jej rotację
                if (playerCamera != null && playerCamera.transform.parent == playerTrans)
                {
                    // Kamera już podąży za graczem, ale dla pewności:
                    playerCamera.transform.rotation = targetRot;
                }

                Debug.Log($"[Flashback] 🚀 Teleported to {flashbackLocation.name} at {targetPos}");
            }
        }

        // 🎬 Aktywuj scenę flashbacku
        if (flashbackScene != null)
        {
            flashbackScene.SetActive(true);
            if (QuestManager.Instance != null && QuestManager.Instance.questPanel != null)
            {
                QuestManager.Instance.questPanel.SetActive(false);
                Debug.Log("[Narrative] 📋 Quest UI hidden during flashback");
            }
        }
        // 🌕 Fade IN
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeIn(fadeSpeed));
        }
        else
        {
            yield return new WaitForSeconds(fadeSpeed);
        }

        // 🔓 Odblokuj gracza w flashbacku
        RestorePlayerControl();

    }

    // ✅ POWRÓT Z FLASHBACKU – identyczna logika
    public void EndFlashback()
    {
        if (!_isInFlashback) return;

        Debug.Log($"[Flashback] 🔙 Returning from flashback for {gameObject.name}");
        _isInFlashback = false;

        StartCoroutine(ReturnFromFlashbackSequence());
    }

    private IEnumerator ReturnFromFlashbackSequence()
    {
        // 🔒 Zablokuj gracza
        if (playerController != null) playerController.enabled = false;
        if (playerCamera != null) playerCamera.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        StopFlashbackAmbience();
        // 🌑 Fade OUT
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeOut(fadeSpeed));
        }
        else
        {
            yield return new WaitForSeconds(fadeSpeed);
        }

        // 🌀 TELEPORT POWROTNY
        Transform playerTrans = playerTransform != null ? playerTransform : playerController?.transform;
        if (playerTrans != null)
        {
            CharacterController cc = playerController?.GetComponent<CharacterController>();
            Vector3 targetPos = _playerReturnPosition;
            Quaternion targetRot = _playerReturnRotation;

            // ✅ Raycast do podłogi dla bezpieczeństwa
            if (cc != null && Physics.Raycast(
                _playerReturnPosition + Vector3.up * 2f,
                Vector3.down,
                out RaycastHit hit,
                5f,
                LayerMask.GetMask("Default", "Floor", "Environment")))
            {
                targetPos = hit.point + Vector3.up * (cc.height * 0.5f) - Vector3.up * 1f;
            }

            playerTrans.position = targetPos;
            playerTrans.rotation = targetRot;

            Debug.Log($"[Flashback] 🔙 Returned to saved position: {targetPos}");
        }

        // 🎬 Wyłącz scenę flashbacku
        if (flashbackScene != null) flashbackScene.SetActive(false);

        // 🌕 Fade IN
        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeIn(fadeSpeed));
        }
        else
        {
            yield return new WaitForSeconds(fadeSpeed);
        }

        if (QuestManager.Instance != null && QuestManager.Instance.questPanel != null)
        {
            QuestManager.Instance.questPanel.SetActive(true);
            Debug.Log("[Narrative] 📋 Quest UI restored after flashback");
        }
        // 🔓 Przywróć normalną grę
        RestorePlayerControl();
    }
    private void StopFlashbackAmbience()
    {
        // Jeśli flashbackScene ma SchoolAmbienceController, zatrzymaj go
        if (flashbackScene != null)
        {
            var ambience = flashbackScene.GetComponentInChildren<SchoolAmbienceController>();
            if (ambience != null)
            {
                ambience.StopVoices(true); // fade out
                Debug.Log("[Narrative] 🔇 Flashback ambience stopped");
            }
        }
    }
    // ✅ JEDNA METODA DO ODBLOKOWANIA – jak w GameNarrativeManager
    private void RestorePlayerControl()
    {
        if (playerController != null) playerController.enabled = true;
        if (playerCamera != null) playerCamera.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log($"[RestorePlayerControl] ✅ Player unlocked");
    }

    // ✅ Helper do fade UI (tylko dla gameplayUI, nie ekranu)
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