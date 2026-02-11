using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using FMODUnity;

public class EnemyAI : MonoBehaviour
{
    public static List<EnemyAI> AllEnemies = new List<EnemyAI>();

    [Header("Components")]
    public NavMeshAgent ai;
    public Animator aiAnim;

    [Header("Patrol Settings")]
    public List<Transform> destinations;
    public float walkSpeed = 2f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

    [Header("Chase Settings")]
    public float chaseSpeed = 5f;
    public float sightDistance = 10f;
    public float catchDistance = 1.5f;
    public Vector3 rayCastOffset = Vector3.zero;

    [Header("Death Settings")]
    public string deathScene;
    public float jumpscareTime = 1f;
    [SerializeField] private EventReference jumpscareEvent;
    private bool jumpscareTriggered = false;

    [Header("Internal State")]
    public bool walking = true;
    public bool chasing = false;

    public Transform player;

    private Transform currentDest;
    private int randNum;
    private float idleTime;
    private float loseSightTime = 2f;
    public float loseSightTimer = 0f;
    public bool playerInSight = false;

    [Header("Respawn Settings")]
    public float spawnInvincibilityTime = 1.5f;
    public float spawnInvincibilityTimer = 0f;

    private void Awake()
    {
        AllEnemies.Add(this);
    }

    private void OnDestroy()
    {
        AllEnemies.Remove(this);
    }

    private void Start()
    {
        walking = true;
        if (destinations != null && destinations.Count > 0)
        {
            randNum = Random.Range(0, destinations.Count);
            currentDest = destinations[randNum];
        }
        else
        {
            Debug.LogWarning($"[EnemyAI] {name} has no patrol destinations assigned!");
        }
    }

    private void Update()
    {
        // ✅ PRIORYTET 1: FinalChase (absolutnie najwyższy priorytet)
        if (GameState.FinalChase)
        {
            // ✅ KLUCZOWE ZABEZPIECZENIE: blokada wielokrotnego wywołania jumpscare
            if (jumpscareTriggered)
            {
                // Jumpscare już aktywowany - nic nie rób (czekaj na załadowanie sceny śmierci)
                return;
            }

            if (!ai.enabled) ai.enabled = true;
            chasing = true;
            walking = false;
            playerInSight = true;

            // ✅ WALIDACJA PLAYERA
            if (player == null)
            {
                Debug.LogError("[EnemyAI] Player reference is null during FinalChase!");
                return;
            }

            if (ai.isOnNavMesh)
            {
                ai.destination = player.position;
                ai.speed = chaseSpeed;
                ai.isStopped = false;
                aiAnim.SetTrigger("run");
            }
            else
            {
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(transform.position, out navHit, 2f, NavMesh.AllAreas))
                {
                    ai.Warp(navHit.position);
                    ai.destination = player.position;
                    ai.speed = chaseSpeed;
                    ai.isStopped = false;
                    aiAnim.SetTrigger("run");
                }
                else
                {
                    Debug.LogWarning($"[EnemyAI] {name} cannot chase - not on NavMesh!");
                    return;
                }
            }

            float distance = Vector3.Distance(player.position, transform.position);

            // ✅ WARUNEK AKTYWACJI JUMPSCARE (z blokadą)
            if (distance <= catchDistance)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null && pc.godMode)
                    return;

                // ✅ AKTYWACJA JUMPSCARE (TYLKO RAZ!)
                jumpscareTriggered = true; // 🔒 BLOKADA - zapobiega ponownemu wywołaniu

                Debug.Log("[EnemyAI] FINAL CHASE CATCH! Triggering jumpscare sequence ONCE.");

                player.gameObject.SetActive(false);
                aiAnim.SetTrigger("jumpscare");
                RuntimeManager.PlayOneShot(jumpscareEvent); // ✅ DŹWIĘK

                chasing = false;
                ai.isStopped = true;
                ai.speed = 0f;

                StartCoroutine(deathRoutine());

                return; // ✅ ZAKOŃCZ UPDATE NATYCHMIAST - nie kontynuuj logiki
            }

            return; // ✅ ZAKOŃCZ UPDATE dla FinalChase
        }
        if (GameState.DemonInStoryMode)
        {
            // Wyłączamy agenta i resetujemy stany
            if (ai != null && ai.enabled) ai.enabled = false;
            chasing = false;
            walking = false;
            playerInSight = false;
            aiAnim?.SetTrigger("idle");
            return; // NIC WIĘCEJ NIE ROBIMY
        }


        // ✅ PRIORYTET 2: Cooldown po respawnowaniu (blokuje detekcję i ruch)
        if (spawnInvincibilityTimer > 0f)
        {
            spawnInvincibilityTimer -= Time.deltaTime;
            playerInSight = false;
            chasing = false;
            walking = false;
            ai.isStopped = true;
            ai.speed = 0f;

            // Upewnij się, że animacja jest idle
            if (aiAnim != null)
            {
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("run");
                aiAnim.SetTrigger("idle");
            }

            return;
        }

        // ✅ PRIORYTET 3: Rozmowa z graczem
        if (GameState.IsTalking)
        {
            ai.speed = 0;
            ai.isStopped = true;
            aiAnim.SetTrigger("idle");

            // Patrzenie na gracza mimo rozmowy
            Vector3 lookDir = (player.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir.magnitude > 0.1f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 120f * Time.deltaTime);
            }
            return;
        }

        // --- DETEKCJA GRACZA ---
        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit rayHit; // ✅ UNIKNIĘCIE KONFLIKTU Z NavMeshHit
        if (Physics.Raycast(transform.position + rayCastOffset, direction, out rayHit, sightDistance))
        {
            playerInSight = rayHit.collider.CompareTag("Player");

            if (playerInSight)
            {
                // SPRAWDZENIE SKRADANIA
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null && pc.isSneaking && !GameState.ChaseLocked)
                {
                    float sneakFactor = 0.4f;
                    if (Vector3.Distance(player.position, transform.position) > sightDistance * sneakFactor)
                    {
                        playerInSight = false;
                    }
                    else
                    {
                        if (Random.value < 0.3f)
                            playerInSight = false;
                    }
                }

                if (playerInSight)
                    loseSightTimer = loseSightTime;
            }
        }
        else
        {
            playerInSight = false;
        }

        // --- CHASE GRACZA ---
        if (playerInSight && !GameState.ChaseLocked)
        {
            chasing = true;
            walking = false;

            // ✅ ZABEZPIECZENIE PRZED UŻYCIEM DESTINATION
            if (!ai.isOnNavMesh)
            {
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(transform.position, out navHit, 2f, NavMesh.AllAreas))
                {
                    ai.Warp(navHit.position);
                }
                else
                {
                    Debug.LogWarning($"[EnemyAI] {name} cannot chase - not on NavMesh!");
                    return;
                }
            }

            ai.destination = player.position;
            ai.speed = chaseSpeed;
            ai.isStopped = false;

            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("run");

            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= catchDistance)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null && pc.godMode)
                    return;

                player.gameObject.SetActive(false);
                aiAnim.SetTrigger("jumpscare");
                StartCoroutine(deathRoutine());
                chasing = false;
                RuntimeManager.PlayOneShot(jumpscareEvent);
            }
        }
        else if (chasing)
        {
            loseSightTimer -= Time.deltaTime;
            if (loseSightTimer <= 0)
            {
                chasing = false;
                walking = true;
                if (destinations != null && destinations.Count > 0)
                {
                    randNum = Random.Range(0, destinations.Count);
                    currentDest = destinations[randNum];
                }
            }
        }

        // --- PATROL ---
        if (walking && !chasing)
        {
            ai.speed = walkSpeed;
            ai.isStopped = false;

            // ✅ ZABEZPIECZENIE: sprawdź czy agent jest aktywny i na NavMesh
            if (ai.isActiveAndEnabled && ai.isOnNavMesh)
            {
                ai.destination = currentDest.position;
            }
            else
            {
                // Jeśli nie na NavMesh, spróbuj przywrócić
                NavMeshHit navHit; // ✅ UNIKNIĘCIE KONFLIKTU NAZW
                if (NavMesh.SamplePosition(transform.position, out navHit, 1f, NavMesh.AllAreas))
                {
                    ai.Warp(navHit.position);
                    ai.destination = currentDest.position;
                }
                else
                {
                    Debug.LogWarning($"[EnemyAI] {name} not on NavMesh! Skipping patrol.");
                    return;
                }
            }

            aiAnim.ResetTrigger("run");
            aiAnim.SetTrigger("walk");

            // ✅ ZABEZPIECZENIE dla remainingDistance
            if (ai.isActiveAndEnabled && !ai.pathPending && ai.hasPath && ai.remainingDistance <= ai.stoppingDistance)
            {
                aiAnim.ResetTrigger("walk");
                aiAnim.SetTrigger("idle");
                ai.speed = 0;
                ai.isStopped = true;

                if (!IsInvoking("dummy"))
                    StartCoroutine(stayIdle());

                walking = false;
            }
        }
    }

    IEnumerator stayIdle()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);

        if (destinations != null && destinations.Count > 0)
        {
            randNum = Random.Range(0, destinations.Count);
            currentDest = destinations[randNum];
            walking = true;
        }
    }

    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);

        // ✅ NULL-CHECKI – player może być null po śmierci/respawnie
        if (player != null)
        {
            // ✅ Wyłącz komponenty gracza
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.enabled = false;

            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = false;

            // ✅ UKRYJ GRACZA WIZUALNIE
            var playerRenderers = player.GetComponentsInChildren<Renderer>();
            foreach (var r in playerRenderers)
            {
                if (r != null)
                    r.enabled = false;
            }

            // ✅ UKRYJ CAŁY GAMEOBJECT GRACZA (dodatkowa ochrona)
            player.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[EnemyAI] ❌ player reference is NULL in deathRoutine!");
        }

        // Reset globalnych stanów
        GameState.LoopSequenceActive = false;
        GameState.DemonLoopPhase = false;
        GameState.ReadyForFinalChase = false;
        GameState.FinalChase = false;
        GameState.ChaseLocked = true;
        jumpscareTriggered = false;

        // ✅ SPRAWDŹ CZY deathScene nie jest pusty
        if (!string.IsNullOrEmpty(deathScene))
        {
            SceneManager.LoadScene(deathScene);
        }
        else
        {
            Debug.LogError("[EnemyAI] ❌ deathScene is empty! Cannot load death scene.");
        }
    }

    // ✅ PUBLICZNA METODA DO ZEWNĘTRZNEGO USTAWIENIA COOLDOWNU (np. z StairLoop)
    public void SetSpawnInvincibility()
    {
        spawnInvincibilityTimer = spawnInvincibilityTime;
        Debug.Log($"[EnemyAI] {name} spawn invincibility set for {spawnInvincibilityTime}s");
    }
}