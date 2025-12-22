using System.Collections;
using UnityEngine;
using TMPro;

public class DialogActivator : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "NPC";

    [Header("UI")]
    [Tooltip("TEXT TYLKO do interakcji z NPC (Press E to talk)")]
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

        // 🔴 TWARDY BEZPIECZNIK – bez przypisanego textu nie jedziemy dalej
        if (interactionText == null)
        {
            Debug.LogError("[DialogActivator] interactionText NIE JEST PRZYPISANY w Inspectorze!");
            enabled = false;
            return;
        }

        interactionText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Jeśli dialog trwa → absolutnie nic nie rób
        if (isTalking)
        {
            HideInteractionText();
            return;
        }

        if (!playerCamera) return;

        CheckForNPC();

        if (canTalk && Input.GetKeyDown(KeyCode.E) && !IsEnemyChasing())
        {
            StartConversation();
        }

        // Debug / awaryjne odblokowanie chase
        if (GameState.ChaseLocked && Input.GetKeyDown(KeyCode.X))
        {
            GameState.ChaseLocked = false;
            Debug.Log("Chase unlocked!");
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
        // 🔥 KLUCZOWE – gasimy text NATYCHMIAST
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
    }
}
