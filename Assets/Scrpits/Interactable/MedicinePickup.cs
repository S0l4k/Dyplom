using UnityEngine;
using TMPro;

public class MedicinePickup : MonoBehaviour
{
    [Header("References")]
    public LightController lightController;    // ✅ Twój LightController
    public EnemyAI demon;                      // ✅ Demon w mieszkaniu

    [Header("UI")]
    public TMP_Text pickupText;               // ✅ "Press E to take medicine"
         // ✅ Dźwięk podniesienia

    private Camera playerCamera;
    private bool canInteract = false;

    void Start()
    {
        playerCamera = Camera.main;
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (pickupText == null || playerCamera == null) return;

        // ✅ Raycast do interakcji
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f) && hit.collider.gameObject == gameObject)
        {
            canInteract = true;
            pickupText.gameObject.SetActive(true);
            pickupText.text = "Press E to take the medicine";

            if (Input.GetKeyDown(KeyCode.E))
            {
                PickupMedicine();
            }
        }
        else
        {
            canInteract = false;
            pickupText.gameObject.SetActive(false);
        }
    }

    void PickupMedicine()
    {
        // ✅ Dźwięk
       

        // ✅ Wyłącz demona (wizualnie + logicznie)
        if (demon != null)
        {
            DisableDemonSafely(demon);
            Debug.Log("[MedicinePickup] 👹 Demon wyłączony po podniesieniu leków");
        }

        // ✅ Włącz światła z płynnym przejściem
        if (lightController != null)
        {
            lightController.RestoreLights();
            Debug.Log("[MedicinePickup] 💡 Światła przywrócone do normy");
        }

        // ✅ Zniszcz obiekt leków
        Destroy(gameObject);
    }

    private void DisableDemonSafely(EnemyAI demon)
    {
        // ✅ Wyłącz NavMeshAgent (zatrzymaj ruch)
        if (demon.ai != null)
        {
            demon.ai.isStopped = true;
            demon.ai.speed = 0f;
            demon.ai.enabled = false; // ✅ Wyłącz agenta
        }

        // ✅ Wyłącz chasing/walking
        demon.chasing = false;
        demon.walking = false;
        demon.playerInSight = false;

        // ✅ Wyłącz renderery (ukryj wizualnie)
        foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            if (r != null) r.enabled = false;

        // ✅ Animacja idle
        demon.aiAnim?.SetTrigger("idle");

        // ✅ Reset globalnych stanów (demon nie będzie się respawnował)
        GameState.DemonRespawnedInApartment = false;
        GameState.FinalChase = false;
        GameState.ChaseLocked = true;
    }
}