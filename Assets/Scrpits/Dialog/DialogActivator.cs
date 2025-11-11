using System.Collections;
using UnityEngine;
using TMPro;

public class DialogActivator : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "NPC";

    [Header("UI")]
    public TMP_Text interactionText;

    [Header("Player Settings")]
    public GameObject player;
    private MonoBehaviour playerMovementScript;

    private Camera playerCamera;
    private bool canTalk = false;
    private static DialogActivator currentTarget = null;
    private bool isTalking = false;

    [SerializeField]
    private GameObject dialogManager;

    void Start()
    {
        playerCamera = Camera.main;

        if (interactionText == null)
            interactionText = FindObjectOfType<TMP_Text>();

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        if (player != null)
        {
            playerMovementScript = player.GetComponent<MonoBehaviour>();
        }
    }

    void Update()
    {
        if (isTalking)
            return;

        CheckForNPC();

        if (canTalk && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }
    }

    void CheckForNPC()
    {
        if (!playerCamera || interactionText == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        float talkRange = 3f;

        if (Physics.Raycast(ray, out hit, talkRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                currentTarget = this;
                ShowInteractionText();
                canTalk = true;
                return;
            }
        }

        if (currentTarget == this)
        {
            HideInteractionText();
            currentTarget = null;
        }

        canTalk = false;
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
            else
            {
                Debug.LogWarning("DialogManager nie ma przypisanego komponentu Dialog!");
            }
        }
        else
        {
            Debug.LogWarning("Brak przypisanego dialogManager w inspektorze!");
        }
    }

    IEnumerator WaitForDialogEnd(Dialog dialog)
    {
        while (dialog.gameObject.activeSelf)
        {
            yield return null;
        }

        isTalking = false;
        Debug.Log($"Rozmowa z {npcName} zakoñczona.");

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;
    }
}