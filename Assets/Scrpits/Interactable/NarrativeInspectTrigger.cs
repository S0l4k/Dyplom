using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class NarrativeInspectTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject inspectScene;
    public Camera playerCamera;
    public PlayerController playerController;
    public CanvasGroup gameplayUI;
    public Transform playerTransform;

    [Header("UI")]
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
    public ScreenFader screenFader;

    [Header("Settings")]
    public float typeSpeed = 0.05f;
    public float fadeSpeed = 0.8f;

    private Camera _inspectCamera;
    private InspectSystem _inspectSystem;
    private bool _canInteract = false;
    private bool _isInInspect = false;
    private bool _isInFlashback = false;
    private bool _hasBeenUsed = false;

    // ✅ ID questa do trackowania
    [Tooltip("ID questa do trackowania postępu (np. 'SchoolFlashback')")]
    public string flashbackQuestID;

    private Vector3 _playerReturnPosition;
    private Quaternion _playerReturnRotation;
    private static NarrativeInspectTrigger _activeInspector;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        _inspectCamera = inspectScene?.GetComponentInChildren<Camera>();
        _inspectSystem = inspectScene?.GetComponentInChildren<InspectSystem>();

        if (outline != null) outline.enabled = false;
        if (inspectScene != null) inspectScene.SetActive(false);
        if (flashbackScene != null) flashbackScene.SetActive(false);
        if (childModelToEnable != null) childModelToEnable.gameObject.SetActive(false);

        if (endInspectButton != null)
        {
            endInspectButton.onClick.AddListener(ExitInspect);
            endInspectButton.gameObject.SetActive(false);
        }

        // ✅ DEBUG: Sprawdź stan na start
        Debug.Log($"[NarrativeInspect] {gameObject.name} | enableFlashback={enableFlashback}, questID={flashbackQuestID}");
    }

    void Update()
    {
        // Jeśli obiekt był już użyty lub w trakcie interakcji – zablokuj
        if (_isInInspect || _isInFlashback || _hasBeenUsed) return;

        CheckInteraction();

        if (_canInteract && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(EnterInspect());
    }

    void CheckInteraction()
    {
        // ✅ DEBUG: Loguj stan checka
        Debug.Log($"[CheckInteraction] {gameObject.name} | unlocked={GameState.ApartmentExplorationUnlocked}, used={_hasBeenUsed}");

        // ✅ Blokada jeśli flashbacki nie są jeszcze odblokowane
        if (enableFlashback && !GameState.ApartmentExplorationUnlocked)
        {
            Debug.Log($"[CheckInteraction] ❌ BLOCKED: ApartmentExplorationUnlocked={GameState.ApartmentExplorationUnlocked}");
            if (outline != null) outline.enabled = false;
            _canInteract = false;
            return;
        }

        // ✅ Jeśli obiekt był już użyty – zablokuj
        if (_hasBeenUsed)
        {
            Debug.Log($"[CheckInteraction] ❌ BLOCKED: Object already used");
            if (outline != null) outline.enabled = false;
            _canInteract = false;
            return;
        }

        // ✅ FIX: Poprawny check kamery (było: !playerCamera== false)
        if (playerCamera == null)
        {
            Debug.LogWarning($"[CheckInteraction] ❌ playerCamera is NULL on {gameObject.name}");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool wasCan = _canInteract;
        _canInteract = false;

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            if (hit.collider.gameObject == gameObject || hit.transform.IsChildOf(transform))
            {
                _canInteract = true;
                Debug.Log($"[CheckInteraction] ✅ Raycast HIT on {gameObject.name}");
            }
        }

        if (_canInteract != wasCan)
        {
            if (outline != null) outline.enabled = _canInteract;
            Debug.Log($"[CheckInteraction] outline.enabled = {_canInteract}");
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

        if (playerCamera != null && !playerCamera.gameObject.activeSelf)
            playerCamera.gameObject.SetActive(true);

        _isInInspect = false;
        _activeInspector = null;

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

    private void StartFlashback()
    {
        Debug.Log($"[Flashback] 🌀 Starting flashback for {gameObject.name}");
        _isInFlashback = true;
        StartCoroutine(FlashbackTeleportSequence());
    }

    private IEnumerator FlashbackTeleportSequence()
    {
        if (playerController != null) playerController.enabled = false;
        if (playerCamera != null) playerCamera.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut(fadeSpeed));
        else
            yield return new WaitForSeconds(fadeSpeed);

        if (flashbackLocation != null)
        {
            Transform playerTrans = playerTransform != null ? playerTransform : playerController?.transform;

            if (playerTrans != null)
            {
                CharacterController cc = playerController?.GetComponent<CharacterController>();
                Vector3 targetPos = flashbackLocation.position;
                Quaternion targetRot = flashbackLocation.rotation;

                if (cc != null && Physics.Raycast(
                    flashbackLocation.position + Vector3.up * 2f,
                    Vector3.down,
                    out RaycastHit hit,
                    5f,
                    LayerMask.GetMask("Default", "Floor", "Environment")))
                {
                    targetPos = hit.point + Vector3.up * (cc.height * 0.5f) - Vector3.up * 1f;
                }

                playerTrans.position = targetPos;
                playerTrans.rotation = targetRot;

                if (playerCamera != null && playerCamera.transform.parent == playerTrans)
                {
                    playerCamera.transform.rotation = targetRot;
                }

                Debug.Log($"[Flashback] 🚀 Teleported to {flashbackLocation.name} at {targetPos}");
            }
        }

        if (flashbackScene != null)
        {
            flashbackScene.SetActive(true);
            if (QuestManager.Instance != null && QuestManager.Instance.questPanel != null)
            {
                QuestManager.Instance.questPanel.SetActive(false);
            }
        }

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn(fadeSpeed));
        else
            yield return new WaitForSeconds(fadeSpeed);

        RestorePlayerControl();
    }

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
        _hasBeenUsed = true;

        if (enableFlashback && !string.IsNullOrEmpty(flashbackQuestID))
        {
            GameNarrativeManager.Instance?.OnFlashbackCompleted(flashbackQuestID);
            Debug.Log($"[Narrative] 📢 Flashback completed: {flashbackQuestID}");
        }

        // ❌ USUNIĘTE: FadeOut na początku – ekran JEST już czarny z AtticQuest!
        // 🌑 Fade OUT – już jest czarno, więc to tylko dla pewności  ← USUŃ TO CAŁKOWICIE!
        // if (screenFader != null) yield return StartCoroutine(screenFader.FadeOut(fadeSpeed));

        // 🌀 TELEPORT POWROTNY (nadal na czarnym ekranie)
        Transform playerTrans = playerTransform != null ? playerTransform : playerController?.transform;
        if (playerTrans != null)
        {
            CharacterController cc = playerController?.GetComponent<CharacterController>();
            Vector3 targetPos = _playerReturnPosition;
            Quaternion targetRot = _playerReturnRotation;

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

        // ✅ TERAZ: Fade IN – dopiero po teleportacji do mieszkania!
        Debug.Log("[ReturnFromFlashback] 🌕 Fading IN after teleport to apartment...");

        // ✅ Najpierw upewnij się, że ekran jest CZARNY przed FadeIn
        if (screenFader != null)
        {
            // Ustaw alpha na 1 (czarny) na wszelki wypadek
            var img = screenFader.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = new Color(0, 0, 0, 1);

            // Teraz FadeIn
            yield return StartCoroutine(screenFader.FadeIn(fadeSpeed));
        }
        else
        {
            yield return new WaitForSeconds(fadeSpeed);
        }

        if (QuestManager.Instance != null && QuestManager.Instance.questPanel != null)
        {
            QuestManager.Instance.questPanel.SetActive(true);
        }

        // 🔓 Przywróć normalną grę
        RestorePlayerControl();

        Debug.Log("[ReturnFromFlashback] ✅ FadeIn complete – player back in apartment");
    }
    private void StopFlashbackAmbience()
    {
        if (flashbackScene != null)
        {
            var ambience = flashbackScene.GetComponentInChildren<SchoolAmbienceController>();
            if (ambience != null)
            {
                ambience.StopVoices(true);
                Debug.Log("[Narrative] 🔇 Flashback ambience stopped");
            }
        }
    }

    private void RestorePlayerControl()
    {
        Debug.Log("[RestorePlayerControl] 🔓 Restoring full player control...");

        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("  ├─ PlayerController.enabled = true");
        }

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            Debug.Log("  ├─ playerCamera GameObject.enabled = true");
        }

        PlayerCam playerCamScript = playerCamera?.GetComponent<PlayerCam>();
        if (playerCamScript != null)
        {
            playerCamScript.enabled = true;
            Debug.Log("  ├─ PlayerCam script.enabled = true");
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("  └─ Cursor locked, hidden");

        Debug.Log("[RestorePlayerControl] ✅ Player fully unlocked");
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