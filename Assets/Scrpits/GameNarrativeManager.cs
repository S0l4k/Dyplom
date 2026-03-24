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
    [SerializeField] private string markerColor = "#00000080";

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

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        playerCam = FindObjectOfType<PlayerCam>();
        demon = FindObjectOfType<EnemyAI>();
        StartCoroutine(StartNarrativeSequence());
        roomTrigger = GetComponent<RoomTrigger>();
    }

    private IEnumerator StartNarrativeSequence()
    {
        yield return new WaitForSeconds(delayAfterStart);

        if (playerController != null && !stomachGrowl.IsNull)
        {
            RuntimeManager.PlayOneShot(stomachGrowl, playerController.transform.position);
        }

        yield return StartCoroutine(ShowThought("I'm hungry...", 0.09f, 2.0f));

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ClearAllQuests();
            QuestManager.Instance.AddQuest(fridgeQuest);
        }

        Debug.Log("[Narrative] ➡️ Quest \"Check your fridge\" active");
    }

    public IEnumerator ShowThought(string text, float speed, float stayTime)
    {
        if (!thoughtText)
        {
            Debug.LogError("[Narrative] thoughtText nie przypisany!");
            yield break;
        }

        thoughtText.gameObject.SetActive(true);
        thoughtText.text = "";
        thoughtText.color = Color.white;

        string open = $"<mark={markerColor}>";
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
        QuestManager.Instance.CompleteQuest("Meet with the courier downstairs");
        Debug.Log("[Narrative] 📦 Kurier: dialog początkowy zakończony – pojawia się demon");

        if (courierDialogActivator != null)
            courierDialogActivator.enabled = false;

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

        if (demonDialogActivator != null)
            demonDialogActivator.enabled = false;

        if (courierDialogActivator != null)
        {
            courierDialogActivator.dialogNodes = new DialogNode[] { courierLine2 };
            courierDialogActivator.enabled = true;
        }
    }

    public void EnableDemonLine3()
    {
        Debug.Log("[Narrative] 📦 Kurier: odpowiedział – demon mówi ostatnią linię");

        if (courierDialogActivator != null)
            courierDialogActivator.enabled = false;

        if (demonDialogActivator != null)
        {
            demonDialogActivator.dialogNodes = new DialogNode[] { demonLine3 };
            demonDialogActivator.enabled = true;
        }
    }

    public void EndCourierDemonSequence()
    {
        
        Debug.Log("[Narrative] 🔚 Sekwencja dialogowa demon↔kurier zakończona");

        if (demonDialogActivator != null)
            demonDialogActivator.enabled = false;

        if (courierDialogActivator != null)
            courierDialogActivator.enabled = false;

        if (demonPresence != null)
        {
            demonPresence.ExitRoom();
        }
        else
        {
            EnemyAI demon = FindObjectOfType<EnemyAI>();
            if (demon != null)
            {
                foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    r.enabled = false;
            }
        }

        if (dialogUI != null)
            dialogUI.gameObject.SetActive(false);
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
                targetPos = hit.point + Vector3.up * (cc ? cc.height * 0.5f : 1f);
            }

            playerController.transform.position = targetPos;
            playerController.transform.rotation = targetRot;

            if (playerCam != null)
                playerCam.SyncRotationWithCamera();
        }

        yield return new WaitForSeconds(1f);
        if (!vomitSound.IsNull)
            RuntimeManager.PlayOneShot(vomitSound, playerController.transform.position);

        yield return new WaitForSeconds(6f);

        yield return StartCoroutine(screenFader.FadeIn(1f));

        RestorePlayerControl();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest(orderFoodQuest);
            Debug.Log("[Narrative] ✅ Quest \"Order Food\" aktywowany po rzyganiu");
        }
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
            RuntimeManager.PlayOneShot(gunshoot, playerController.transform.position);

        yield return new WaitForSeconds(2.2f);
        UICanvas.SetActive(true);

        yield return StartCoroutine(screenFader.FadeIn(1f));

        RestorePlayerControl();

        if (demonDialogActivator != null && demonAfterShot != null)
        {
            demonDialogActivator.dialogNodes = new DialogNode[] { demonAfterShot };
            demonDialogActivator.enabled = true;
            Debug.Log("[Narrative] ✅ Dialog po zastrzeleniu aktywowany – demon stoi na miejscu");
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
        if (screenFader == null)
        {
            Debug.LogError("[Narrative] screenFader NIE PRZYPISANY!");
            yield break;
        }

        Transform playerCamera = Camera.main.transform;
        Vector3 originalCamPos = playerCamera.position;
        Quaternion originalCamRot = playerCamera.rotation;

        yield return StartCoroutine(screenFader.FadeOut(1.2f));
        UICanvas.SetActive(false);

        if (couchCameraPosition != null)
        {
            playerCamera.position = couchCameraPosition.position;
            playerCamera.rotation = couchCameraPosition.rotation;
            Debug.Log($"[Narrative] 📷 Kamera teleportowana na pozycję: {couchCameraPosition.position}");
        }
        else
        {
            Debug.LogError("[Narrative] ❌ couchCameraPosition NULL – kamera nie została przeniesiona!");
        }

        if (demon != null && demonCouchPosition != null)
        {
            demon.transform.position = demonCouchPosition.position;
            demon.transform.rotation = demonCouchPosition.rotation;
            Debug.Log($"[Narrative] 👹 Demon teleportowany na pozycję: {demonCouchPosition.position}");

            if (demonAnimator != null)
            {
                demonAnimator.Rebind();
                demonAnimator.Update(0f);
                yield return null;
                demonAnimator.SetTrigger("sit_couch");
                Debug.Log("[Narrative] ✅ Animacja sit_couch aktywowana");
            }
        }

        if (playerController != null)
            playerController.enabled = false;

        if (playerCam != null)
            playerCam.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return StartCoroutine(screenFader.FadeIn(1.2f));

        Debug.Log("[Narrative] 👁️ Gracz może teraz ruszać kamerą – 4 sekundy na obejrzenie demona");
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
        QuestManager.Instance.AddQuest("Go back to your flat");
        EnemyAI demon = FindObjectOfType<EnemyAI>();
        if (demon != null && demon.ai != null)
        {
            demon.ai.enabled = true;
            Debug.Log("[Narrative] ✅ NavMeshAgent włączony dla demona");
        }

        if (demonPresence != null)
        {
            demonPresence.ExitRoom();
        }

        triggers.SetActive(false);

        if (!staircaseScream.IsNull && playerController != null)
        {
            RuntimeManager.PlayOneShot(staircaseScream, playerController.transform.position);
            Debug.Log("[Narrative] 🔊 Krzyk demona w tle");
        }

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