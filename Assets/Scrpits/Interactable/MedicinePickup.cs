using UnityEngine;
using TMPro;
using System.Collections;

public class MedicinePickup : MonoBehaviour
{
    [Header("References")]
    public LightController lightController;
    public EnemyAI demon;
    public GameObject pickupText;
    public GameObject sofaInteractObject;
    public Outline outline;
    private Camera playerCamera;
    private bool canInteract = false;

    void Start()
    {
        playerCamera = Camera.main;
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
        if (outline != null) outline.enabled = false;
    }

    void Update()
    {
        if (GameState.InteractionsLocked)
        {
            canInteract = false;
            if (outline != null) outline.enabled = false; 
            if (pickupText != null) pickupText.gameObject.SetActive(false);
            return;
        }

        if (pickupText == null || playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f) && hit.collider.gameObject == gameObject)
        {
            canInteract = true;
            pickupText.gameObject.SetActive(true);
            if (outline != null) outline.enabled = true;

            if (Input.GetKeyDown(KeyCode.E))
            {
                PickupMedicine();
            }
        }
        else
        {
            canInteract = false;
            if (outline != null) outline.enabled = false;
            pickupText.gameObject.SetActive(false);
        }
    }

    void PickupMedicine()
    {
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
        QuestManager.Instance.ClearAllQuests();
        QuestManager.Instance.AddQuest("Rest on Sofa");
        GameNarrativeManager.Instance.PlayOneShotAtPlayer(GameNarrativeManager.Instance.takingMeds);
        GameNarrativeManager.Instance.blood.SetActive(false);
    

        // ✅ Wyłącz demona
        if (demon != null)
        {
            DisableDemonSafely(demon);
        }

        if (lightController != null)
        {
            lightController.RestoreLights();
        }
 
        StartCoroutine(StartEndingSequence());
       
    }


    private IEnumerator StartEndingSequence()
    {
        yield return new WaitForSeconds(1.5f);

        if (GameNarrativeManager.Instance != null)
        {
            GameNarrativeManager.Instance.ChangeBackgroundMusic(
               GameNarrativeManager.Instance.victoryMusic,
               GameNarrativeManager.Instance.victoryFadeTime  
            );
            GameNarrativeManager.Instance.PlayOneShotAtPlayer(GameNarrativeManager.Instance.demonDefeatedSFX );
            

            if (GameNarrativeManager.Instance.thoughtText != null)
            {
                yield return GameNarrativeManager.Instance.StartCoroutine(
                    GameNarrativeManager.Instance.ShowThought("He is gone...", 0.09f, 2.5f)
                ); 
            }

        }
        GameState.InteractionsLocked = true;

        if (sofaInteractObject != null)
        {
            sofaInteractObject.SetActive(true);
        }
    }

    private void DisableDemonSafely(EnemyAI demon)
    {
        if (demon.ai != null)
        {
            demon.ai.isStopped = true;
            demon.ai.speed = 0f;
            demon.ai.enabled = false;
        }
        if (demon.GetComponent<EnemyAI>() is EnemyAI ai)
        {
            ai.StopHeartbeat();
        }

        demon.chasing = false;
        demon.walking = false;
        demon.playerInSight = false;

        foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            if (r != null) r.enabled = false;

        demon.aiAnim?.SetTrigger("idle");

        foreach (var col in demon.GetComponents<Collider>())
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        foreach (var col in demon.GetComponentsInChildren<Collider>(true))
        {
            if (col != null && col.gameObject != demon.gameObject) 
            {
                col.enabled = false;
            }
        }

        GameState.DemonRespawnedInApartment = false;
        GameState.FinalChase = false;
        GameState.ChaseLocked = true;
    }
}