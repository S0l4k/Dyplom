using UnityEngine;
using System.Collections;
using FMODUnity;
using TMPro;
using UnityEngine.SceneManagement;

public class GameNarrativeManager : MonoBehaviour
{
    public static GameNarrativeManager Instance { get; private set; }
    private bool hasStartedNarrative = false;
    [Header("Hunger Sequence")]
    public EventReference stomachGrowl;
    public float delayAfterStart = 3.0f;

    [Header("Thought UI")]
    public TMP_Text thoughtText;
    [SerializeField] private string defaultMarkerColor = "#00000080";

    [Header("Quests")]
    public string fridgeQuest = "Check your fridge";
    public string orderFoodQuest = "Order Food";

    [Header("Fridge Demon")]
    public DemonRoomPresence demonPresence;
    private bool waitingForFridgeDemonDialog = false;

    [Header("Vomit Sequence")]
    public Transform bathroomSpawn;
    public ScreenFader screenFader;
    public EventReference vomitSound;
    public GameObject plate;

    [Header("Courier-Demon Exchange")]
    public Dialog dialogUI;
    public DialogActivator demonDialogActivator;
    public DialogActivator courierDialogActivator;
    public Transform stairsBottomSpawn;
    public EventReference gunshoot;
    public EventReference staircaseScream;
    public RoomTrigger roomTrigger;
    public GameObject triggers;
    
    public GameObject UICanvas;

    [Header("Dialog Lines (konfiguruj w Inspectorze)")]
    public DialogNode demonLine1;
    public DialogNode courierLine2;
    public DialogNode demonLine3;
    public DialogNode demonAfterShot;

    [Header("Second Ending")]
    public Transform couchCameraPosition;
    public Transform demonCouchPosition;
    public Animator demonAnimator;
    [Tooltip("Pozycja kamery po wstaniu z kanapy (powrót do normalnej gry)")]
    public Transform normalPlayerCameraPosition;

    [Tooltip("Punkt, w którym gracz staje przy oknie (trigger jumpscare)")]
    public Transform windowJumpscareStandPoint;

    [Tooltip("Pozycja demona przy jumpscare (ZA graczem)")]
    public Transform demonJumpscarePosition;
    [Header("Music")]
    public EventReference ambientMusic;
    public EventReference chaseMusic;
    public EventReference patrolMusic;
    public EventReference victoryMusic;
    public EventReference stairsMusic;
    [Header("One-Shot SFX")]
    [Tooltip("Dźwięk gdy demon pojawia się na dole schodów (po 5 pętlach)")]
    public EventReference demonStairsAppearSFX;
    public EventReference takingMeds;
    [Tooltip("Dźwięk gdy demon zaczyna gonić gracza (start chase)")]
    public EventReference demonChaseStartSFX;
    public EventReference policeSirenSFX;
    public EventReference demonJumpscareRevealSFX;
    [Tooltip("Dźwięk gdy demon zostaje pokonany (meds taken)")]
    public EventReference demonDefeatedSFX;
    [Header("Music Fade Times")]
    [Tooltip("Czas fade-in przy starcie gry (ambient)")]
    public float ambientFadeTime = 2f;

    [Tooltip("Czas fade-in przy rozpoczęciu pościgu")]
    public float chaseFadeTime = 0.5f;

    [Tooltip("Czas fade-in przy wejściu do mieszkania (patrol)")]
    public float patrolFadeTime = 1f;

    [Tooltip("Czas fade-in po pokonaniu demona (victory)")]
    public float victoryFadeTime = 1.5f;
    [Header("Apartment Exploration Quest")]
    [Tooltip("Quest ID dla 'rozglądnij się po mieszkaniu'")]
    public string explorationQuestID = "Look around the apartment";
    [Tooltip("Prompt gdy gracz nie wie co robić")]
    public string explorationHint = "I should look around the apartment while waiting...";
    [Tooltip("Dźwięk dzwonka do drzwi")]
    public EventReference doorbellSound;

    [Header("Second Ending - Extended")]
    [Tooltip("Demon pojawiający się w oknie (iluzja, osobny obiekt, domyślnie WYŁĄCZONY)")]
    public GameObject demonWindowObject;

    [Tooltip("Demon do finalnego jumpscare (stoi ZA graczem przy oknie, domyślnie WYŁĄCZONY)")]
    public GameObject demonFinalJumpscareObject;

    [Tooltip("Punkt, w który kamera ma spojrzeć przy oknie")]
    public Transform windowLookAtPoint;

    [Tooltip("Pozycja gracza przy oknie (gdzie ma 'podejść')")]
    public Transform playerAtWindowPosition;

    [Tooltip("Punkt, w który kamera ma spojrzeć przy jumpscare (ZA graczem)")]
    public Transform jumpscareLookAtPoint;

    [Header("Second Ending - SFX")]
    [Tooltip("Dźwięk gdy gracz zobaczy demona w oknie (szmer/szept/napięcie)")]
    public EventReference windowDemonRevealSFX;

    [Tooltip("JEDEN event FMOD: rozbite okno + upadek ciała (Timeline)")]
    public EventReference windowBreakAndFallSFX;

    private EventReference currentMusicEvent;

    [Header("Second Ending - Timing")]
    [Tooltip("Czas fade in/out przy siadaniu na kanapie")]
    public float couchFadeDuration = 1.5f;
    [Tooltip("Czas siedzenia na kanapie przed pojawieniem się iluzji")]
    public float couchSitTime = 6f;
    [Tooltip("Czas widoczności demona w oknie")]
    public float windowDemonVisibleTime = 4f;
    [Tooltip("Czas płynnego obrotu kamery w stronę okna")]
    public float cameraLookAtWindowDuration = 3f;
    [Tooltip("Czas patrzenia w okno po obrocie kamery")]
    public float lookAtWindowPause = 2.5f;
    [Tooltip("Czas 'chodzenia' gracza do okna")]
    public float walkToWindowDuration = 3f;
    [Tooltip("Pauza przy oknie przed jumpscare")]
    public float preJumpscarePause = 1.5f;
    [Tooltip("Czas widoczności jumpscare przed fade out")]
    public float jumpscareVisibleTime = 0.5f;
    [Tooltip("Czas fade out do czerni przy jumpscare")]
    public float jumpscareFadeDuration = 1f;
    [Tooltip("Czas czarnego ekranu z dźwiękami")]
    public float blackScreenDuration = 4f;
    public float postTurnRevealPause = 2f;
    [Tooltip("Czas powrotu kamery do normalnej pozycji po wstaniu z kanapy")]
    public float cameraReturnDuration = 1.5f;
    public Outline windowOutline;

    [Tooltip("Czas płynnego obrotu kamery na demona przy jumpscare")]
    public float cameraTurnToDemonDuration = 0.8f;

    private bool isNarrativeBusy = false;
    private bool isDisplayingText = false;

    private PlayerController playerController;
    private PlayerCam playerCam;
    private EnemyAI demon;
    public GameObject windowTrigger;
    public GameObject blood;
    public GameObject bloodStaircase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public void ChangeBackgroundMusic(EventReference newMusic, float fadeTime = 1f)
    {
        if (AudioManager.Instance == null || newMusic.IsNull) return;

        currentMusicEvent = newMusic;
        AudioManager.Instance.PlayMusic(newMusic, fadeTime);
    }
    public void PlayOneShotAtPlayer(EventReference sfx)
    {
        if (AudioManager.Instance == null || sfx.IsNull) return;

        Vector3? playerPos = null;
        if (playerController != null)
            playerPos = playerController.transform.position;

        AudioManager.Instance.PlaySFX(sfx, playerPos);
    }

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        playerCam = FindObjectOfType<PlayerCam>();
        demon = FindObjectOfType<EnemyAI>();
        windowOutline.enabled = false;
        StartCoroutine(StartNarrativeSequence());
        if (!ambientMusic.IsNull && AudioManager.Instance != null)
        {
            ChangeBackgroundMusic(ambientMusic, ambientFadeTime);
        }

    }
    private IEnumerator StartNarrativeSequence()
    {
        yield return new WaitForSeconds(delayAfterStart);

        if (playerController != null && !stomachGrowl.IsNull)
        {
            AudioManager.Instance.PlaySFX(stomachGrowl, playerController.transform.position);
        }

        yield return StartCoroutine(ShowThought("I'm hungry...", 0.09f, 2.0f));

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ClearAllQuests();
            QuestManager.Instance.AddQuest(fridgeQuest);
        }
    }

    public bool IsNarrativeBusy()
    {
        return isNarrativeBusy;
    }

    public IEnumerator ShowThoughtWithStyle(string text, float speed, float stayTime, string customMarkerColor)
    {
        if (!thoughtText)
        {
            yield break;
        }
        if (GameState.IsTalking || isDisplayingText)
            yield break;

        isDisplayingText = true;
        isNarrativeBusy = true;

        thoughtText.gameObject.SetActive(true);
        thoughtText.text = "";
        thoughtText.color = Color.white;

        string open = $"<mark={customMarkerColor}>";
        string close = "</mark>";

        for (int i = 0; i < text.Length; i++)
        {
            thoughtText.text = open + text.Substring(0, i + 1) + close;
            yield return new WaitForSeconds(speed);
        }

        yield return new WaitForSeconds(stayTime);

        Color startCol = thoughtText.color;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            thoughtText.color = Color.Lerp(startCol, new Color(startCol.r, startCol.g, startCol.b, 0), elapsed / 0.3f);
            yield return null;
        }

        thoughtText.gameObject.SetActive(false);

        isDisplayingText = false;
        isNarrativeBusy = false;
    }

    public IEnumerator ShowThought(string text, float speed, float stayTime)
    {
        yield return StartCoroutine(ShowThoughtWithStyle(text, speed, stayTime, defaultMarkerColor));
    }

    public void TriggerFridgeDemon()
    {
        if (demonPresence != null)
        {
            demonPresence.ForceAppear("fridge");
            waitingForFridgeDemonDialog = true;
        }
    }

    public void OnCourierInitialDialogComplete()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest("Meet with the courier downstairs");

        if (courierDialogActivator != null) courierDialogActivator.enabled = false;

        if (demonPresence != null)
        {
            demonPresence.ForceAppear("stairs_bottom");
        }
    }

    public void EnableCourierLine2()
    {
        if (demonDialogActivator != null) demonDialogActivator.enabled = false;

        if (courierDialogActivator != null)
        {
            courierDialogActivator.dialogNodes = new DialogNode[] { courierLine2 };
            courierDialogActivator.enabled = true;
        }
    }

    public void EnableDemonLine3()
    {
        if (courierDialogActivator != null) courierDialogActivator.enabled = false;

        if (demonDialogActivator != null)
        {
            demonDialogActivator.dialogNodes = new DialogNode[] { demonLine3 };
            demonDialogActivator.enabled = true;
        }
    }

    public void EndCourierDemonSequence()
    {
        if (demonDialogActivator != null) demonDialogActivator.enabled = false;
        if (courierDialogActivator != null) courierDialogActivator.enabled = false;

        if (demonPresence != null)
        {
            demonPresence.ExitRoom();
        }
        else
        {
            EnemyAI demonRef = FindObjectOfType<EnemyAI>();
            if (demonRef != null)
            {
                foreach (var r in demonRef.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    r.enabled = false;
            }
        }

        if (dialogUI != null) dialogUI.gameObject.SetActive(false);
    }

    public void OnPlayerAcceptsFood()
    {
        if (playerController == null || playerCam == null)
        {
            return;
        }
        plate.SetActive(false);
        playerController.enabled = false;
        playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        GameState.FridgeDemonDialogCompleted = true;
        CompleteFridgeQuest();
        StartCoroutine(VomitSequence());
    }

    public void OnPlayerRefusesFood()
    {
        GameState.FridgeDemonDialogCompleted = true;
        if (QuestManager.Instance != null)
        {
            CompleteFridgeQuest();
            QuestManager.Instance.AddQuest(orderFoodQuest);
        }
    }

    private IEnumerator VomitSequence()
    {
        if (screenFader == null)
        {
            RestorePlayerControl();
            yield break;
        }

        yield return StartCoroutine(screenFader.FadeOut(0.8f));

        if (bathroomSpawn != null)
        {
            CharacterController cc = playerController.GetComponent<CharacterController>();
            Vector3 targetPos = bathroomSpawn.position;
            Quaternion targetRot = bathroomSpawn.rotation;
            playerController.enabled = false;
            playerCam.enabled = false;

            if (Physics.Raycast(bathroomSpawn.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, LayerMask.GetMask("Default", "Floor", "Environment")))
            {
                targetPos = hit.point + Vector3.up * (cc ? cc.height * 0.5f : 1f) - Vector3.up * 1f;
            }

            playerController.transform.position = targetPos;
            playerController.transform.rotation = targetRot;

            if (playerCam != null) playerCam.SyncRotationWithCamera();
        }

        yield return new WaitForSeconds(1f);
        if (!vomitSound.IsNull) AudioManager.Instance.PlaySFX(vomitSound, playerController.transform.position);
        yield return new WaitForSeconds(6f);
        yield return StartCoroutine(screenFader.FadeIn(1f));

        RestorePlayerControl();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest(orderFoodQuest);
        }
    }
    public void UnlockApartmentExploration()
    {
        GameState.ApartmentExplorationUnlocked = true;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest(explorationQuestID);
        }

        if (!string.IsNullOrEmpty(explorationHint))
        {
            StartCoroutine(ShowThought(explorationHint, 0.05f, 3f));
        }
    }
    public void OnFlashbackCompleted(string questID)
    {
        GameState.FlashbacksCompleted++;

        if (GameState.AllFlashbacksCompleted)
        {
            StartCoroutine(TriggerDoorbellSequence());
            GameState.CourierArrived = true;
        }
    }
    private IEnumerator TriggerDoorbellSequence()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(explorationQuestID);
        }

        if (!doorbellSound.IsNull && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(doorbellSound);
        }

        yield return new WaitForSeconds(1f);

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest("Meet with the courier downstairs");
        }

        yield return StartCoroutine(ShowThought("Someone's at the door...", 0.05f, 2.5f));
    }
    public void PlayerAcceptedOffer()
    {

        QuestManager.Instance?.ClearAllQuests();
        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        StartCoroutine(KillSequenceSecondEnding(() => {
           
        }));
    }

    public IEnumerator KillSequenceSecondEnding(System.Action onDialogComplete = null)
    {
        if (screenFader == null)
        {
            RestorePlayerControl();
            yield break;
        }

        yield return StartCoroutine(screenFader.FadeOut(0.8f));
        UICanvas.SetActive(false);
        yield return new WaitForSeconds(6f);

        if (!gunshoot.IsNull && playerController != null)
            AudioManager.Instance.PlaySFX(gunshoot, playerController.transform.position);
        bloodStaircase.SetActive(true);
        yield return new WaitForSeconds(2.2f);
        UICanvas.SetActive(true);
        yield return StartCoroutine(screenFader.FadeIn(1f));

        RestorePlayerControl();

        if (demonDialogActivator != null && demonAfterShot != null)
        {
            demonDialogActivator.dialogNodes = new DialogNode[] { demonAfterShot };
            demonDialogActivator.enabled = true;

            float timeout = 180f;
            float elapsed = 0f;

            while (demonDialogActivator.isTalking && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);

            if (onDialogComplete != null)
            {
                onDialogComplete.Invoke();
            }
        }
        else
        {
            if (onDialogComplete != null) onDialogComplete.Invoke();
        }
    }

    public void FirstEnding()
    {
        StartCoroutine(FirstEndingCooperateSequence());
    }
    public IEnumerator FirstEndingCooperateSequence()
    {

        ChangeBackgroundMusic(victoryMusic, victoryFadeTime);

        if (screenFader == null) { Debug.LogError("[Ending1] screenFader NULL!"); yield break; }
        Transform playerCamera = Camera.main.transform;

        yield return StartCoroutine(screenFader.FadeOut(couchFadeDuration));
        UICanvas.SetActive(false);

        if (couchCameraPosition != null)
        {
            playerCamera.position = couchCameraPosition.position;
            playerCamera.rotation = couchCameraPosition.rotation;
        }

        if (demon != null && demonCouchPosition != null && demonAnimator != null)
        {
            demon.gameObject.SetActive(true);
            demon.transform.position = demonCouchPosition.position;
            demon.transform.rotation = demonCouchPosition.rotation;
            demonAnimator.Rebind();
            demonAnimator.Update(0f);
            yield return null;
            demonAnimator.SetTrigger("sit_couch");
        }

        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return StartCoroutine(screenFader.FadeIn(couchFadeDuration));

        yield return new WaitForSeconds(12f);

        if (!policeSirenSFX.IsNull)
        {
            AudioManager.Instance.PlaySFX(policeSirenSFX, playerCamera.position);
        }

        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(screenFader.FadeOut(2.5f)); 
        yield return new WaitForSeconds(2f); 

        EndingSaveManager.SaveEnding(EndingSaveManager.EndingType.Cooperate);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }


    public void StartSecondEndingFinalSequence(Vector3 returnCamPos, Quaternion returnCamRot)
    {

        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(SecondEndingFinalSequence(returnCamPos, returnCamRot));
    }

    private IEnumerator SecondEndingFinalSequence(Vector3 returnCamPos, Quaternion returnCamRot)
    {
        
        ChangeBackgroundMusic(victoryMusic, victoryFadeTime);
        if (screenFader == null)
        { yield break; }

        Transform playerCamera = Camera.main.transform;

        yield return StartCoroutine(screenFader.FadeOut(couchFadeDuration));
        UICanvas.SetActive(false);

        if (couchCameraPosition != null)
        {
            playerCamera.position = couchCameraPosition.position;
            playerCamera.rotation = couchCameraPosition.rotation;
        }

        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return StartCoroutine(screenFader.FadeIn(couchFadeDuration));
        yield return new WaitForSeconds(couchSitTime);

        if (demonWindowObject != null)
        {
            demonWindowObject.SetActive(true);
        }
        if (!windowDemonRevealSFX.IsNull)
            AudioManager.Instance.PlaySFX(windowDemonRevealSFX, windowLookAtPoint?.position ?? playerCamera.position);

        yield return new WaitForSeconds(windowDemonVisibleTime);

        if (windowLookAtPoint != null)
            yield return StartCoroutine(SmoothLookAt(playerCamera, windowLookAtPoint, cameraLookAtWindowDuration));

        yield return new WaitForSeconds(lookAtWindowPause);

        if (demonWindowObject != null)
            demonWindowObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        Vector3 camStartPos = playerCamera.position;
        Quaternion camStartRot = playerCamera.rotation;
        float elapsed = 0f;
        float returnDuration = cameraReturnDuration;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            playerCamera.position = Vector3.Lerp(camStartPos, returnCamPos, t);
            playerCamera.rotation = Quaternion.Slerp(camStartRot, returnCamRot, t);
            yield return null;
        }

        playerCamera.position = returnCamPos;
        playerCamera.rotation = returnCamRot;

        if (playerController != null) playerController.enabled = true;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        windowTrigger.SetActive(true);
        windowOutline.enabled = true;
    }


    public IEnumerator SmoothLookAt(Transform source, Transform target, float duration)
    {

        Quaternion startRot = source.rotation;
        Quaternion targetRot = Quaternion.LookRotation(target.position - source.position, Vector3.up);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            yield return null;
        }
        source.rotation = targetRot;

    }

    public void PlayerRefuseddOffer()
    {

        GameState.DemonInStoryMode = false;
        GameState.ChaseLocked = true;

        if (QuestManager.Instance != null)
            QuestManager.Instance.AddQuest("Go back to your flat");

        EnemyAI demonRef = FindObjectOfType<EnemyAI>();
        if (demonRef != null && demonRef.ai != null)
        {
            demonRef.ai.enabled = true;
        }

        if (demonPresence != null) demonPresence.ExitRoom();
        if (triggers != null) triggers.SetActive(false);

        if (!staircaseScream.IsNull && playerController != null)
        {
            AudioManager.Instance.PlaySFX(staircaseScream, playerController.transform.position);
        }
        ChangeBackgroundMusic(stairsMusic);

        GameState.LoopSequenceActive = true;
        blood.SetActive(true);
    }
    private void CompleteFridgeQuest()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest("Check your fridge");
        }

        waitingForFridgeDemonDialog = false;
    }

    private void RestorePlayerControl()
    {
        if (playerController != null) playerController.enabled = true;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

}