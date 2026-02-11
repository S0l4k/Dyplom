using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ApartmentTrigger : MonoBehaviour
{
    [Header("Demon Settings")]
    public EnemyAI demon;                   // ✅ Przypisz demona
    public Transform demonSpawnPoint;       // ✅ Punkt respawnu W ŚRODKU MIESZKANIA
    public float respawnDelay = 10f;        // ✅ 10 sekund

    private bool hasTriggered = false;
    public LightController lightController;// ✅ Zapobiega wielokrotnemu wywołaniu
    public GameObject flashlight;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered) return; // ✅ Już aktywowany – ignoruj kolejne wejścia
        if (GameState.DemonRespawnedInApartment) return; // ✅ Demon już w mieszkaniu

        hasTriggered = true;
        Debug.Log("[ApartmentTrigger] ✅ Gracz wszedł do mieszkania – aktywuję respawn demona");

        // ✅ ZATRZYMAJ AKTUALNY CHASE (jeśli trwa)
        GameState.FinalChase = false;

        // ✅ KLUCZOWE: WYŁĄCZ DEMONA BEZ BŁĘDÓW NAVMESH
        if (demon != null && demon.ai != null)
        {
            // ✅ 1. ZATRZYMAJ AGENTA ZANIM GO WYŁĄCZYMY (BEZPIECZNIE!)
            if (demon.ai.enabled && demon.ai.isOnNavMesh)
            {
                demon.ai.isStopped = true;
                demon.ai.speed = 0f;
            }

            // ✅ 2. WYŁĄCZ AGENTA
            demon.ai.enabled = false;

            // ✅ 3. ZRESETUJ STANY
            demon.chasing = false;
            demon.walking = false;
            demon.playerInSight = false;

            // ✅ 4. UKRYJ WIZUALNIE
            foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (r != null) r.enabled = false;

            Debug.Log("[ApartmentTrigger] 👹 Demon WYŁĄCZONY fizycznie i wizualnie");
        }

        // ✅ PLANUJ RESPAWN W MIESZKANIU
        if (demon != null && demonSpawnPoint != null)
        {
            StartCoroutine(RespawnDemonAfterDelay());
        }
    }

    private IEnumerator RespawnDemonAfterDelay()
    {
        flashlight.SetActive(true);
        yield return new WaitForSeconds(respawnDelay);
        lightController.ActivateDemonLights();
        if (demon == null || demonSpawnPoint == null) yield break;
        if (GameState.DemonRespawnedInApartment) yield break;

        Debug.Log("[ApartmentTrigger] ✅ Respawn demona W MIESZKANIU po " + respawnDelay + "s");

        // ✅ TELEPORT DO MIESZKANIA
        demon.transform.position = demonSpawnPoint.position;
        demon.transform.rotation = demonSpawnPoint.rotation;

        // ✅ KLUCZOWE: WŁĄCZ AGENTA ZANIM UŻYJEMY WARP (inaczej błąd!)
        if (demon.ai != null)
        {
            demon.ai.enabled = true; // ✅ NAJPIERW włącz
            yield return null;       // ✅ Poczekaj 1 klatkę na aktywację

            // ✅ TERAZ warp (agent jest aktywny na NavMesh)
            demon.ai.Warp(demonSpawnPoint.position);
            demon.ai.isStopped = false;
            demon.ai.speed = demon.walkSpeed;
        }

        // ✅ WŁĄCZ RENDERERY
        foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            if (r != null) r.enabled = true;

        // ✅ AKTYWUJ PATROLOWANIE
        demon.chasing = false;
        demon.walking = true;
        demon.playerInSight = false;
        demon.loseSightTimer = 0f;

        // ✅ USTAW PIERWSZY PUNKT PATROLU
        if (demon.destinations != null && demon.destinations.Count > 0)
        {
            demon.randNum = 0;
            demon.currentDest = demon.destinations[demon.randNum];
        }

        // ✅ ANIMACJA
        demon.aiAnim?.SetTrigger("walk");

        // ✅ GLOBALNA FLAGA
        GameState.DemonRespawnedInApartment = true;
        GameState.ChaseLocked = false; // ✅ Chase AKTYWNY w mieszkaniu

        Debug.Log("[ApartmentTrigger] 👹 Demon aktywny W MIESZKANIU – patroluje z chase'em");
    }
}