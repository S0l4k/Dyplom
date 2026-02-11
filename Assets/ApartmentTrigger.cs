using UnityEngine;
using System.Collections;

public class ApartmentTrigger : MonoBehaviour
{
    [Header("Demon Settings")]
    public EnemyAI demon;                   // ✅ Przypisz demona
    public Transform demonSpawnPoint;       // ✅ Punkt respawnu w środku mieszkania
    public float respawnDelay = 10f;        // ✅ 10 sekund opóźnienia

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameState.DemonRespawnedInApartment) return; // Demon już się zrespawnował

        Debug.Log("[Apartment] Gracz wszedł do mieszkania – kończę chase, planuję respawn demona");

        // ✅ ZATRZYMAJ AKTUALNY CHASE (jeśli trwa)
        GameState.FinalChase = false;

        // ✅ UKRYJ DEMONA WIZUALNIE (ale NIE wyłączaj NavMeshAgent – zostanie włączony przy respawnie)
        if (demon != null)
        {
            foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (r != null) r.enabled = false;

            demon.chasing = false;
            demon.walking = false;
            demon.playerInSight = false;
            demon.ai.isStopped = true;
            demon.ai.speed = 0f;
        }

        // ✅ PLANUJ RESPAWN DEMONA PO 10 SEKUNDACH
        if (demon != null && demonSpawnPoint != null)
        {
            StartCoroutine(RespawnDemonAfterDelay());
        }
    }

    private IEnumerator RespawnDemonAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (demon == null || demonSpawnPoint == null) yield break;
        if (GameState.DemonRespawnedInApartment) yield break; // Zapobiegaj podwójnemu respawnowi

        Debug.Log("[Apartment] ✅ Respawn demona w mieszkaniu po " + respawnDelay + "s");

        // ✅ TELEPORT DEMONA DO MIESZKANIA
        demon.transform.position = demonSpawnPoint.position;
        demon.transform.rotation = demonSpawnPoint.rotation;

        // ✅ WŁĄCZ RENDERERY
        foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            if (r != null) r.enabled = true;

        // ✅ AKTYWUJ AGENTA I PATROLOWANIE
        demon.ai.enabled = true;
        demon.ai.isStopped = false;
        demon.ai.speed = demon.walkSpeed;
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

        // ✅ GLOBALNA FLAGA (by nie respawnić ponownie)
        GameState.DemonRespawnedInApartment = true;

        Debug.Log("[Apartment] 👹 Demon patroluje w mieszkaniu – CHASE AKTYWNY (jeśli zobaczy gracza)");
    }
}