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

    [Header("Blood & SFX")]
    [Tooltip("Dźwięk krzyku dzieci po zjedzeniu szkła")]
    public EventReference childrenScreamSound;

    [Tooltip("Obiekt krwi w szkole (aktywowany po cutscence)")]
    public GameObject schoolBlood;

    private QuestManager _questManager;
    private bool _dialogFinished = false;
    private bool _cutscenePlayed = false;
    private bool _questAdded = false;
    private bool _initialized = false;  // ✅ NOWA flaga
    private Camera _playerCamera;
    private PlayerController _player;
    private PlayerCam _playerCamScript;
    public GameObject glass;
    
    private bool _wasCameraFollowEnabled;
    private Transform _originalCameraParent;
    private Vector3 _originalCameraLocalPos;
    private Quaternion _originalCameraLocalRot;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized) return;

        _questManager = QuestManager.Instance;
        if (_questManager == null)
            _questManager = FindObjectOfType<QuestManager>();
        _playerCamera = Camera.main;

        
        _player = FindObjectOfType<PlayerController>();
        if (_player == null && linkedTrigger != null && linkedTrigger.playerController != null)
            _player = linkedTrigger.playerController;
        _playerCamScript = _playerCamera?.GetComponent<PlayerCam>();

        if (_playerCamera != null && _player != null)
        {
            _originalCameraParent = _playerCamera.transform.parent;
            _originalCameraLocalPos = _playerCamera.transform.localPosition;
            _originalCameraLocalRot = _playerCamera.transform.localRotation;
            _wasCameraFollowEnabled = _playerCamScript != null && _playerCamScript.enabled;
        }

        if (ambience != null)
        {
            Vector3 startPos = _player != null ? _player.transform.position : transform.position;
            ambience.StartPlaying(startPos);
        }

        TryAddQuest();

        _initialized = true;
    }

    private void TryAddQuest()
    {
        if (_questAdded) return;
        if (string.IsNullOrEmpty(questID)) return;
        if (_questManager == null)
        {
            return;
        }

        _questManager.AddQuest(questID);
        _questAdded = true;
    }

    void Update()
    {
        if (!_initialized)
        {
            Initialize();
            return;
        }

        if (!_questAdded)
            TryAddQuest();

        if (!_dialogFinished && demonDialog != null && demonDialog.HasDialogJustFinished())
        {
            _dialogFinished = true;
            StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        if (_cutscenePlayed) yield break;
        _cutscenePlayed = true;

        if (ambience != null)
        {
            ambience.StopVoices(false);
        }

        if (glass != null)
        {
            glass.SetActive(false);
        }

        if (demonDialog != null)
        {
            demonDialog.enabled = false;
        }

        
        if (_player != null) _player.enabled = false;

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);

        if (!glassEatingSound.IsNull && AudioManager.Instance != null && _player != null)
        {
            AudioManager.Instance.PlaySFX(glassEatingSound, _player.transform.position);
        }

        yield return new WaitForSeconds(10f);

        if (schoolBlood != null)
        {
            schoolBlood.SetActive(true);
        }
        SetupCutsceneCamera();

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);


        if (!childrenScreamSound.IsNull && AudioManager.Instance != null)
        {
            Vector3 playPos = _player != null ? _player.transform.position : transform.position;
            AudioManager.Instance.PlaySFX(childrenScreamSound, playPos);
        }

        yield return new WaitForSeconds(2f);

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);

        if (glass != null) glass.SetActive(false);

        yield return new WaitForSeconds(delayBeforeReturn);

        if (!string.IsNullOrEmpty(questID) && _questManager != null)
        {
            _questManager.CompleteQuest(questID);
        }

        RestorePlayerCamera();
        GameState.IsInFlashback = false;
        if (linkedTrigger != null)
        {
            linkedTrigger.EndFlashback();
        }

    }

    private void SetupCutsceneCamera()
    {
        if (_playerCamera == null || demonCutscenePosition == null) return;

        if (_playerCamera.transform.parent != null)
            _playerCamera.transform.SetParent(null, worldPositionStays: true);

        _playerCamera.transform.position = demonCutscenePosition.position + cameraOffset;
        _playerCamera.transform.LookAt(demonCutscenePosition);

        if (_playerCamScript != null) _playerCamScript.enabled = false;
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
        }
    }
}