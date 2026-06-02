using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ApartmentTrigger : MonoBehaviour
{
    [Header("Demon Settings")]
    public EnemyAI demon;                  
    public Transform demonSpawnPoint;      
    public float respawnDelay = 10f;       

    private bool hasTriggered = false;
    public LightController lightController;
    public GameObject flashlight;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered) return; 
        if (GameState.DemonRespawnedInApartment) return; 

        hasTriggered = true;

        GameState.FinalChase = false;

        if (demon != null && demon.ai != null)
        {
            if (demon.ai.enabled && demon.ai.isOnNavMesh)
            {
                demon.ai.isStopped = true;
                demon.ai.speed = 0f;
            }

            demon.ai.enabled = false;
            demon.chasing = false;
            demon.walking = false;
            demon.playerInSight = false;

            foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (r != null) r.enabled = false;

        }
        QuestManager.Instance.CompleteQuest("RUN");
        if (GameNarrativeManager.Instance != null)
        {
            GameNarrativeManager.Instance.ChangeBackgroundMusic(
                GameNarrativeManager.Instance.patrolMusic,
                GameNarrativeManager.Instance.patrolFadeTime 
            );
        }

        if (demon != null && demonSpawnPoint != null)
        {
            StartCoroutine(RespawnDemonAfterDelay());
        }
    }

    private IEnumerator RespawnDemonAfterDelay()
    {
      
        yield return new WaitForSeconds(respawnDelay);
        if (flashlight != null) flashlight.SetActive(true);
        lightController.ActivateDemonLights();
        if (demon == null || demonSpawnPoint == null) yield break;
        if (GameState.DemonRespawnedInApartment) yield break;
        demon.transform.position = demonSpawnPoint.position;
        demon.transform.rotation = demonSpawnPoint.rotation;
        QuestManager.Instance.AddQuest("Find flashlight");

        if (demon.ai != null)
        {
            demon.ai.enabled = true;
            yield return null; 

            demon.ai.Warp(demonSpawnPoint.position);
            demon.ai.isStopped = false;
            demon.ai.speed = demon.walkSpeed;
        }

        foreach (var r in demon.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            if (r != null) r.enabled = true;

        demon.chasing = false;
        demon.walking = true;
        demon.playerInSight = false;
        demon.loseSightTimer = 0f;

        if (demon.destinations != null && demon.destinations.Count > 0)
        {
            demon.randNum = 0;
            demon.currentDest = demon.destinations[demon.randNum];
        }

        demon.aiAnim?.SetTrigger("walk");

        GameState.DemonRespawnedInApartment = true;
        GameState.ChaseLocked = false;

    }
}