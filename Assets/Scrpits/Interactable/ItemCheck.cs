using FMODUnity;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ItemCheck : MonoBehaviour
{
    [Header("Check Settings")]
    public string itemName = "Object";
    public string playerThought = "You examine the object closely...";

    [Header("Demon Response (opcjonalnie)")]
    public string demonResponse = "";
    public EventReference demonVoiceEvent;

    [Header("UI")]

    public GameObject crossair;

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.07f;
    [SerializeField] private float playerThoughtStayTime = 1.2f;
    [SerializeField] private float delayBeforeDemon = 0.2f;

    [Header("Visual Styling")]
    [SerializeField] private string playerMarkerColor = "#00000080";
    [SerializeField] private string demonMarkerColor = "#FF000080";

    [Header("Outline")]
    public Outline outline;

    private Camera playerCamera;
    private bool canInteract = false;
    private bool isChecking = false;
    private FMOD.Studio.EventInstance demonVoiceInstance;


    void Start()
    {

        playerCamera = Camera.main;



        if (outline != null) outline.enabled = false;

        if (GameNarrativeManager.Instance == null)
            Debug.LogError("[ItemCheck] Brak GameNarrativeManager w scenie!");
    }

    void Update()
    {
        if (isChecking) return;

        CheckForInteraction();

        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(CheckItem());
        }
    }

    void CheckForInteraction()
    {
        // ✅ NOWY CHECK: Jeśli interakcje zablokowane → seizure
        if (GameState.InteractionsLocked)
        {
            if (outline != null) outline.enabled = false;
            crossair.SetActive(false);
            canInteract = false;

            // ✅ Użyj UNIKALNYCH nazw zmiennych – bez konfliktu!
            Ray seizureRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(seizureRay, out RaycastHit seizureHit, 3f)
                && (seizureHit.collider.gameObject == gameObject || seizureHit.transform.IsChildOf(transform)))
            {
                GameState.TriggerSeizureEffect = true;
            }
            return;
        }

        if (isChecking) return;

        bool wasInteracting = canInteract;
        canInteract = false;

        // ✅ Reszta metody BEZ ZMIAN – oryginalny kod:
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            if (hit.collider.gameObject == gameObject || hit.transform.IsChildOf(transform))
            {
                canInteract = true;
                if (outline != null) outline.enabled = true;
                crossair.SetActive(true);
            }
            else
            {
                if (outline != null) outline.enabled = false;
                crossair.SetActive(false);
            }
        }
        else
        {
            if (outline != null) outline.enabled = false;
            crossair.SetActive(false);
        }
    }




    IEnumerator CheckItem()
    {
        isChecking = true;


        if (outline != null) outline.enabled = false;
        crossair.SetActive(false);

        Debug.Log($"[ItemCheck] Rozpoczęto interakcję z: {itemName}");

        // === ETAP 1: Myśl gracza (przez GameNarrativeManager) ===
        if (GameNarrativeManager.Instance != null)
        {
            yield return StartCoroutine(GameNarrativeManager.Instance.ShowThoughtWithStyle(
                playerThought,
                typeSpeed,
                playerThoughtStayTime,
                playerMarkerColor
            ));
        }

        yield return new WaitForSeconds(delayBeforeDemon);

        // === ETAP 2: Odpowiedź demona (opcjonalnie) ===
        if (!string.IsNullOrWhiteSpace(demonResponse))
        {
            if (!demonVoiceEvent.IsNull && AudioManager.Instance != null)
            {
                demonVoiceInstance = AudioManager.Instance.PlayDialogVoice(demonVoiceEvent);
            }

            if (GameNarrativeManager.Instance != null)
            {
                yield return StartCoroutine(GameNarrativeManager.Instance.ShowThoughtWithStyle(
                    demonResponse,
                    typeSpeed,
                    playerThoughtStayTime,
                    demonMarkerColor
                ));
            }

            if (demonVoiceInstance.isValid())
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.StopDialogVoice(ref demonVoiceInstance, true);
            }
        }

        // ✅ QUEST: ukończ po sprawdzeniu talerza
        if (itemName == "Plate" && QuestManager.Instance != null)
        {
           
            if (GameNarrativeManager.Instance != null)
                GameNarrativeManager.Instance.TriggerFridgeDemon();
        }



        isChecking = false;
        Debug.Log($"[ItemCheck] Interakcja z {itemName} zakończona");
    }

    void OnDestroy()
    {
        if (demonVoiceInstance.isValid() && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopDialogVoice(ref demonVoiceInstance, true);
        }
    }
}