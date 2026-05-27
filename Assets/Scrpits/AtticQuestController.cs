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

    // ✅ Animacje demona
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

    // ✅ NOWE: Przypisz główną świeczkę do zniszczenia po cutscence
    [Header("Candles to Destroy")]
    public GameObject mainCandleToDestroy;  // ✅ Przeciągnij główną świeczkę z ItemPickup

    // ✅ References
    private QuestManager _questManager;
    private int _litCount = 0;
    private bool _cutscenePlayed = false;
    private bool _cameraLockedOnDemon = false;
    private PlayerController _player;
    private Camera _mainCamera;
    private PlayerCam _playerCam;
    public GameObject[] bloods;

    // ✅ Do zarządzania dźwiękiem
    private FMOD.Studio.EventInstance _demonSoundInstance;

    // ✅ Camera backup
    private Transform _camOriginalParent;
    private Vector3 _camOriginalLocalPos;
    private Quaternion _camOriginalLocalRot;
    private bool _camWasEnabled;

    // ✅ PUBLICZNE GETTERY dla CandleController
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

        if (demonAnimator != null)
        {
            Debug.Log($"[AtticQuest] 🎭 Animator assigned | turn_on={animTurnOn}, point={animPoint}, jumpscare={animJumpscare}");
        }

        Debug.Log("[AtticQuest] ✅ Ready | Candles to light: " + candlesToLight);
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
            Debug.Log("[AtticQuest] ✅ All candles lit! Trigger enabled");
        }
    }

    public void StartCutscene()
    {
        if (_cutscenePlayed) return;
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        _cutscenePlayed = true;
        Debug.Log("[AtticQuest] 🎬 Cutscene STARTED – FIXED flow");

        // 🔒 Zablokuj gracza i kamerę
        if (_player != null) _player.enabled = false;
        // 🎥 Kamera na demona (pozycja startowa)
        if (_mainCamera != null && demonCutscenePosition != null)
        {
            _mainCamera.transform.SetParent(null, true);
            _mainCamera.transform.position = demonCutscenePosition.position + cameraOffset;
            _mainCamera.transform.LookAt(demonCutscenePosition);
            Debug.Log($"[Cutscene] 🎥 Camera locked on demon at {demonCutscenePosition.position}");

            // ✅ ZABLOKUJ KAMERĘ NA CZAS CUTSCENKI
            _cameraLockedOnDemon = true;

            // ✅ Wyłącz PlayerCam, żeby nie nadpisywał rotacji
            if (_playerCam != null)
                _playerCam.enabled = false;
        }

        // 🌑 Fade out na start
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);

        // 🎥 Kamera na demona (pozycja startowa)
        if (_mainCamera != null && demonCutscenePosition != null)
        {
            _mainCamera.transform.SetParent(null, true);
            _mainCamera.transform.position = demonCutscenePosition.position + cameraOffset;
            _mainCamera.transform.LookAt(demonCutscenePosition);
            Debug.Log($"[Cutscene] 🎥 Camera locked on demon at {demonCutscenePosition.position}");
        }

        // 🌕 Fade in – gracz widzi scenę
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeIn(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);

        // ─────────────────────────────────────────────
        // 🎬 KROK 1: Demon się pojawia + dźwięk (5 sekund)
        // ─────────────────────────────────────────────
        Debug.Log("[Cutscene] 👹 Step 1: Demon appear + sound (5s)");

        if (demonGameObject != null)
        {
            demonGameObject.SetActive(true);
            if (demonAnimator != null && !demonAnimator.enabled)
                demonAnimator.enabled = true;
        }

        if (!demonAppearSound.IsNull && AudioManager.Instance != null)
        {
            _demonSoundInstance = AudioManager.Instance.PlayDialogVoice(demonAppearSound);
            Debug.Log("[Cutscene] 🔊 demonAppearSound playing");
        }

        yield return new WaitForSeconds(5f);

        // ─────────────────────────────────────────────
        // 🎬 KROK 2: Demon wskazuje na gracza (3 sekundy)
        // ─────────────────────────────────────────────
        Debug.Log("[Cutscene] 👉 Step 2: Demon point animation (3s)");

        if (demonAnimator != null && !string.IsNullOrEmpty(animPoint))
        {
            demonAnimator.ResetTrigger(animTurnOn);
            demonAnimator.ResetTrigger(animJumpscare);
            demonAnimator.SetTrigger(animPoint);
            Debug.Log($"[Cutscene] 🎭 SetTrigger('{animPoint}') called");
            yield return new WaitForSeconds(3f);
        }
        else
        {
            Debug.LogWarning("[Cutscene] ⚠️ demonAnimator or animPoint not set – skipping point animation");
            yield return new WaitForSeconds(2f);
        }

        // ─────────────────────────────────────────────
        // 🎬 KROK 3: Świeczki gasną + demon znika + dźwięk stop (1.5 sekundy)
        // ─────────────────────────────────────────────
        Debug.Log("[Cutscene] 🕯️ Step 3: Candles off + demon hide + sound stop (1.5s)");

        var candles = FindObjectsOfType<CandleController>();
        foreach (var c in candles) if (c != null) c.SetLit(false);

        if (demonGameObject != null) demonGameObject.SetActive(false);

        if (_demonSoundInstance.isValid() && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopDialogVoice(ref _demonSoundInstance, true);
            Debug.Log("[Cutscene] 🔇 demonAppearSound stopped");
        }

        yield return new WaitForSeconds(1.5f);

        // ─────────────────────────────────────────────
        // 🎬 KROK 4: Świeczki się zapalają, demona nie ma (3 sekundy)
        // ─────────────────────────────────────────────
        Debug.Log("[Cutscene] 🕯️ Step 4: Candles on, demon hidden (3s)");
     
        foreach (var c in candles) if (c != null) c.SetLit(true);
        yield return new WaitForSeconds(3f);

        // ─────────────────────────────────────────────
        // 🎬 KROK 5a: Świeczki gasną → pauza → jumpscare prep (1.5 sekundy)
        // ─────────────────────────────────────────────
        Debug.Log("[Cutscene] 🕯️ Step 5a: Candles off + pause before jumpscare (1.5s)");

        foreach (var c in candles) if (c != null) c.SetLit(false);
        yield return new WaitForSeconds(1.5f);

        // ─────────────────────────────────────────────
        // 🎬 KROK 5b: DEMON JUMPSCARE – teleport + animacja + dźwięk (3 sekundy)
        // ─────────────────────────────────────────────
        Debug.Log("[Cutscene] 👹 Step 5b: JUMPSCARE! (3s)");

        if (demonGameObject != null)
        {
            demonGameObject.SetActive(true);

            if (demonJumpscarePosition != null)
            {
                demonGameObject.transform.position = demonJumpscarePosition.position;
                demonGameObject.transform.rotation = demonJumpscarePosition.rotation;
                Debug.Log($"[Cutscene] 🚀 Demon teleported to jumpscare pos: {demonJumpscarePosition.position}");
            }
            else if (_player != null)
            {
                demonGameObject.transform.position = _player.transform.position + _player.transform.forward * 2f;
                demonGameObject.transform.LookAt(_player.transform);
                Debug.Log("[Cutscene] 🚀 Demon teleported to fallback position");
            }

            if (demonAnimator != null && !string.IsNullOrEmpty(animJumpscare))
            {
                demonAnimator.ResetTrigger(animPoint);
                demonAnimator.SetTrigger(animJumpscare);
                Debug.Log($"[Cutscene] 🎭 SetTrigger('{animJumpscare}') called for jumpscare");
            }
        }

        if (!demonJumpscareSound.IsNull && AudioManager.Instance != null && _player != null)
        {
            AudioManager.Instance.PlaySFX(demonJumpscareSound, _player.transform.position);
            Debug.Log("[Cutscene] 🔊 demonJumpscareSound playing");
        }

        yield return new WaitForSeconds(3f);

        // ─────────────────────────────────────────────
        // 🎬 KROK 6: Fade out → zniszcz świeczki → powrót (BEZ FadeIn!)
        // ─────────────────────────────────────────────
        Debug.Log("[Cutscene] 🗑️ Step 6: Fade out + destroy candles + return");

        // 🌑 Fade out przed powrotem – i ZOSTAW EKRAN CZARNY
        if (screenFader != null)
            yield return StartCoroutine(screenFader.FadeOut(cutsceneFadeSpeed));
        else
            yield return new WaitForSeconds(cutsceneFadeSpeed);

        // ✅ Dłuższa pauza w czerni przed teleportem
        Debug.Log("[Cutscene] ⏱️ Holding fade out for 3s before return...");
        yield return new WaitForSeconds(3f);

        // 🗑️ ZNISZCZ WSZYSTKIE ŚWIECZKI Z SCENY
        foreach (var c in candles)
        {
            if (c != null && c.gameObject != null)
            {
                Destroy(c.gameObject);
                Debug.Log($"[AtticQuest] 🗑️ Destroyed scene candle: {c.gameObject.name}");
            }
        }

        // ✅ ZNISZCZ GŁÓWNĄ ŚWIECZKĘ PRZYPISANĄ W INSPECTORZE
        if (mainCandleToDestroy != null && mainCandleToDestroy.gameObject != null)
        {
            Destroy(mainCandleToDestroy.gameObject);
            Debug.Log($"[AtticQuest] 🗑️ Destroyed main candle: {mainCandleToDestroy.gameObject.name}");
        }

        Debug.Log("[AtticQuest] 🗑️ All candles destroyed");

        yield return new WaitForSeconds(delayBeforeReturn);
        GameState.IsInFlashback = false;

        // ✅ Quest ukończony
        if (!string.IsNullOrEmpty(questID) && _questManager != null)
        {
            _questManager.CompleteQuest(questID);
            Debug.Log($"[AtticQuest] ✅ Quest COMPLETED: {questID}");
        }


        _cameraLockedOnDemon = false;

        // ✅ Przywróć PlayerCam (ale nie włączaj jeszcze – zrobi to NarrativeInspectTrigger)
        if (_playerCam != null && _camWasEnabled)
        {
            _playerCam.enabled = true;
            _playerCam.SyncRotationWithCamera(); // ✅ Zsynczuj rotację po odblokowaniu
        }
        // 🔙 Powrót do mieszkania – FadeIn będzie w ReturnFromFlashbackSequence!
        if (linkedTrigger != null)
        {
            Debug.Log("[AtticQuest] 🔙 Triggering return to apartment");
            linkedTrigger.EndFlashback();
        }

        Debug.Log("[AtticQuest] 🎬 Cutscene FINISHED");
        // 🎥 Przywróć kamerę gracza (ale NIE rób FadeIn!)
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
            Debug.Log("[Cutscene] 🎥 PlayerCamera restored");
        }
    }
}