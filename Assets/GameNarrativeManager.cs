using UnityEngine;
using System.Collections;
using FMODUnity;
using TMPro;

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
    public Dialog dialogUI;                    // Canvas → DialogPanel
    public DialogActivator demonDialogActivator;  // Demon GameObject z DialogActivator
    public DialogActivator courierDialogActivator; // Drzwi GameObject z DialogActivator
    public Transform stairsBottomSpawn;        // Pusty GameObject przy dole schodów

    // ✅ DIALOGI ZDEFINIOWANE BEZPOŚREDNIO W SKRYPCIE (bez ScriptableObjects!)
    [Header("Dialog Lines (konfiguruj w Inspectorze)")]
    public DialogNode demonLine1;
    public DialogNode courierLine2;
    public DialogNode demonLine3;

    private PlayerController playerController;
    private PlayerCam playerCam;

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
        StartCoroutine(StartNarrativeSequence());
    }

    private IEnumerator StartNarrativeSequence()
    {
        yield return new WaitForSeconds(delayAfterStart);

        // 🔊 BRZUCH
        if (playerController != null && !stomachGrowl.IsNull)
        {
            RuntimeManager.PlayOneShot(stomachGrowl, playerController.transform.position);
        }

        // 💭 "I'm hungry..."
        yield return StartCoroutine(ShowThought("I'm hungry...", 0.09f, 2.0f));

        // 📜 QUEST
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ClearAllQuests();
            QuestManager.Instance.AddQuest(fridgeQuest);
        }

        Debug.Log("[Narrative] ➡️ Quest \"Check your fridge\" active");
    }

    private IEnumerator ShowThought(string text, float speed, float stayTime)
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
    }

    // ✅ WYWOŁYWANE Z ItemCheck PO SPRAWDZENIU TALERZA
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

    // ✅ WYWOŁYWANE Z DialogNode KURIERA (responseEvents po dialogu początkowym)
    public void OnCourierInitialDialogComplete()
    {
        Debug.Log("[Narrative] 📦 Kurier: dialog początkowy zakończony – pojawia się demon");

        // ✅ 1. UKRYJ KURIERA
        if (courierDialogActivator != null)
            courierDialogActivator.enabled = false;

        // ✅ 2. USTAW DIALOG DEMONA NA LINE 1 W JEGO ROOM PRESENCE DANYCH
        // (Twój DemonRoomPresence zrobi to automatycznie w ShowAfterDelay)
        // ✅ 3. POJAW DEMON PRZY SCHODACH – użyje dialogNodes z room presence "stairs_bottom"
        if (demonPresence != null)
        {
            demonPresence.ForceAppear("stairs_bottom");
        }
        else
        {
            Debug.LogError("[Narrative] demonPresence nie przypisany!");
        }
    }

    // ✅ WYWOŁYWANE Z DemonLine1 → response "Why?"
    public void EnableCourierLine2()
    {
        Debug.Log("[Narrative] 👹 Demon: gracz zapytał 'why?' – kurier odpowiada");

        // ✅ 1. UKRYJ DEMONA (ale fizycznie zostaje przy schodach)
        if (demonDialogActivator != null)
            demonDialogActivator.enabled = false;

        // ✅ 2. USTAW DIALOG KURIERA NA LINE 2 I WŁĄCZ GO
        if (courierDialogActivator != null)
        {
            courierDialogActivator.dialogNodes = new DialogNode[] { courierLine2 };
            courierDialogActivator.enabled = true;
        }
    }

    // ✅ WYWOŁYWANE Z CourierLine2 → obie odpowiedzi
    public void EnableDemonLine3()
    {
        Debug.Log("[Narrative] 📦 Kurier: odpowiedział – demon mówi ostatnią linię");

        // ✅ 1. UKRYJ KURIERA
        if (courierDialogActivator != null)
            courierDialogActivator.enabled = false;

        // ✅ 2. USTAW DIALOG DEMONA NA LINE 3 I WŁĄCZ GO (demon jest już przy schodach!)
        if (demonDialogActivator != null)
        {
            demonDialogActivator.dialogNodes = new DialogNode[] { demonLine3 };
            demonDialogActivator.enabled = true;
        }
    }

    // ✅ WYWOŁYWANE Z DemonLine1 ("Shut up") LUB DemonLine3 (obie odpowiedzi)
    public void EndCourierDemonSequence()
    {
        Debug.Log("[Narrative] 🔚 Sekwencja dialogowa demon↔kurier zakończona");

        // ✅ 1. UKRYJ OBA DIALOGI
        if (demonDialogActivator != null)
            demonDialogActivator.enabled = false;

        if (courierDialogActivator != null)
            courierDialogActivator.enabled = false;

        // ✅ 2. UKRYJ DEMONA WIZUALNIE
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

        // ✅ 3. UKRYJ UI DIALOGU
        if (dialogUI != null)
            dialogUI.gameObject.SetActive(false);
    }

    // ✅ WYWOŁYWANE Z DialogActivator PO WYBORZE "Eat it"
    public void OnPlayerAcceptsFood()
    {
        Debug.Log("[Narrative] Gracz zgadza się zjeść – rozpoczynam sekwencję rzygania");

        if (playerController == null || playerCam == null)
        {
            Debug.LogError("[Narrative] Brak PlayerController lub PlayerCam!");
            return;
        }

        // ✅ BLOKUJ KONTROLĘ
        playerController.enabled = false;
        playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(VomitSequence());
    }

    // ✅ WYWOŁYWANE Z DialogActivator PO WYBORZE "No"
    public void OnPlayerRefusesFood()
    {
        Debug.Log("[Narrative] Gracz odmówił jedzenia");

        // 📜 DODAJ QUEST (bez sekwencji rzygania)
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

        // ✅ FADE OUT
        yield return StartCoroutine(screenFader.FadeOut(0.8f));

        // ✅ TELEPORT DO ŁAZIENKI
        if (bathroomSpawn != null)
        {
            CharacterController cc = playerController.GetComponent<CharacterController>();
            Vector3 targetPos = bathroomSpawn.position;
            Quaternion targetRot = bathroomSpawn.rotation;

            // 🔍 Szukaj podłogi
            if (Physics.Raycast(bathroomSpawn.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, LayerMask.GetMask("Default", "Floor", "Environment")))
            {
                targetPos = hit.point + Vector3.up * (cc ? cc.height * 0.5f : 1f);
            }

            if (cc != null)
            {
                cc.enabled = false;
                playerController.transform.position = targetPos;
                playerController.transform.rotation = targetRot;
                cc.enabled = true;
            }
            else
            {
                playerController.transform.position = targetPos;
                playerController.transform.rotation = targetRot;
            }

            if (playerCam != null)
                playerCam.SyncRotationWithCamera();
        }

        // 🔊 RZYGANIE
        yield return new WaitForSeconds(0.3f);
        if (!vomitSound.IsNull)
            RuntimeManager.PlayOneShot(vomitSound, playerController.transform.position);

        yield return new WaitForSeconds(2.2f);

        // ✅ FADE IN
        yield return StartCoroutine(screenFader.FadeIn(1f));

        RestorePlayerControl();

        // 📜 DODAJ QUEST PO SEKWENCJI
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest(orderFoodQuest);
            Debug.Log("[Narrative] ✅ Quest \"Order Food\" aktywowany po rzyganiu");
        }
    }

    private void RestorePlayerControl()
    {
        if (playerController != null) playerController.enabled = true;
        if (playerCam != null) playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
