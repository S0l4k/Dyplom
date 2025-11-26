using System.Collections;
using UnityEngine;
using TMPro;

public class DialogActivator : MonoBehaviour
{
    public string npcName = "NPC";
    public TMP_Text interactionText;
    public GameObject player;
    private MonoBehaviour playerMovementScript;
    private Camera playerCamera;
    private bool canTalk = false;
    private bool isTalking = false;
    [SerializeField] private GameObject dialogManager;

    void Start()
    {
        playerCamera = Camera.main;
        if (interactionText == null) interactionText = FindObjectOfType<TMP_Text>();
        if (interactionText != null) interactionText.gameObject.SetActive(false);
        if (player != null) playerMovementScript = player.GetComponent<MonoBehaviour>();
    }

    void Update()
    {
        if (!playerCamera || interactionText == null) return;

        CheckForNPC();

        
        if (canTalk && Input.GetKeyDown(KeyCode.E) && !isTalking && !IsEnemyChasing())
        {
            StartConversation();
        }

        
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

        if (Physics.Raycast(ray, out hit, talkRange) && hit.collider.gameObject == gameObject)
        {
            ShowInteractionText();
            canTalk = true;
        }
        else
        {
            HideInteractionText();
            canTalk = false;
        }
    }

    bool IsEnemyChasing()
    {
        EnemyAI ai = GetComponent<EnemyAI>();
        return ai != null && ai.chasing;
    }

    void ShowInteractionText()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(true);
            interactionText.text = $"Press E to talk with {npcName}";
        }
    }

    void HideInteractionText()
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    void StartConversation()
    {
        HideInteractionText();
        isTalking = true;
        GameState.IsTalking = true; 

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

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

        Debug.Log($"Rozmowa z {npcName} zakoñczona.");
    }
}
