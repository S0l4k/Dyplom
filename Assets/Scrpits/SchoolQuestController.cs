using UnityEngine;
using System.Collections;
using FMODUnity;

public class SchoolQuestController : MonoBehaviour
{
    [Header("Quest Settings")]
    public string questID = "SchoolFlashback";
    public DialogActivator demonDialog;
    public SchoolAmbienceController ambience;

    [Header("Cutscene Settings")]
    public ScreenFader screenFader;

    [Header("Cutscene Camera (PlayerCamera with offset)")]
    public Transform demonCutscenePosition;
    public Vector3 cameraOffset = new Vector3(0, 1.7f, -3f);

    [Header("Audio")]
    public EventReference glassEatingSound;
    public float cutsceneFadeSpeed = 1.2f;

    [Header("Return Settings")]
    public NarrativeInspectTrigger linkedTrigger;
    public float delayBeforeReturn = 2f;

    // References
    private QuestManager _questManager;
    private bool _dialogFinished = false;
    private bool _cutscenePlayed = false;
    private bool _questAdded = false;
    private bool _initialized = false;  // ✅ NOWA flaga
    private Camera _playerCamera;
    private PlayerController _player;
    private PlayerCam _playerCamScript;
    public GameObject glass;
    public GameObject blood;
    // Camera backup
    private bool _wasCameraFollowEnabled;
    private Transform _originalCameraParent;
    private Vector3 _originalCameraLocalPos;
    private Quaternion _originalCameraLocalRot;

    void Start()
    {
        Debug.Log($"[SchoolQuest] 🎬 Start() called | questID={questID}");
        Initialize();
    }

    // ✅ NOWA METODA: Bezpieczna inicjalizacja (może być wywołana wielokrotnie)
    private void Initialize()
    {
        if (_initialized) return;

        Debug.Log("[SchoolQuest] 🔧 Initializing...");

        // Znajdź QuestManager – spróbuj kilka metod
        _questManager = QuestManager.Instance;
        if (_questManager == null)
            _questManager = FindObjectOfType<QuestManager>();
        if (_questManager == null)
            Debug.LogError("[SchoolQuest] ❌ QuestManager NOT FOUND!");
        else
            Debug.Log($"[SchoolQuest] ✅ QuestManager found: {_questManager.name}");

        _playerCamera = Camera.main;

        // Znajdź gracza – spróbuj kilka metod
        _player = FindObjectOfType<PlayerController>();
        if (_player == null && linkedTrigger != null && linkedTrigger.playerController != null)
            _player = linkedTrigger.playerController;
        if (_player == null)
            Debug.LogError("[SchoolQuest] ❌ PlayerController NOT FOUND!");
        else
            Debug.Log($"[SchoolQuest] ✅ Player found: {_player.name}");

        _playerCamScript = _playerCamera?.GetComponent<PlayerCam>();

        if (_playerCamera != null && _player != null)
        {
            _originalCameraParent = _playerCamera.transform.parent;
            _originalCameraLocalPos = _playerCamera.transform.localPosition;
            _originalCameraLocalRot = _playerCamera.transform.localRotation;
            _wasCameraFollowEnabled = _playerCamScript != null && _playerCamScript.enabled;
        }

        // ✅ START ODTWARZANIA AMBIENCE
        if (ambience != null)
        {
            Vector3 startPos = _player != null ? _player.transform.position : transform.position;
            ambience.StartPlaying(startPos);
            Debug.Log("[SchoolQuest] 🎵 Ambience started");
        }

        // ✅ Dodaj quest TERAZ, jeśli wszystko jest gotowe
        TryAddQuest();

        _initialized = true;
        Debug.Log("[SchoolQuest] ✅ Initialization complete");
    }

    // ✅ NOWA METODA: Bezpieczne dodanie questa
    private void TryAddQuest()
    {
        if (_questAdded) return;
        if (string.IsNullOrEmpty(questID)) return;
        if (_questManager == null)
        {
            Debug.LogWarning("[SchoolQuest] ⏳ QuestManager not ready, retrying next frame...");
            return;
        }

        _questManager.AddQuest(questID);
        _questAdded = true;
        Debug.Log($"[SchoolQuest] ➕ Quest ADDED: {questID}");
    }

    void Update()
    {
        // ✅ Upewnij się, że inicjalizacja się udała (na wypadek gdyby Start() było za wcześnie)
        if (!_initialized)
        {
            Initialize();
            return;
        }

        // ✅ Spróbuj dodać quest jeśli jeszcze się nie udało
        if (!_questAdded)
            TryAddQuest();

        // Sprawdź koniec dialogu
        if (!_dialogFinished && demonDialog != null && demonDialog.HasDialogJustFinished())
        {
            _dialogFinished = true;
            Debug.Log("[SchoolQuest] ✅ Dialog finished – starting cutscene");
            StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        if (_cutscenePlayed) yield break;
        _cutscenePlayed = true;

        Debug.Log("[SchoolQuest] 🎬 PlayCutscene() started");

        if (_player != null) _player.enabled = false;
        ambience?.FadeOutVoices(0.5f);

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);

        if (!glassEatingSound.IsNull && AudioManager.Instance != null && _player != null)
        {
            AudioManager.Instance.PlaySFX(glassEatingSound, _player.transform.position);
            Debug.Log("[SchoolQuest] 🔊 Glass eating sound played");
        }

        yield return new WaitForSeconds(10f);
        SetupCutsceneCamera();

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);

        yield return new WaitForSeconds(2f);

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);
        glass.SetActive(false);
        blood.SetActive(true);

        yield return new WaitForSeconds(delayBeforeReturn);

        // ✅ UKOŃCZ QUEST
        if (!string.IsNullOrEmpty(questID) && _questManager != null)
        {
            _questManager.CompleteQuest(questID);
            Debug.Log($"[SchoolQuest] ✅ Quest COMPLETED: {questID}");
        }

        RestorePlayerCamera();
        linkedTrigger?.EndFlashback();
    }

    private void SetupCutsceneCamera()
    {
        if (_playerCamera == null || demonCutscenePosition == null) return;

        if (_playerCamera.transform.parent != null)
            _playerCamera.transform.SetParent(null, worldPositionStays: true);

        _playerCamera.transform.position = demonCutscenePosition.position + cameraOffset;
        _playerCamera.transform.LookAt(demonCutscenePosition);

        if (_playerCamScript != null) _playerCamScript.enabled = false;
        Debug.Log("[SchoolQuest] 🎥 Cutscene camera setup complete");
    }

    private void RestorePlayerCamera()
    {
        if (_playerCamera == null || _player == null) return;

        bool wasEnabled = _playerCamera.enabled;
        _playerCamera.enabled = false;

        if (_originalCameraParent != null)
        {
            _playerCamera.transform.SetParent(_originalCameraParent, worldPositionStays: false);
            _playerCamera.transform.localPosition = _originalCameraLocalPos;
            _playerCamera.transform.localRotation = _originalCameraLocalRot;
        }

        if (_playerCamScript != null && _wasCameraFollowEnabled)
            _playerCamScript.enabled = true;

        _playerCamera.enabled = wasEnabled;
        if (_player != null) _player.enabled = true;

        Debug.Log("[SchoolQuest] 🎥 PlayerCamera restored to follow mode");
    }

    public void TriggerCutscene()
    {
        if (!_cutscenePlayed) StartCoroutine(PlayCutscene());
    }

    public void ForceCompleteQuest()
    {
        if (!string.IsNullOrEmpty(questID) && _questManager != null)
        {
            _questManager.CompleteQuest(questID);
            Debug.Log($"[SchoolQuest] ✅ Quest force-completed: {questID}");
        }
    }
}