using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;

public class AtticQuestController : MonoBehaviour
{
    [Header("Quest")]
    public string questID = "AtticFlashback";
    public int candlesToLight = 8;

    [Header("Cutscene")]
    public ScreenFader screenFader;
    public Transform demonCutscenePosition;
    public Transform demonJumpscarePosition;
    public Vector3 cameraOffset = new Vector3(0, 1.7f, -4f);

    public Animator demonAnimator;
    public string animTurnOn = "turn_on";
    public string animPoint = "point_at_player";
    public string animJumpscare = "jumpscare";

    [Header("Audio")]
    public EventReference candleLightSound;
    public EventReference demonAppearSound;
    public EventReference demonJumpscareSound;
    public float cutsceneFadeSpeed = 1.2f;

    [Header("Return")]
    public NarrativeInspectTrigger linkedTrigger;
    public float delayBeforeReturn = 2f;

    public AtticCutsceneTrigger cutsceneTrigger;
    public GameObject demonGameObject;
    public GameObject cutsceneCollider;

    [Header("Candles to Destroy")]
    public GameObject mainCandleToDestroy;

    [Header("Blood Effects")]
    public GameObject[] bloods; 

    public GameObject hintbox;
    private QuestManager _questManager;
    private int _litCount = 0;
    private bool _cutscenePlayed = false;
    private bool _cameraLockedOnDemon = false;
    private PlayerController _player;
    private Camera _mainCamera;
    private PlayerCam _playerCam;

    private FMOD.Studio.EventInstance _demonSoundInstance;

    private Transform _camOriginalParent;
    private Vector3 _camOriginalLocalPos;
    private Quaternion _camOriginalLocalRot;
    private bool _camWasEnabled;

    public PlayerController GetPlayer() => _player;
    public int GetLitCount() => _litCount;

    void Start()
    {
        _questManager = QuestManager.Instance;
        _player = FindObjectOfType<PlayerController>();
        _mainCamera = Camera.main;
        _playerCam = _mainCamera?.GetComponent<PlayerCam>();

        if (_mainCamera != null)
        {
            _camOriginalParent = _mainCamera.transform.parent;
            _camOriginalLocalPos = _mainCamera.transform.localPosition;
            _camOriginalLocalRot = _mainCamera.transform.localRotation;
            _camWasEnabled = _playerCam != null && _playerCam.enabled;
        }

        if (!string.IsNullOrEmpty(questID) && _questManager != null)
            _questManager.AddQuest(questID);

        if (demonGameObject != null)
            demonGameObject.SetActive(false);

        if (cutsceneTrigger != null)
            cutsceneTrigger.enabled = false;

        SetBloodParticlesActive(false);

    }

    public void OnCandleLit()
    {
        _litCount++;

        if (!candleLightSound.IsNull && AudioManager.Instance != null && _player != null)
            AudioManager.Instance.PlaySFX(candleLightSound, transform.position);

        if (_litCount >= candlesToLight && cutsceneTrigger != null)
        {
            cutsceneCollider.SetActive(true);
            cutsceneTrigger.enabled = true;
            hintbox.SetActive(true);
        }
    }

    public void StartCutscene()
    {
        if (_cutscenePlayed) return;
        StartCoroutine(PlayCutscene());
        hintbox.SetActive(false); 
    }

    private void SetBloodParticlesActive(bool active)
    {
        if (bloods == null) return;

        foreach (var blood in bloods)
        {
            if (blood != null)
            {
                blood.SetActive(active);

                var particle = blood.GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    if (active) particle.Play();
                    else particle.Stop();
                }
            }
        }
    }

    IEnumerator PlayCutscene()
    {
        _cutscenePlayed = true;

        if (_player != null) _player.enabled = false;

        if (_mainCamera != null && demonCutscenePosition != null)
        {
            _mainCamera.transform.SetParent(null, true);
            _mainCamera.transform.position = demonCutscenePosition.position + cameraOffset;
            _mainCamera.transform.LookAt(demonCutscenePosition);
            _cameraLockedOnDemon = true;
            if (_playerCam != null) _playerCam.enabled = false;
        }

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);

        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);

        SetBloodParticlesActive(true);

        if (demonGameObject != null)
        {
            demonGameObject.SetActive(true);
            if (demonAnimator != null && !demonAnimator.enabled)
                demonAnimator.enabled = true;
        }

        if (!demonAppearSound.IsNull && AudioManager.Instance != null)
        {
            _demonSoundInstance = AudioManager.Instance.PlayDialogVoice(demonAppearSound);
        }

        yield return new WaitForSeconds(5f);

        if (demonAnimator != null && !string.IsNullOrEmpty(animPoint))
        {
            demonAnimator.ResetTrigger(animTurnOn);
            demonAnimator.ResetTrigger(animJumpscare);
            demonAnimator.SetTrigger(animPoint);
            yield return new WaitForSeconds(3f);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        var candles = FindObjectsOfType<CandleController>();
        foreach (var c in candles) if (c != null) c.SetLit(false);

        if (demonGameObject != null) demonGameObject.SetActive(false);

        if (_demonSoundInstance.isValid() && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopDialogVoice(ref _demonSoundInstance, true);
        }

        yield return new WaitForSeconds(1.5f);

        foreach (var c in candles) if (c != null) c.SetLit(true);
        yield return new WaitForSeconds(3f);

        foreach (var c in candles) if (c != null) c.SetLit(false);
        yield return new WaitForSeconds(1.5f);

        SetBloodParticlesActive(false);

        if (demonGameObject != null)
        {
            demonGameObject.SetActive(true);

            if (demonJumpscarePosition != null)
            {
                demonGameObject.transform.position = demonJumpscarePosition.position;
                demonGameObject.transform.rotation = demonJumpscarePosition.rotation;
            }
            else if (_player != null)
            {
                demonGameObject.transform.position = _player.transform.position + _player.transform.forward * 2f;
                demonGameObject.transform.LookAt(_player.transform);
            }

            if (demonAnimator != null && !string.IsNullOrEmpty(animJumpscare))
            {
                demonAnimator.ResetTrigger(animPoint);
                demonAnimator.SetTrigger(animJumpscare);
            }
        }

        if (!demonJumpscareSound.IsNull && AudioManager.Instance != null && _player != null)
        {
            AudioManager.Instance.PlaySFX(demonJumpscareSound, _player.transform.position);
        }

        yield return new WaitForSeconds(1.5f);

        if (screenFader != null)
        {
            yield return StartCoroutine(screenFader.FadeOut(0.3f)); 
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }


        foreach (var c in candles)
        {
            if (c != null && c.gameObject != null)
            {
                Destroy(c.gameObject);
            }
        }

        if (mainCandleToDestroy != null && mainCandleToDestroy.gameObject != null)
        {
            Destroy(mainCandleToDestroy.gameObject);
        }

        yield return new WaitForSeconds(0.5f);

        if (!string.IsNullOrEmpty(questID) && _questManager != null)
        {
            _questManager.CompleteQuest(questID);
        }

        GameState.IsInFlashback = false;
        _cameraLockedOnDemon = false;

        if (_playerCam != null && _camWasEnabled)
        {
            _playerCam.enabled = true;
            _playerCam.SyncRotationWithCamera();
        }

        if (linkedTrigger != null)
        {
            linkedTrigger.EndFlashback();
        }

        if (_mainCamera != null)
        {
            _mainCamera.enabled = false;
            if (_camOriginalParent != null)
            {
                _mainCamera.transform.SetParent(_camOriginalParent, false);
                _mainCamera.transform.localPosition = _camOriginalLocalPos;
                _mainCamera.transform.localRotation = _camOriginalLocalRot;
            }
            if (_playerCam != null && _camWasEnabled) _playerCam.enabled = true;
            _mainCamera.enabled = true;
        }
    }
}