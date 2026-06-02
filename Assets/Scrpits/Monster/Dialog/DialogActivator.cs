using System.Collections;
using UnityEngine;
using TMPro;
using FMODUnity;

public class DialogActivator : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "NPC";

    [Header("Dialog Settings")]
    [Tooltip("Jeśli true, dialog dostępny TYLKO gdy DemonLoopPhase = true (po 5 loopach)")]
    public bool isFinalDialog = false;

    [Header("Dialog Customization (per NPC)")]
    public EventReference voiceEvent;         
    public Color textColor = Color.white;     

    [Header("Dialog Content")]
    public DialogNode[] dialogNodes;

    [Header("UI")]
    


    [Header("References")]
    public GameObject player;
    [SerializeField] private GameObject dialogManager;

    private Camera playerCamera;
    private PlayerCam playerCamScript;
    private MonoBehaviour playerMovementScript;
    private bool canTalk = false;
    public bool isTalking = false;
    private EventReference originalVoice;
    private Color originalTextColor = Color.white;
   

    void Start()
    {
        playerCamera = Camera.main;

        if (playerCamera != null)
            playerCamScript = playerCamera.GetComponent<PlayerCam>();

        if (player != null)
            playerMovementScript = player.GetComponent<MonoBehaviour>();

        if (dialogManager != null)
        {
            Dialog dialog = dialogManager.GetComponent<Dialog>();
            if (dialog != null)
            {
                originalVoice = dialog.npcVoiceEvent;
                originalTextColor = dialog.dialogText != null ? dialog.dialogText.color : Color.white;
                
            }
        }
    }

    void Update()
    {
        if (isTalking)
        {
           
            return;
        }

        if (!playerCamera) return;

        if (npcName == "Courier" && !GameState.CourierArrived)
        {
           
            canTalk = false;
            return;
        }

        if (isFinalDialog && !GameState.DemonLoopPhase)
        {
           
            canTalk = false;
            return;
        }

        if (!isFinalDialog && GameState.DemonLoopPhase && !GameState.ReadyForFinalChase)
        {
           
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
               
                canTalk = true;
                return;
            }
        }

 
        canTalk = false;
    }

    bool IsEnemyChasing()
    {
        EnemyAI ai = GetComponent<EnemyAI>();
        return ai != null && ai.chasing;
    }



    void StartConversation()
    {
    
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
                dialog.npcVoiceEvent = voiceEvent;  

                if (dialog.dialogText != null)
                    dialog.dialogText.color = textColor;

                dialog.StartDialog(dialogNodes);
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

        dialog.npcVoiceEvent = originalVoice;
       

        if (dialog.dialogText != null)
            dialog.dialogText.color = originalTextColor;

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        if (playerCamScript != null)
            playerCamScript.enabled = true;

        _dialogJustFinished = true;

        if (isFinalDialog)
        {
            GameState.ReadyForFinalChase = true;
            GameState.DemonLoopPhase = false;
        }
    }
    private bool _dialogJustFinished = false;
    public bool HasDialogJustFinished()
    {
        if (_dialogJustFinished)
        {
            _dialogJustFinished = false;
            return true;
        }
        return false;
    }

}