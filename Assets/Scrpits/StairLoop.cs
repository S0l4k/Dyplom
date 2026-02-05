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
    public EnemyAI demon;
    public Transform demonWaitingPoint;
    public DialogActivator demonDialog; // ✅ NOWA REFERENCJA!

    private void OnTriggerEnter(Collider other)
    {
        if (!GameState.LoopSequenceActive) return;
        if (GameState.FinalChase) return;
        if (!other.CompareTag("Player")) return;

        if (GameState.ReadyForFinalChase)
        {
            TriggerFinalChase();
            return;
        }

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

        // Po 5 loopach: przenieś demona na dół I aktywuj finalny dialog
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

        // ✅ WARP na NavMesh
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

            if (NavMesh.SamplePosition(demon.transform.position, out navHit, 5f, NavMesh.AllAreas))
            {
                demon.ai.Warp(navHit.position);
            }
        }

        // ✅ ZRESETUJ STANY ENEMY
        demon.ai.isStopped = true;
        demon.ai.speed = 0f;
        demon.ai.destination = demon.transform.position;
        demon.chasing = false;
        demon.walking = false;
        demon.playerInSight = false;
        demon.loseSightTimer = 0f;
        demon.spawnInvincibilityTimer = demon.spawnInvincibilityTime;

        // ✅ ZRESETUJ ANIMACJĘ
        demon.aiAnim.ResetTrigger("walk");
        demon.aiAnim.ResetTrigger("run");
        demon.aiAnim.SetTrigger("idle");

        // ✅ KLUCZOWE: AKTYWUJ FINALNY DIALOG PROGRAMOWO!
        demonDialog.isFinalDialog= true;   

        // ✅ GLOBALNY STAN
        GameState.DemonLoopPhase = true;

        Debug.Log($"[StairLoop] Demon respawned at bottom. Final dialog mode: {(demonDialog != null ? demonDialog.isFinalDialog.ToString() : "UNKNOWN")}");
    }

    void TriggerFinalChase()
    {
        if (demon == null || demon.ai == null || demon.player == null) return;

        if (!demon.ai.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(demon.transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                demon.ai.Warp(hit.position);
            }
            else
            {
                Debug.LogError("[StairLoop] Cannot warp demon to NavMesh! Chase aborted.");
                return;
            }
        }

        demon.ai.isStopped = false;
        demon.ai.speed = demon.chaseSpeed;
        demon.ai.destination = demon.player.position;
        demon.chasing = true;
        demon.walking = false;
        demon.playerInSight = true;

        GameState.FinalChase = true;
        GameState.ChaseLocked = false;
        GameState.LoopSequenceActive = false;
        GameState.ReadyForFinalChase = false;
        GameState.DemonLoopPhase = false;

        GetComponent<Collider>().enabled = false;

        Debug.Log($"[StairLoop] FINAL CHASE ACTIVATED!");
    }
}