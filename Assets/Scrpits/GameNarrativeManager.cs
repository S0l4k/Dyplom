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
    // ✅ NOWE: One-shot SFX dla kluczowych momentów
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
    // ✅ NOWE: Osobne czasy fade-in dla każdej muzyki
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

    // ✅ NOWA FLAGA: Blokuje inne myśli podczas ważnych sekwencji (np. ItemCheck)
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
        // ✅ Singleton tylko w ramach sceny – bez DontDestroyOnLoad!
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ❌ USUNIĘTE: DontDestroyOnLoad(gameObject);

        Debug.Log("[GameNarrativeManager] ✅ Initialized (scene-bound)");
    }

    /// <summary>
    /// Zmienia muzykę w tle przez AudioManager.
    /// </summary>
    public void ChangeBackgroundMusic(EventReference newMusic, float fadeTime = 1f)
    {
        if (AudioManager.Instance == null || newMusic.IsNull) return;

        currentMusicEvent = newMusic;
        AudioManager.Instance.PlayMusic(newMusic, fadeTime);
        Debug.Log($"[Narrative] 🎵 Changed music to: {newMusic.Guid}");
    }
    public void PlayOneShotAtPlayer(EventReference sfx)
    {
        if (AudioManager.Instance == null || sfx.IsNull) return;

        Vector3? playerPos = null;
        if (playerController != null)
            playerPos = playerController.transform.position;

        AudioManager.Instance.PlaySFX(sfx, playerPos);
        Debug.Log($"[Narrative] 🔊 One-shot: {sfx.Guid}");
    }

    private void Start()
    {
        // ✅ Znajdź referencje przy każdym starcie sceny
        playerController = FindObjectOfType<PlayerController>();
        playerCam = FindObjectOfType<PlayerCam>();
        demon = FindObjectOfType<EnemyAI>();
        windowOutline.enabled = false;
        // ✅ Uruchom sekwencję narracyjną – zawsze przy starcie sceny
        StartCoroutine(StartNarrativeSequence());

        // ✅ Uruchom muzykę ambient z fade-in
        if (!ambientMusic.IsNull && AudioManager.Instance != null)
        {
            ChangeBackgroundMusic(ambientMusic, ambientFadeTime);
        }

        Debug.Log("[GameNarrativeManager] ▶️ StartNarrativeSequence triggered");
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

        Debug.Log("[Narrative] ➡️ Quest \"Check your fridge\" active");
    }

    // ✅ METODA PUBLICZNA: Sprawdzenie czy system jest zajęty
    public bool IsNarrativeBusy()
    {
        return isNarrativeBusy;
    }

    // ✅ GŁÓWNA METODA: Obsługuje stylowanie i blokadę
    public IEnumerator ShowThoughtWithStyle(string text, float speed, float stayTime, string customMarkerColor)
    {
        if (!thoughtText)
        {
            Debug.LogError("[Narrative] thoughtText nie przypisany!");
            yield break;
        }
        if (GameState.IsTalking || isDisplayingText)
            yield break;

        isDisplayingText = true;
        // BLOKADA: Oznaczamy system jako zajęty
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

        // Fade out
        Color startCol = thoughtText.color;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            thoughtText.color = Color.Lerp(startCol, new Color(startCol.r, startCol.g, startCol.b, 0), elapsed / 0.3f);
            yield return null;
        }

        thoughtText.gameObject.SetActive(false);

        isDisplayingText = false;  // ✅ ZWOLNIJ miejsce
        isNarrativeBusy = false;
    }

    // ✅ PRZECIĄŻENIE: Dla starych wywołań bez koloru (używa domyślnego)
    public IEnumerator ShowThought(string text, float speed, float stayTime)
    {
        yield return StartCoroutine(ShowThoughtWithStyle(text, speed, stayTime, defaultMarkerColor));
    }

    public void TriggerFridgeDemon()
    {
        if (demonPresence != null)
        {
            demonPresence.ForceAppear("fridge");

            // ✅ ZACZNIJ CZEKAĆ na koniec dialogu z demonem
            waitingForFridgeDemonDialog = true;

            Debug.Log("[Narrative] 👹 Demon appears at fridge – waiting for dialog completion");
        }
        else
        {
            Debug.LogError("[Narrative] demonPresence NIE PRZYPISANY!");
        }
    }

    public void OnCourierInitialDialogComplete()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest("Meet with the courier downstairs");

        Debug.Log("[Narrative] 📦 Kurier: dialog początkowy zakończony – pojawia się demon");

        if (courierDialogActivator != null) courierDialogActivator.enabled = false;

        if (demonPresence != null)
        {
            demonPresence.ForceAppear("stairs_bottom");
        }
        else
        {
            Debug.LogError("[Narrative] demonPresence nie przypisany!");
        }
    }

    public void EnableCourierLine2()
    {
        Debug.Log("[Narrative] 👹 Demon: gracz zapytał 'why?' – kurier odpowiada");
        if (demonDialogActivator != null) demonDialogActivator.enabled = false;

        if (courierDialogActivator != null)
        {
            courierDialogActivator.dialogNodes = new DialogNode[] { courierLine2 };
            courierDialogActivator.enabled = true;
        }
    }

    public void EnableDemonLine3()
    {
        Debug.Log("[Narrative] 📦 Kurier: odpowiedział – demon mówi ostatnią linię");
        if (courierDialogActivator != null) courierDialogActivator.enabled = false;

        if (demonDialogActivator != null)
        {
            demonDialogActivator.dialogNodes = new DialogNode[] { demonLine3 };
            demonDialogActivator.enabled = true;
        }
    }

    public void EndCourierDemonSequence()
    {
        Debug.Log("[Narrative] 🔚 Sekwencja dialogowa demon↔kurier zakończona");
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
        Debug.Log("[Narrative] Gracz zgadza się zjeść – rozpoczynam sekwencję rzygania");
        if (playerController == null || playerCam == null)
        {
            Debug.LogError("[Narrative] Brak PlayerController lub PlayerCam!");
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
        Debug.Log("[Narrative] Gracz odmówił jedzenia");
        if (QuestManager.Instance != null)
        {
            CompleteFridgeQuest();
            QuestManager.Instance.AddQuest(orderFoodQuest);
            Debug.Log("[Narrative] ✅ Quest \"Order Food\" aktywowany");
        }
    }

    private IEnumerator VomitSequence()
    {
        if (screenFader == null)
        {
            Debug.LogError("[Narrative] screenFader NIE PRZYPISANY!");
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
            Debug.Log("[Narrative] ✅ Quest \"Order Food\" aktywowany po rzyganiu");
        }
    }
    public void UnlockApartmentExploration()
    {
        Debug.Log("[Narrative] 🔓 Unlocking apartment exploration quest");

        GameState.ApartmentExplorationUnlocked = true;

        if (QuestManager.Instance != null)
        {
            // ✅ Dodaj quest "rozglądnij się"
            QuestManager.Instance.AddQuest(explorationQuestID);
            Debug.Log($"[Narrative] ➕ Quest added: {explorationQuestID}");
        }

        // ✅ Opcjonalnie: pokaż podpowiedź od razu
        if (!string.IsNullOrEmpty(explorationHint))
        {
            StartCoroutine(ShowThought(explorationHint, 0.05f, 3f));
        }
    }
    public void OnFlashbackCompleted(string questID)
    {
        Debug.Log($"[Narrative] 🎯 Flashback completed: {questID}");

        GameState.FlashbacksCompleted++;
        Debug.Log($"[Narrative] 📊 Progress: {GameState.FlashbacksCompleted}/{GameState.TotalFlashbacksRequired}");

        // ✅ Sprawdź czy wszystkie flashbacki są ukończone
        if (GameState.AllFlashbacksCompleted)
        {
            Debug.Log("[Narrative] ✅ All flashbacks completed! Triggering doorbell...");
            StartCoroutine(TriggerDoorbellSequence());
            GameState.CourierArrived = true;
        }
    }
    private IEnumerator TriggerDoorbellSequence()
    {
        // ✅ Ukończ quest "rozglądnij się"
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest(explorationQuestID);
            Debug.Log($"[Narrative] ✅ Quest completed: {explorationQuestID}");
        }

        // 🔊 Odtwórz dźwięk dzwonka
        if (!doorbellSound.IsNull && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(doorbellSound);
            Debug.Log("[Narrative] 🔔 Doorbell sound played");
        }

        // ⏱️ Krótka pauza na reakcję gracza
        yield return new WaitForSeconds(1f);

        // ✅ Dodaj nowy quest: spotkaj się z kurierem
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest("Meet with the courier downstairs");
            Debug.Log("[Narrative] ➕ Quest added: Meet with the courier downstairs");
        }

        // 💭 Pokaż myśl dla gracza
        yield return StartCoroutine(ShowThought("Someone's at the door...", 0.05f, 2.5f));
    }
    public void PlayerAcceptedOffer()
    {
        Debug.Log("[Narrative] 🤝 Gracz zaakceptował ofertę demona – Ending 1 (Cooperate)");

        // 🔒 Zablokuj kontrolę
        QuestManager.Instance?.ClearAllQuests();
        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        // 🎬 Uruchom kill sequence, a PO DIALOGU przejdź do kanapy
        StartCoroutine(KillSequenceSecondEnding(() => {
            Debug.Log("[Narrative] 🔁 Callback: starting couch sequence");
           
        }));
    }

    /// <summary>
    /// Sekwencja strzału + dialog z demonem.
    /// Po skończonym dialogu wywołuje onDialogComplete (jeśli podane).
    /// </summary>
    public IEnumerator KillSequenceSecondEnding(System.Action onDialogComplete = null)
    {
        if (screenFader == null)
        {
            Debug.LogError("[Narrative] screenFader NIE PRZYPISANY!");
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
            Debug.Log("[Narrative] ✅ Dialog po zastrzeleniu aktywowany");

            // ✅ NOWE: Czekaj na FAKTYCZNE zakończenie dialogu
            Debug.Log("[Narrative] ⏳ Waiting for post-shot dialog to finish...");

            float timeout = 180f; // ✅ Bezpiecznik: max 60 sekund czekania
            float elapsed = 0f;

            // Czekaj aż dialog się skończy LUB timeout
            while (demonDialogActivator.isTalking && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Dodatkowe opóźnienie, żeby gracz mógł "przetrawić" ostatnią linię
            yield return new WaitForSeconds(1.5f);

            Debug.Log($"[Narrative] ✅ Post-shot dialog finished (waited: {elapsed:F1}s)");

            // ✅ Wywołaj callback (przejście do kanapy) TYLKO po dialogu
            if (onDialogComplete != null)
            {
                Debug.Log("[Narrative] 🔁 Calling couch sequence callback");
                onDialogComplete.Invoke();
            }
        }
        else
        {
            Debug.LogError("[Narrative] ❌ demonDialogActivator lub demonAfterShot NULL");
            // Jeśli nie ma dialogu, i tak wywołaj callback (żeby nie zablokować endingu)
            if (onDialogComplete != null) onDialogComplete.Invoke();
        }
    }

    public void FirstEnding()
    {
        StartCoroutine(FirstEndingCooperateSequence());
    }
    public IEnumerator FirstEndingCooperateSequence()
    {
        Debug.Log("[Ending1] 🎬 Starting cooperate ending cutscene");

        ChangeBackgroundMusic(victoryMusic, victoryFadeTime);

        if (screenFader == null) { Debug.LogError("[Ending1] screenFader NULL!"); yield break; }
        Transform playerCamera = Camera.main.transform;

        // === FAZA 1: Fade out + teleport na kanapę ===
        yield return StartCoroutine(screenFader.FadeOut(couchFadeDuration));
        UICanvas.SetActive(false);

        // Kamera na pozycję kanapy
        if (couchCameraPosition != null)
        {
            playerCamera.position = couchCameraPosition.position;
            playerCamera.rotation = couchCameraPosition.rotation;
        }

        // Demon na pozycję kanapy (siedzi obok gracza)
        if (demon != null && demonCouchPosition != null && demonAnimator != null)
        {
            demon.gameObject.SetActive(true);
            demon.transform.position = demonCouchPosition.position;
            demon.transform.rotation = demonCouchPosition.rotation;

            // Reset i trigger animacji siedzenia
            demonAnimator.Rebind();
            demonAnimator.Update(0f);
            yield return null;
            demonAnimator.SetTrigger("sit_couch");
            Debug.Log("[Ending1] 👹 Demon seated on couch");
        }

        // 🔒 Blokada kontroli
        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // === FAZA 2: Fade in + siedzenie razem ===
        yield return StartCoroutine(screenFader.FadeIn(couchFadeDuration));

        // ✅ PRZEDŁUŻONE: Siedzenie razem (cisza/napięcie) – z 5s na 12s
        yield return new WaitForSeconds(12f);

        // === FAZA 3: Policyjne syreny w tle ===
        Debug.Log("[Ending1] 🚨 Playing police sirens");
        if (!policeSirenSFX.IsNull)
        {
            AudioManager.Instance.PlaySFX(policeSirenSFX, playerCamera.position);
        }

        // ✅ PRZEDŁUŻONE: Pauza na "wejście" syren w klimat – z 2s na 5s
        yield return new WaitForSeconds(5f);

        // === FAZA 4: Fade out + exit ===
        yield return StartCoroutine(screenFader.FadeOut(2.5f)); // ✅ Nieco wolniejszy fade
        yield return new WaitForSeconds(2f); // ✅ Dłuższa pauza przed exit

        // 💾 ZAPISZ Ending 1 przed wyjściem
        EndingSaveManager.SaveEnding(EndingSaveManager.EndingType.Cooperate);
        Debug.Log("[Ending1] 💾 Saved Cooperate ending");

        // 🔚 Wyjście z gry
        Debug.Log("[Ending1] 🔚 Exiting game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    // ✅ ZAMIENIENIE: public void zamiast IEnumerator
    // ✅ Przyjmuje pozycję i rotację kamery do powrotu
    public void StartSecondEndingFinalSequence(Vector3 returnCamPos, Quaternion returnCamRot)
    {
        Debug.Log("[Narrative] 🎬 Rozpoczynam finał drugiego zakończenia");

        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ✅ Przekaż pozycje do korutiny
        StartCoroutine(SecondEndingFinalSequence(returnCamPos, returnCamRot));
    }

    // ✅ Prywatna korutina z właściwą logiką
    private IEnumerator SecondEndingFinalSequence(Vector3 returnCamPos, Quaternion returnCamRot)
    {
        
        ChangeBackgroundMusic(victoryMusic, victoryFadeTime);
        if (screenFader == null) { Debug.LogError("[Narrative] screenFader NIE PRZYPISANY!"); yield break; }

        Transform playerCamera = Camera.main.transform;

        // === FAZA 1: Gracz SAM siedzi na kanapie ===
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

        // === FAZA 2: Demon w oknie (iluzja) ===
        if (demonWindowObject != null)
        {
            demonWindowObject.SetActive(true);
            Debug.Log("[Ending2] 👹 Iluzja demona w oknie AKTYWNA");
        }
        if (!windowDemonRevealSFX.IsNull)
            AudioManager.Instance.PlaySFX(windowDemonRevealSFX, windowLookAtPoint?.position ?? playerCamera.position);

        yield return new WaitForSeconds(windowDemonVisibleTime);

        // === FAZA 3: Kamera PŁYNNIE do okna ===
        if (windowLookAtPoint != null)
            yield return StartCoroutine(SmoothLookAt(playerCamera, windowLookAtPoint, cameraLookAtWindowDuration));

        yield return new WaitForSeconds(lookAtWindowPause);

        // === FAZA 4: Iluzja znika ===
        if (demonWindowObject != null)
            demonWindowObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        // === FAZA 5: Gracz WSTAJE + POWRÓT KAMERY + aktywacja triggera ===
        Debug.Log("[Ending2] 🔄 Restoring player control & camera position");

        // 🔄 Kamera wraca do ZAPISANEJ pozycji i rotacji (jak w ComputerInteract)
        Vector3 camStartPos = playerCamera.position;
        Quaternion camStartRot = playerCamera.rotation;
        float elapsed = 0f;
        float returnDuration = cameraReturnDuration; // np. 1.5s

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;

            // ✅ Interpoluj POZYCJĘ i ROTACJĘ (nie tylko rotację!)
            playerCamera.position = Vector3.Lerp(camStartPos, returnCamPos, t);
            playerCamera.rotation = Quaternion.Slerp(camStartRot, returnCamRot, t);
            yield return null;
        }

        // Upewnij się, że kamera jest dokładnie w celu
        playerCamera.position = returnCamPos;
        playerCamera.rotation = returnCamRot;

        // Przywróć kontrolę gracza
        if (playerController != null) playerController.enabled = true;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 🔓 Aktywuj collider jumpscare przy oknie
        windowTrigger.SetActive(true);
        windowOutline.enabled = true;

        Debug.Log("[Ending2] ✅ Player control restored - camera returned to original position");
        // ✅ KONIEC – reszta w WindowJumpscareTrigger
    }

    /// <summary>
    /// Płynnie obraca transform w kierunku targetu przez określony czas.
    /// </summary>
    public IEnumerator SmoothLookAt(Transform source, Transform target, float duration)
    {
        Debug.Log($"[SmoothLookAt] START | From: {source.name} | To: {target.name} | Duration: {duration}s");

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

        Debug.Log($"[SmoothLookAt] DONE | Final rot: {source.rotation.eulerAngles}");
    }

    public void PlayerRefuseddOffer()
    {
        Debug.Log("[Narrative] 👹 Demon: gracz odmówił zabicia – demon znika");

        GameState.DemonInStoryMode = false;
        GameState.ChaseLocked = true;

        if (QuestManager.Instance != null)
            QuestManager.Instance.AddQuest("Go back to your flat");

        EnemyAI demonRef = FindObjectOfType<EnemyAI>();
        if (demonRef != null && demonRef.ai != null)
        {
            demonRef.ai.enabled = true;
            Debug.Log("[Narrative] ✅ NavMeshAgent włączony dla demona");
        }

        if (demonPresence != null) demonPresence.ExitRoom();
        if (triggers != null) triggers.SetActive(false);

        if (!staircaseScream.IsNull && playerController != null)
        {
            AudioManager.Instance.PlaySFX(staircaseScream, playerController.transform.position);
            Debug.Log("[Narrative] 🔊 Krzyk demona w tle");
        }
        ChangeBackgroundMusic(stairsMusic);

        GameState.LoopSequenceActive = true;
        Debug.Log("[Narrative] 🔁 Stair loop aktywowany");
        blood.SetActive(true);
    }
    private void CompleteFridgeQuest()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest("Check your fridge");
            Debug.Log("[Narrative] ✅ Quest completed: Check your fridge (after demon dialog)");
        }

        // ✅ Reset flagi – quest już zakończony
        waitingForFridgeDemonDialog = false;
    }

    private void RestorePlayerControl()
    {
        if (playerController != null) playerController.enabled = true;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    // === 🧪 DEBUG SKIP - tylko w Editorze ===
#if UNITY_EDITOR
    [Header("Debug")]
    [Tooltip("DEBUG: Pozycja, do której teleportować gracza po F12 (opcjonalnie)")]
    public Transform debugTeleportPosition;

    [Tooltip("DEBUG: Reference do SofaInteract (przeciągnij w Inspectorze)")]
    public SofaInteract debugSofaInteractReference;


   
#endif
}