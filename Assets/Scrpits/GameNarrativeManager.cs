using UnityEngine;
using System.Collections;
using FMODUnity;
using TMPro;
using UnityEngine.SceneManagement;

public class GameNarrativeManager : MonoBehaviour
{
    public static GameNarrativeManager Instance { get; private set; }

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

    [Header("Vomit Sequence")]
    public Transform bathroomSpawn;
    public ScreenFader screenFader;
    public EventReference vomitSound;

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

    private EventReference currentMusicEvent;
  

    // ✅ NOWA FLAGA: Blokuje inne myśli podczas ważnych sekwencji (np. ItemCheck)
    private bool isNarrativeBusy = false;

    private PlayerController playerController;
    private PlayerCam playerCam;
    private EnemyAI demon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        playerController = FindObjectOfType<PlayerController>();
        playerCam = FindObjectOfType<PlayerCam>();
        demon = FindObjectOfType<EnemyAI>();
        StartCoroutine(StartNarrativeSequence());

        // ✅ START: uruchom domyślną muzykę z ustawionym fade-time
        if (!ambientMusic.IsNull)
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

        // ODBLOKOWANIE: Sekwencja zakończona
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
            Debug.Log("[Narrative] 👹 Demon pojawia się obok lodówki");
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

        playerController.enabled = false;
        playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        StartCoroutine(VomitSequence());
    }

    public void OnPlayerRefusesFood()
    {
        Debug.Log("[Narrative] Gracz odmówił jedzenia");
        if (QuestManager.Instance != null)
        {
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
        Debug.Log("[Narrative] 🔫 Gracz zgodził się zabić kuriera – sekwencja wystrzału");
        QuestManager.Instance.ClearAllQuests();
        playerController.enabled = false;
        playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        StartCoroutine(KillSequenceSecondEnding());
    }

    private IEnumerator KillSequenceSecondEnding()
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

        yield return new WaitForSeconds(2.2f);
        UICanvas.SetActive(true);
        yield return StartCoroutine(screenFader.FadeIn(1f));

        RestorePlayerControl();

        if (demonDialogActivator != null && demonAfterShot != null)
        {
            demonDialogActivator.dialogNodes = new DialogNode[] { demonAfterShot };
            demonDialogActivator.enabled = true;
            Debug.Log("[Narrative] ✅ Dialog po zastrzeleniu aktywowany");
        }
        else
        {
            Debug.LogError("[Narrative] ❌ demonDialogActivator lub demonAfterShot NULL");
        }
    }

    public void StartSecondEndingFinalSequence()
    {
        Debug.Log("[Narrative] 🎬 Rozpoczynam finał drugiego zakończenia");
        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(SecondEndingFinalSequence());
    }

    private IEnumerator SecondEndingFinalSequence()
    {
        ChangeBackgroundMusic(victoryMusic, victoryFadeTime);
        if (screenFader == null)
        {
            Debug.LogError("[Narrative] screenFader NIE PRZYPISANY!");
            yield break;
        }

        Transform playerCamera = Camera.main.transform;

        yield return StartCoroutine(screenFader.FadeOut(1.2f));
        UICanvas.SetActive(false);

        if (couchCameraPosition != null)
        {
            playerCamera.position = couchCameraPosition.position;
            playerCamera.rotation = couchCameraPosition.rotation;
        }

        if (demon != null && demonCouchPosition != null)
        {
            demon.transform.position = demonCouchPosition.position;
            demon.transform.rotation = demonCouchPosition.rotation;

            if (demonAnimator != null)
            {
                demonAnimator.Rebind();
                demonAnimator.Update(0f);
                yield return null;
                demonAnimator.SetTrigger("sit_couch");
            }
        }

        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return StartCoroutine(screenFader.FadeIn(1.2f));
        yield return new WaitForSeconds(4f);
        yield return StartCoroutine(screenFader.FadeOut(1.5f));
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("MainMenu");
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
    }

    private void RestorePlayerControl()
    {
        if (playerController != null) playerController.enabled = true;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}