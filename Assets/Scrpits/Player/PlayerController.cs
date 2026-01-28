using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float gravity = -20f;

    [Header("Sprint & Stamina")]
    public float maxStamina = 5f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 0.8f;
    public bool godMode = false;

    [Header("References")]
    public Transform orientation;
    public GameObject console;
    public Transform hand;
    public TMP_Text pickupText;
    public PauseMenu pauseMenu;

    [Header("Audio")]
    private PlayerSound playerSound;
    private float lastStepTime;
    public float walkStepInterval = 0.22f;
    public float sprintStepInterval = 0.13f;


    private CharacterController controller;
    private Vector3 velocity;
    private float currentStamina;
    private bool isSprinting;
    private bool isExhausted;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 1.3f;

    private bool IsGroundedCustom()
    {
        return Physics.Raycast(
            groundCheck.position,
            Vector3.down,
            groundCheckDistance
        );
    }

    void Start()
    {
        playerSound = GetComponent<PlayerSound>();
        controller = GetComponent<CharacterController>();
        currentStamina = maxStamina;
    }

    void Update()
    {
        HandleInput();
        HandleStamina();
        ApplyGravity();
        Move();
        HandleFootsteps();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            pauseMenu.OpenConsole();
        }

    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move =
            orientation.forward * vertical +
            orientation.right * horizontal;

        move.Normalize();

        float speed = isSprinting ? sprintSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);
    }
    private void HandleFootsteps()
    {
        if (!IsGroundedCustom()) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        if (!isMoving) return;

        float interval = isSprinting ? sprintStepInterval : walkStepInterval;

        if (Time.time - lastStepTime >= interval)
        {
            playerSound.PlayFootstep();
            lastStepTime = Time.time;
        }
    }


    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f; 

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleStamina()
    {
        bool wantsToSprint =
            Input.GetKey(KeyCode.LeftShift) &&
            !isExhausted &&
            Input.GetAxisRaw("Vertical") > 0;

        isSprinting = wantsToSprint && currentStamina > 0;

        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
                isSprinting = false;
            }
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (currentStamina >= maxStamina * 0.4f)
                    isExhausted = false;
            }
        }
    }

    public float GetStamina01()
    {
        return currentStamina / maxStamina;
    }
}
