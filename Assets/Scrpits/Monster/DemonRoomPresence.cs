using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class RoomPresenceData
{
    public string roomTag;          // np. "Kitchen", "LivingRoom_TV"
    public Transform spawnPoint;    // pozycja + rotacja demona
    public string animationState;   // np. "sit_counter", "lie_bed" (trigger w Animatorze)
    public DialogNode[] dialogNodes; // dialog specyficzny dla pokoju
}

public class DemonRoomPresence : MonoBehaviour
{
    public List<RoomPresenceData> roomPresences;
    public EnemyAI demonAI;
    public DialogActivator dialogActivator;
    public Animator animator;
    public float appearDuration = 0.3f; // płynne pojawianie

    private RoomPresenceData currentRoom;
    private Coroutine activeRoutine;

    private void Start()
    {
        gameObject.SetActive(false); // demon startuje NIEWIDOCZNY
    }

    // Wywoływane przez RoomTrigger gdy gracz wejdzie do pokoju
    public void EnterRoom(string roomTag)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);

        var data = roomPresences.Find(r => r.roomTag == roomTag);
        if (data == null) return;

        currentRoom = data;
        activeRoutine = StartCoroutine(AppearInRoom(data));
    }

    // Wywoływane gdy gracz opuści pokój
    public void ExitRoom()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        gameObject.SetActive(false);
        currentRoom = null;
    }

    private IEnumerator AppearInRoom(RoomPresenceData data)
    {
        // 1. Teleport na pozycję (bez kolizji)
        transform.position = data.spawnPoint.position;
        transform.rotation = data.spawnPoint.rotation;

        // 2. Wyłącz kolizje na chwilę pojawienia
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders) col.enabled = false;

        // 3. Aktywuj demona
        gameObject.SetActive(true);

        // 4. Odtwórz animację aktywności
        if (!string.IsNullOrEmpty(data.animationState) && animator != null)
        {
            animator.SetTrigger(data.animationState);
        }
        else
        {
            animator?.SetTrigger("idle");
        }

        // 5. Włącz kolizje po chwili
        yield return new WaitForSeconds(0.1f);
        foreach (var col in colliders) col.enabled = true;

        // 6. Ustaw dialog
        if (dialogActivator != null)
        {
            dialogActivator.dialogNodes = data.dialogNodes;
            dialogActivator.enabled = true;
        }
    }

    // ✅ PRZEŁĄCZ W TRYB PATROLOWANIA (wywołaj z eventu fabularnego)
    public void SwitchToPatrolMode()
    {
        ExitRoom();
        GameState.DemonInStoryMode = false; // odblokowuje EnemyAI

        // Opcjonalnie: przenieś demona na punkt startowy patrolu
        if (demonAI != null && demonAI.destinations.Count > 0)
        {
            transform.position = demonAI.destinations[0].position;
            demonAI.ai.enabled = true;
            demonAI.walking = true;
        }
    }
}