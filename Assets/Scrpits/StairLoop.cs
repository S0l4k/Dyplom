using UnityEngine;
using UnityEngine.AI;

public class StairLoop : MonoBehaviour
{
    [Header("Segments")]
    public Transform startSegment;
    public Transform endSegment;

    [Header("Loop Logic")]
    public int loopCount = 0;
    public int loopsToTriggerDemon = 5;

    [Header("Demon")]
    public EnemyAI demon; // Upewnij się, że jest przypisany w Inspectorze!
    public Transform demonWaitingPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!GameState.LoopSequenceActive) return;
        if (GameState.FinalChase) return; // już w pościgu - ignoruj
        if (!other.CompareTag("Player")) return;

        // ✅ NOWA LOGIKA: po dialogu - aktywuj pościg BEZ teleportacji
        if (GameState.ReadyForFinalChase)
        {
            TriggerFinalChase();
            return; // NIE teleportujemy gracza!
        }

        // Normalna pętla (przed dialogiem)
        loopCount++;
        Debug.Log($"[StairLoop] Loop #{loopCount}");

        // Teleportacja gracza
        Vector3 offset = other.transform.position - endSegment.position;
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            other.transform.position = startSegment.position + offset;
            cc.enabled = true;
        }
        else
        {
            other.transform.position = startSegment.position + offset;
        }

        // Po 5 loopach: przenieś demona na dół
        if (loopCount >= loopsToTriggerDemon && !GameState.DemonLoopPhase)
        {
            TriggerDemonPhase();
        }
    }

    void TriggerDemonPhase()
    {
        if (demon == null || demon.ai == null || demon.aiAnim == null)
        {
            Debug.LogError("[StairLoop] Demon references not set in Inspector!");
            return;
        }

        // ✅ WARP na NavMesh (NIGDY nie ustawiaj ręcznie transform.position!)
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(demonWaitingPoint.position, out navHit, 2f, NavMesh.AllAreas))
        {
            demon.ai.Warp(navHit.position);
            demon.transform.rotation = demonWaitingPoint.rotation;
        }
        else
        {
            Debug.LogWarning("[StairLoop] demonWaitingPoint not on NavMesh! Using fallback.");
            demon.transform.position = demonWaitingPoint.position;
            demon.transform.rotation = demonWaitingPoint.rotation;

            // Spróbuj znowu zwiększyć dystans szukania
            if (NavMesh.SamplePosition(demon.transform.position, out navHit, 5f, NavMesh.AllAreas))
            {
                demon.ai.Warp(navHit.position);
            }
        }

        // ✅ KLUCZOWE: ZATRZYMAJ AGENTA, ALE NIE WYŁĄCZAJ GO!
        demon.ai.isStopped = true;  // ✅ PRAWIDŁOWY SPOSÓB NA ZATRZYMANIE
        demon.ai.speed = 0f;
        demon.ai.destination = demon.transform.position; // zatrzymaj ruch

        // ✅ ZRESETUJ WSZYSTKIE STANY ENEMY
        demon.chasing = false;
        demon.walking = false;      // 🔴 MUSI BYĆ FALSE! To blokowało dialog
        demon.playerInSight = false;
        demon.loseSightTimer = 0f;

        // ✅ ZRESETUJ ANIMACJĘ NA IDLE
        demon.aiAnim.ResetTrigger("walk");
        demon.aiAnim.ResetTrigger("run");
        demon.aiAnim.SetTrigger("idle");

        // ✅ USTAW COOLDOWN PRZED DETEKCJĄ (zapobiega natychmiastowemu chase'owi)
        demon.spawnInvincibilityTimer = demon.spawnInvincibilityTime;

        // ✅ GLOBALNY STAN
        GameState.DemonLoopPhase = true;

        Debug.Log($"[StairLoop] Demon respawned at bottom. Position: {demon.transform.position}, isOnNavMesh: {demon.ai.isOnNavMesh}, walking={demon.walking}");
    }

    // ✅ NOWA METODA: aktywacja finałowego pościgu
    void TriggerFinalChase()
    {
        if (demon == null || demon.ai == null || demon.player == null) return;

        // ✅ SPRAWDŹ CZY AGENT JEST NA NAVMESH PRZED UŻYCIEM DESTINATION
        if (!demon.ai.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(demon.transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                Debug.LogWarning("[StairLoop] Demon not on NavMesh! Warping to nearest valid position.");
                demon.ai.Warp(hit.position);
            }
            else
            {
                Debug.LogError("[StairLoop] Cannot warp demon to NavMesh! Chase aborted.");
                return;
            }
        }

        // ✅ WZNÓW RUCH
        demon.ai.isStopped = false;
        demon.ai.speed = demon.chaseSpeed;
        demon.ai.destination = demon.player.position;

        // ✅ FLAGI ENEMY
        demon.chasing = true;
        demon.walking = false;
        demon.playerInSight = true;

        // ✅ GLOBALNE STANY
        GameState.FinalChase = true;
        GameState.ChaseLocked = false;
        GameState.LoopSequenceActive = false;
        GameState.ReadyForFinalChase = false;
        GameState.DemonLoopPhase = false;

        // Wyłącz collider loopa by uniknąć ponownej aktywacji
        GetComponent<Collider>().enabled = false;

        Debug.Log($"[StairLoop] FINAL CHASE ACTIVATED! Demon position: {demon.transform.position}, isOnNavMesh: {demon.ai.isOnNavMesh}");
    }
}