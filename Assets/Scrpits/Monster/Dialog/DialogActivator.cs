using System.Collections;
using UnityEngine;
using TMPro;

public class DialogActivator : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "NPC";

    [Header("Dialog Settings")]
    [Tooltip("Jeśli true, dialog jest dostępny TYLKO gdy DemonLoopPhase = true (po 5 loopach)")]
    public bool isFinalDialog = false; // ✅ Będzie ustawiane programowo z StairLoop!

    [Header("UI")]
    public TMP_Text interactionText;

    [Header("References")]
    public GameObject player;
    [SerializeField] private GameObject dialogManager;

    private Camera playerCamera;
    private PlayerCam playerCamScript;
    private MonoBehaviour playerMovementScript;

    private bool canTalk = false;
    private bool isTalking = false;

    void Start()
    {
        playerCamera = Camera.main;

        if (playerCamera != null)
            playerCamScript = playerCamera.GetComponent<PlayerCam>();

        if (player != null)
            playerMovementScript = player.GetComponent<MonoBehaviour>();

        if (interactionText == null)
        {
            Debug.LogError("[DialogActivator] interactionText NIE JEST PRZYPISANY!");
            enabled = false;
            return;
        }

        interactionText.gameObject.SetActive(false);
        Debug.Log($"[DialogActivator] {npcName} initialized | isFinalDialog={isFinalDialog}");
    }

    void Update()
    {
        if (isTalking)
        {
            HideInteractionText();
            return;
        }

        if (!playerCamera) return;

        // ✅ NOWA LOGIKA: dialog zablokowany TYLKO jeśli:
        // - To finalny dialog ALE DemonLoopPhase = false (jeszcze nie respawnowany)
        // - To zwykły dialog ALE DemonLoopPhase = true (faza demona aktywna)
        if (isFinalDialog && !GameState.DemonLoopPhase)
        {
            // Czekamy na respawnowanie demona
            HideInteractionText();
            canTalk = false;
            return;
        }

        if (!isFinalDialog && GameState.DemonLoopPhase && !GameState.ReadyForFinalChase)
        {
            // Zwykłe NPC zablokowane podczas fazy demona (ale nie po dialogu)
            HideInteractionText();
            canTalk = false;
            return;
        }

        CheckForNPC();

        if (canTalk && Input.GetKeyDown(KeyCode.E) && !IsEnemyChasing())
        {
            StartConversation();
        }
    }

    void CheckForNPC()
    {
        if (IsEnemyChasing())
        {
            HideInteractionText();
            canTalk = false;
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        float talkRange = 3f;

        if (Physics.Raycast(ray, out hit, talkRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                ShowInteractionText();
                canTalk = true;
                return;
            }
        }

        HideInteractionText();
        canTalk = false;
    }

    bool IsEnemyChasing()
    {
        EnemyAI ai = GetComponent<EnemyAI>();
        return ai != null && ai.chasing;
    }

    void ShowInteractionText()
    {
        interactionText.gameObject.SetActive(true);
        interactionText.text = $"Press E to talk with {npcName}";
    }

    void HideInteractionText()
    {
        if (interactionText.gameObject.activeSelf)
            interactionText.gameObject.SetActive(false);
    }

    void StartConversation()
    {
        HideInteractionText();
        isTalking = true;
        GameState.IsTalking = true;

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        if (playerCamScript != null)
            playerCamScript.enabled = false;

        if (dialogManager != null)
        {
            dialogManager.SetActive(true);
            Dialog dialog = dialogManager.GetComponent<Dialog>();
            if (dialog != null)
            {
                dialog.StartDialog();
                StartCoroutine(WaitForDialogEnd(dialog));
            }
        }
    }

    IEnumerator WaitForDialogEnd(Dialog dialog)
    {
        while (dialog.gameObject.activeSelf)
            yield return null;

        isTalking = false;
        GameState.IsTalking = false;

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        if (playerCamScript != null)
            playerCamScript.enabled = true;

        Debug.Log($"Rozmowa z {npcName} zakończona.");

        if (isFinalDialog)
        {
            GameState.ReadyForFinalChase = true;
            GameState.DemonLoopPhase = false;
            Debug.Log("[DialogActivator] Final dialog finished. ReadyForFinalChase = true");
        }
    }

    // ✅ NOWA METODA: aktywacja finalnego dialogu PROGRAMOWO

}