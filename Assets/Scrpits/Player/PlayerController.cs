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

    [Header("Crouch")]
    public KeyCode sneakKey = KeyCode.LeftControl;
    public float sneakHeight = 1f;
    public Vector3 sneakCenter = new Vector3(0, 0.5f, 0);
    public float sneakSpeedMultiplier = 0.5f;

    [Header("Crouch Check")]
    public LayerMask obstacleMask;

    [Header("Audio")]
    private PlayerSound playerSound;
    private float lastStepTime;
    public float walkStepInterval = 0.22f;
    public float sprintStepInterval = 0.13f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.1f;

    private CharacterController controller;
    private Vector3 velocity;
    private float currentStamina;
    private bool isSprinting;
    public bool isSneaking = false;

    private float normalHeight;
    private Vector3 normalCenter;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerSound = GetComponent<PlayerSound>();
        currentStamina = maxStamina;

        normalHeight = controller.height;
        normalCenter = controller.center;
    }

    private void Update()
    {
        HandleInput();
        HandleStamina();
        HandleCrouch();
        Move();
        ApplyGravity();
        HandleFootsteps();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            pauseMenu.OpenConsole();
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = (orientation.forward * v + orientation.right * h).normalized;
        float speed = isSprinting ? sprintSpeed : walkSpeed;
        if (isSneaking) speed *= sneakSpeedMultiplier;

        controller.Move(move * speed * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        if (Input.GetKey(sneakKey))
        {
            if (!isSneaking)
                Crouch();
        }
        else
        {
            if (isSneaking)
                TryStandUp();
        }
    }

    private void Crouch()
    {
        float heightDiff = controller.height - sneakHeight;
        controller.height = sneakHeight;
        controller.center = sneakCenter;
        controller.Move(Vector3.down * heightDiff / 2f); // bezpieczne opuszczenie
        isSneaking = true;
    }

    private void TryStandUp()
    {
        // Sprawdzenie przestrzeni nad głową
        float radius = controller.radius * 0.9f;
        Vector3 bottom = transform.position + controller.center - Vector3.up * (controller.height / 2f - radius);
        Vector3 top = bottom + Vector3.up * (normalHeight - sneakHeight);
        if (Physics.CheckCapsule(bottom, top, radius, obstacleMask))
            return; // nie wstawaj, coś nad głową

        float heightDiff = normalHeight - controller.height;
        controller.height = normalHeight;
        controller.center = normalCenter;
        controller.Move(Vector3.up * heightDiff / 2f); // bezpieczne podniesienie
        isSneaking = false;
    }

    private void HandleFootsteps()
    {
        if (!IsGrounded()) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f) return;

        float interval = isSprinting ? sprintStepInterval : walkStepInterval;
        if (isSneaking) interval *= 2f;

        if (Time.time - lastStepTime >= interval)
        {
            playerSound.PlayFootstep();
            lastStepTime = Time.time;
        }
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance);
    }

    private void HandleStamina()
    {
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift) && !isSneaking && currentStamina > 0;
        isSprinting = wantsSprint;

        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isSprinting = false;
            }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }

    public float GetStamina01() => currentStamina / maxStamina;
}
