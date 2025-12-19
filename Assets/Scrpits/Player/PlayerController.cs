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

    [Header("References")]
    public Transform orientation;
    public GameObject console;
    public Transform hand;
    public TMP_Text pickupText;

    private CharacterController controller;
    private Vector3 velocity;
    private float currentStamina;
    private bool isSprinting;
    private bool isExhausted;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentStamina = maxStamina;
    }

    void Update()
    {
        HandleInput();
        HandleStamina();
        ApplyGravity();
        Move();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            console.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f; // trzyma przy ziemi

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
