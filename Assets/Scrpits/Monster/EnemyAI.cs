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
    private bool isIdling = false;

    public Transform player;
    public Transform currentDest;
    public int randNum;
    private float idleTime;
    private float loseSightTime = 2f;
    public float loseSightTimer = 0f;
    public bool playerInSight = false;

    [Header("Respawn Settings")]
    public float spawnInvincibilityTime = 1.5f;
    public float spawnInvincibilityTimer = 0f;

    // ✅ POLA DO DETEKCJI ZAPĘTLENIA (na poziomie klasy, nie w metodzie!)
    private float lastProgressTime = 0f;
    private Vector3 lastPosition = Vector3.zero;
    private int stuckCounter = 0; // ✅ Licznik "zacięć" przy tym samym punkcie

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
        // ✅ PRIORYTET 1: FinalChase
        if (GameState.FinalChase)
        {
            if (jumpscareTriggered) return;

            if (!ai.enabled) ai.enabled = true;
            chasing = true;
            walking = false;
            playerInSight = true;

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

            if (distance <= catchDistance)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null && pc.godMode) return;

                jumpscareTriggered = true;
                Debug.Log("[EnemyAI] FINAL CHASE CATCH! Triggering jumpscare sequence ONCE.");

                if (player != null)
                    player.gameObject.SetActive(false);

                aiAnim.SetTrigger("jumpscare");
                RuntimeManager.PlayOneShot(jumpscareEvent);

                chasing = false;
                ai.isStopped = true;
                ai.speed = 0f;

                StartCoroutine(deathRoutine());
                return;
            }

            return;
        }

        if (GameState.DemonInStoryMode)
        {
            if (ai != null && ai.enabled) ai.enabled = false;
            chasing = false;
            walking = false;
            playerInSight = false;
            aiAnim?.SetTrigger("idle");
            return;
        }

        // ✅ PRIORYTET 2: Cooldown po respawnowaniu
        if (spawnInvincibilityTimer > 0f)
        {
            spawnInvincibilityTimer -= Time.deltaTime;
            playerInSight = false;
            chasing = false;
            walking = false;
            ai.isStopped = true;
            ai.speed = 0f;

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
        RaycastHit rayHit;
        if (Physics.Raycast(transform.position + rayCastOffset, direction, out rayHit, sightDistance))
        {
            playerInSight = rayHit.collider.CompareTag("Player");

            if (playerInSight)
            {
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
            isIdling = false;

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
                if (pc != null && pc.godMode) return;

                if (player != null)
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

        // --- PATROL (NAPRAWIONY – BEZ ZAPĘTLENIA) ---
        if (walking && !chasing && !isIdling)
        {
            ai.speed = walkSpeed;
            ai.isStopped = false;

            // ✅ SNAPUJ CEL DO NAVMESH (KLUCZOWE!)
            Vector3 snappedDest = currentDest.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(currentDest.position, out hit, 2f, NavMesh.AllAreas))
            {
                snappedDest = hit.position;
            }

            ai.destination = snappedDest;

            // ✅ ANIMACJA
            aiAnim.ResetTrigger("run");
            aiAnim.SetTrigger("walk");

            // ✅ DEBUG: wizualizacja celu
            Debug.DrawLine(transform.position, snappedDest, Color.cyan, 0f);
            Debug.DrawRay(snappedDest, Vector3.up * 0.5f, Color.red, 0f);

            // ✅ SPRZAWDŹ STATUS ŚCIEŻKI – jeśli niekompletna, przejdź do następnego punktu
            if (ai.pathStatus != NavMeshPathStatus.PathComplete)
            {
                Debug.LogWarning($"[EnemyAI] {name} Path status: {ai.pathStatus} – przechodzę do następnego punktu");

                if (destinations != null && destinations.Count > 0)
                {
                    randNum = (randNum + 1) % destinations.Count;
                    currentDest = destinations[randNum];
                }
                return;
            }

            // ✅ WARUNEK DOTARCIA DO CELU (0.5m margines)
            float distanceToDest = Vector3.Distance(transform.position, snappedDest);
            if (distanceToDest <= 0.5f)
            {
                aiAnim.ResetTrigger("walk");
                aiAnim.SetTrigger("idle");
                ai.speed = 0;
                ai.isStopped = true;

                if (!isIdling)
                {
                    isIdling = true;
                    StartCoroutine(stayIdle());
                }

                walking = false;
                return;
            }

            // ✅ DETEKCJA ZAPĘTLENIA: jeśli demon stoi w tym samym miejscu > 2s
            if (ai.velocity.magnitude < 0.05f) // ✅ Prawie nieruchomy
            {
                if (Vector3.Distance(transform.position, lastPosition) < 0.15f)
                {
                    if (Time.time - lastProgressTime > 2f) // ✅ Stoi > 2s = zapętlenie
                    {
                        stuckCounter++;
                        Debug.LogWarning($"[EnemyAI] {name} Wykryto zapętlenie #{stuckCounter} – przechodzę do następnego punktu");

                        if (stuckCounter >= 2) // ✅ Po 2 zacięciach – zmień punkt
                        {
                            if (destinations != null && destinations.Count > 0)
                            {
                                randNum = (randNum + 1) % destinations.Count;
                                currentDest = destinations[randNum];
                                stuckCounter = 0;
                                Debug.Log($"[EnemyAI] {name} Nowy cel: {currentDest.name}");
                            }
                        }

                        lastProgressTime = Time.time;
                    }
                }
                else
                {
                    // ✅ Resetuj licznik gdy demon się porusza
                    lastProgressTime = Time.time;
                    lastPosition = transform.position;
                    stuckCounter = 0;
                }
            }
            else
            {
                // ✅ Resetuj gdy demon aktywnie się porusza
                lastPosition = transform.position;
                stuckCounter = 0;
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

        isIdling = false;
    }

    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);

        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;

            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;

            var playerRenderers = player.GetComponentsInChildren<Renderer>();
            foreach (var r in playerRenderers)
            {
                if (r != null) r.enabled = false;
            }

            player.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[EnemyAI] ❌ player reference is NULL in deathRoutine!");
        }

        // ✅ KLUCZOWE: RESETUJ WSZYSTKIE STANY DEMONA PO ŚMIERCI
        GameState.DemonInStoryMode = true;          // ✅ Wróć do story mode (agent wyłączony)
        GameState.DemonRespawnedInApartment = false; // ✅ Resetuj respawn w mieszkaniu
        GameState.DemonReadyForChase = false;        // ✅ Resetuj gotowość do chase
        GameState.ChaseLocked = true;                // ✅ Zablokuj chase po resecie

        // Reszta resetów
        GameState.LoopSequenceActive = false;
        GameState.DemonLoopPhase = false;
        GameState.ReadyForFinalChase = false;
        GameState.FinalChase = false;
        jumpscareTriggered = false;

        if (!string.IsNullOrEmpty(deathScene))
        {
            SceneManager.LoadScene(deathScene);
        }
        else
        {
            Debug.LogError("[EnemyAI] ❌ deathScene is empty! Cannot load death scene.");
        }
    }

    public void SetSpawnInvincibility()
    {
        spawnInvincibilityTimer = spawnInvincibilityTime;
        Debug.Log($"[EnemyAI] {name} spawn invincibility set for {spawnInvincibilityTime}s");
    }
}