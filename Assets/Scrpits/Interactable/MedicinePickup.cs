using UnityEngine;
using TMPro;
using System.Collections;

public class MedicinePickup : MonoBehaviour
{
    [Header("References")]
    public LightController lightController;
    public EnemyAI demon;
    public GameObject pickupText;
    public GameObject sofaInteractObject; // ✅ Obiekt z SofaInteract (kanapa)
    public Outline outline;
    private Camera playerCamera;
    private bool canInteract = false;

    void Start()
    {
        playerCamera = Camera.main;
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
        if (outline != null) outline.enabled = false;
        Debug.Log($"[MedicinePickup] Start | sofaInteractObject={(sofaInteractObject != null ? "OK" : "MISSING!")}");
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
        // ✅ UKRYJ TEKST NATYCHMIAST
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
        QuestManager.Instance.ClearAllQuests();
        QuestManager.Instance.AddQuest("Rest on Sofa");
        Debug.Log("[MedicinePickup] ➕ Quest added: Rest on Sofa");
        GameNarrativeManager.Instance.PlayOneShotAtPlayer(GameNarrativeManager.Instance.takingMeds);
        Debug.Log("[MedicinePickup] 💊 Leki podniesione – rozpoczynam sekwencję końcową");

        GameNarrativeManager.Instance.blood.SetActive(false);
    

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
            GameNarrativeManager.Instance.ChangeBackgroundMusic(
               GameNarrativeManager.Instance.victoryMusic,
               GameNarrativeManager.Instance.victoryFadeTime  // ✅ Użyj pola z Inspektora
            );
            GameNarrativeManager.Instance.PlayOneShotAtPlayer(GameNarrativeManager.Instance.demonDefeatedSFX );
            

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
        // ✅ Wyłącz NavMeshAgent i animacje (istniejący kod)
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

        // ✅ NOWE: Wyłącz WSZYSTKIE collidery demona (główny + dzieci)
        Debug.Log($"[MedicinePickup] 🔕 Disabling colliders on {demon.name}");

        // 1. Collidery na głównym obiekcie demona
        foreach (var col in demon.GetComponents<Collider>())
        {
            if (col != null)
            {
                col.enabled = false;
                Debug.Log($"[MedicinePickup] 🔕 Disabled Collider: {col.GetType().Name} on {demon.name}");
            }
        }

        // 2. Collidery na dzieciach demona (np. ręce, body parts)
        foreach (var col in demon.GetComponentsInChildren<Collider>(true))
        {
            if (col != null && col.gameObject != demon.gameObject) // Uniknij dubla głównego
            {
                col.enabled = false;
                Debug.Log($"[MedicinePickup] 🔕 Disabled child Collider: {col.GetType().Name} on {col.gameObject.name}");
            }
        }

        // ✅ Aktualizuj flagi GameState (istniejący kod)
        GameState.DemonRespawnedInApartment = false;
        GameState.FinalChase = false;
        GameState.ChaseLocked = true;

        Debug.Log($"[MedicinePickup] 👹 Demon fully disabled: AI + visuals + colliders");
    }
}