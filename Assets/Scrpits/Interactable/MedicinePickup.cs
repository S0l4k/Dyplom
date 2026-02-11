using UnityEngine;
using TMPro;
using System.Collections;

public class MedicinePickup : MonoBehaviour
{
    [Header("References")]
    public LightController lightController;
    public EnemyAI demon;
    public TMP_Text pickupText;
    public GameObject sofaInteractObject; // ✅ Obiekt z SofaInteract (kanapa)

    private Camera playerCamera;
    private bool canInteract = false;

    void Start()
    {
        playerCamera = Camera.main;
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        Debug.Log($"[MedicinePickup] Start | sofaInteractObject={(sofaInteractObject != null ? "OK" : "MISSING!")}");
    }

    void Update()
    {
        if (GameState.InteractionsLocked)
        {
            canInteract = false;
            if (pickupText != null) pickupText.gameObject.SetActive(false);
            return;
        }

        if (pickupText == null || playerCamera == null) return;

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
        // ✅ UKRYJ TEKST NATYCHMIAST
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        Debug.Log("[MedicinePickup] 💊 Leki podniesione – rozpoczynam sekwencję końcową");

        // ✅ Wyłącz demona
        if (demon != null)
        {
            DisableDemonSafely(demon);
            Debug.Log("[MedicinePickup] 👹 Demon wyłączony");
        }
        else
        {
            Debug.LogWarning("[MedicinePickup] ⚠️ demon reference NULL – pomijam wyłączanie demona");
        }

        // ✅ Włącz światła
        if (lightController != null)
        {
            lightController.RestoreLights();
            Debug.Log("[MedicinePickup] 💡 Światła przywrócone");
        }
        else
        {
            Debug.LogWarning("[MedicinePickup] ⚠️ lightController NULL – pomijam przywracanie świateł");
        }

        // ✅ KLUCZOWE: URUCHOM COROUTINE (z pełnym debugowaniem)
        StartCoroutine(StartEndingSequence());
       
    }

    private IEnumerator StartEndingSequence()
    {
        Debug.Log("[MedicinePickup] ⏳ Rozpoczynam sekwencję końcową (po 1.5s)");

        yield return new WaitForSeconds(1.5f);

        Debug.Log("[MedicinePickup] 💭 Wyświetlam myśl 'He is gone...'");

        // ✅ BEZPIECZNE WYŚWIETLANIE MYŚLI
        if (GameNarrativeManager.Instance != null)
        {
            Debug.Log("[MedicinePickup] ✅ GameNarrativeManager.Instance znaleziony");

            if (GameNarrativeManager.Instance.thoughtText != null)
            {
                Debug.Log("[MedicinePickup] ✅ thoughtText znaleziony – uruchamiam ShowThought");
                yield return GameNarrativeManager.Instance.StartCoroutine(
                    GameNarrativeManager.Instance.ShowThought("He is gone...", 0.09f, 2.5f)
                );
                Debug.Log("[MedicinePickup] ✅ Myśl 'He is gone...' wyświetlona");
            }
            else
            {
                Debug.LogError("[MedicinePickup] ❌ thoughtText NULL – nie mogę wyświetlić myśli!");
            }
        }
        else
        {
            Debug.LogError("[MedicinePickup] ❌ GameNarrativeManager.Instance NULL – nie mogę wyświetlić myśli!");
        }

        // ✅ BLOKUJ WSZYSTKIE INTERAKCJE (KLUCZOWE!)
        GameState.InteractionsLocked = true;
        Debug.Log("[MedicinePickup] 🔒 InteractionsLocked = true – interakcje zablokowane");

        // ✅ AKTYWUJ KANAPĘ
        if (sofaInteractObject != null)
        {
            sofaInteractObject.SetActive(true);
            Debug.Log("[MedicinePickup] 🛋️ sofaInteractObject aktywowany");
        }
        else
        {
            Debug.LogError("[MedicinePickup] ❌ sofaInteractObject NULL – kanapa NIE zostanie aktywowana!");
        }

        Debug.Log("[MedicinePickup] ✅ Sekwencja końcowa zakończona – oczekuję interakcji z kanapą");
    }

    private void DisableDemonSafely(EnemyAI demon)
    {
        if (demon.ai != null)
        {
            demon.ai.isStopped = true;
            demon.ai.speed = 0f;
            demon.ai.enabled = false;
        }

        demon.chasing = false;
        demon.walking = false;
        demon.playerInSight = false;

        foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            if (r != null) r.enabled = false;

        demon.aiAnim?.SetTrigger("idle");

        GameState.DemonRespawnedInApartment = false;
        GameState.FinalChase = false;
        GameState.ChaseLocked = true;
    }
}