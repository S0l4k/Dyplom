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

    [Header("Internal State")]
    public bool walking = true;
    public bool chasing = false;

    public Transform player;

    private Transform currentDest;
    private int randNum;
    private float idleTime;
    private float loseSightTime = 2f;
    private float loseSightTimer = 0f;
    private bool playerInSight = false;

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
        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
    }

    private void Update()
    {
        if (GameState.FinalChase)
        {
            chasing = true;
            walking = false;
            playerInSight = true;

            ai.destination = player.position;
            ai.speed = chaseSpeed;
            aiAnim.SetTrigger("run");
        }
        if (GameState.IsTalking)
        {
            ai.speed = 0;
            aiAnim.SetTrigger("idle");

            // patrzenie na gracza mimo rozmowy
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
        RaycastHit hit;
        if (Physics.Raycast(transform.position + rayCastOffset, direction, out hit, sightDistance))
        {
            playerInSight = hit.collider.CompareTag("Player");

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

            ai.destination = player.position;
            ai.speed = chaseSpeed;

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
                randNum = Random.Range(0, destinations.Count);
                currentDest = destinations[randNum];
            }
        }

        // --- PATROL ---
        if (walking && !chasing)
        {
            ai.speed = walkSpeed;
            ai.destination = currentDest.position;

            aiAnim.ResetTrigger("run");
            aiAnim.SetTrigger("walk");

            if (!ai.pathPending && ai.remainingDistance <= ai.stoppingDistance)
            {
                aiAnim.ResetTrigger("walk");
                aiAnim.SetTrigger("idle");
                ai.speed = 0;

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

        randNum = Random.Range(0, destinations.Count);
        currentDest = destinations[randNum];
        walking = true;
    }

    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(deathScene);
        GameState.ChaseLocked = true;
    }
}
