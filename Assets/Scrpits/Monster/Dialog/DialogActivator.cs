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
    public EventReference voiceEvent;          // ✅ Głos tego NPC (demon LUB kurier)
    public Color textColor = Color.white;      // ✅ Kolor tekstu NPC
    // ✅ Kolor markera (tło pod tekstem)

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

    // ✅ ZAPAMIĘTAJ ORYGINALNE USTAWIENIA DIALOG UI (do przywrócenia po dialogu)
    private EventReference originalVoice;
    private Color originalTextColor = Color.white;
   

    void Start()
    {
        playerCamera = Camera.main;

        if (playerCamera != null)
            playerCamScript = playerCamera.GetComponent<PlayerCam>();

        if (player != null)
            playerMovementScript = player.GetComponent<MonoBehaviour>();




        // ✅ ZAPISZ ORYGINALNE USTAWIENIA DIALOG UI
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

        // ✅ BLOKADA DIALOGU KURIERA PRZED ZAMÓWIENIEM
        if (npcName == "Courier" && !GameState.CourierArrived)
        {
           
            canTalk = false;
            return;
        }

        // ✅ BLOKADA FINALNEGO DIALOGU
        if (isFinalDialog && !GameState.DemonLoopPhase)
        {
           
            canTalk = false;
            return;
        }

        // ✅ BLOKADA ZWYKŁYCH DIALOGÓW PODCZAS FAZY DEMONA
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
                // ✅ PODMIEŃ USTAWIENIA DIALOGU NA TE Z TEGO NPC
                dialog.npcVoiceEvent = voiceEvent;  // 🔥 TO BYŁO BRAKUJĄCE!

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

        // ✅ PRZYWRÓĆ ORYGINALNE USTAWIENIA DIALOG UI
        dialog.npcVoiceEvent = originalVoice;
       

        if (dialog.dialogText != null)
            dialog.dialogText.color = originalTextColor;

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        if (playerCamScript != null)
            playerCamScript.enabled = true;

        _dialogJustFinished = true;
        Debug.Log($"Rozmowa z {npcName} zakończona.");

        if (isFinalDialog)
        {
            GameState.ReadyForFinalChase = true;
            GameState.DemonLoopPhase = false;
            Debug.Log("[DialogActivator] Final dialog finished. ReadyForFinalChase = true");
        }
    }
    // Flag: czy dialog właśnie się skończył
    private bool _dialogJustFinished = false;

    // ✅ Publiczna metoda dla SchoolQuestController
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