using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    public DialogActivator demonDialog;
    public GameObject entryTrigger;
    [Header("Final Dialog")]
    public DialogNode finalDialogNode; 


    public QuestManager qmanager;
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

        if (loopCount >= loopsToTriggerDemon && !GameState.DemonLoopPhase)
        {
            StartCoroutine(TriggerDemonPhaseSafe());
        }
    }

    private IEnumerator TriggerDemonPhaseSafe()
    {
        if (GameNarrativeManager.Instance != null)
        {
            GameNarrativeManager.Instance.PlayOneShotAtPlayer(
                GameNarrativeManager.Instance.demonStairsAppearSFX
            );
        }
        if (demon == null || demon.ai == null || demon.aiAnim == null)
        {
            yield break;
        }


        demon.gameObject.SetActive(true);
        SetVisibilityRecursive(demon.transform, true);

        Vector3 spawnPos = demonWaitingPoint.position;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(demonWaitingPoint.position, out navHit, 5f, NavMesh.AllAreas))
        {
            spawnPos = navHit.position;
        }
        else if (NavMesh.SamplePosition(demon.transform.position, out navHit, 10f, NavMesh.AllAreas))
        {
            spawnPos = navHit.position;
        }
        else
        {
            yield break;
        }

        demon.ai.updatePosition = false;
        demon.transform.position = spawnPos;
        demon.transform.rotation = demonWaitingPoint.rotation;
        demon.ai.Warp(spawnPos);
        demon.ai.updatePosition = true;

        yield return null;

        demon.ai.isStopped = true;
        demon.ai.speed = 0f;
        demon.ai.destination = demon.transform.position;
        demon.chasing = false;
        demon.walking = false;
        demon.playerInSight = false;
        demon.loseSightTimer = 0f;
        demon.spawnInvincibilityTimer = demon.spawnInvincibilityTime;

        demon.aiAnim.ResetTrigger("walk");
        demon.aiAnim.ResetTrigger("run");
        demon.aiAnim.SetTrigger("idle");

        if (demonDialog != null && finalDialogNode != null)
        {
            demonDialog.dialogNodes = new DialogNode[] { finalDialogNode };
            demonDialog.isFinalDialog = true; 
            demonDialog.enabled = true;
        }

        GameState.DemonLoopPhase = true;
        GameState.ChaseLocked = true;
    }


    private void SetVisibilityRecursive(Transform root, bool visible)
    {
        SkinnedMeshRenderer[] skinned = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        MeshRenderer[] mesh = root.GetComponentsInChildren<MeshRenderer>(true);

        foreach (var r in skinned)
            if (r != null) r.enabled = visible;

        foreach (var r in mesh)
            if (r != null) r.enabled = visible;

    }

    void TriggerFinalChase()
    {
        if (demon == null || demon.ai == null || demon.player == null) return;
        DemonRoomPresence[] allPresences = FindObjectsOfType<DemonRoomPresence>();
        foreach (var presence in allPresences)
        {
            presence.enabled = false;
        }
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(demon.transform.position, out navHit, 5f, NavMesh.AllAreas))
        {
            demon.ai.Warp(navHit.position);
        }
        entryTrigger.SetActive(true);
        Invoke("StartChaseAfterWarp", 0.1f);
    }

    private void StartChaseAfterWarp()
    {
        if (demon == null || demon.ai == null || demon.player == null) return;
        if (!demon.ai.isOnNavMesh)
        {
            return;
        }
        qmanager.AddQuest("RUN");
        qmanager.CompleteQuest("Go back to your flat");

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

    }
}