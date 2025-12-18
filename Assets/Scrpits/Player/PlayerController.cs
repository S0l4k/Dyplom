using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float acceleration = 20f;

    [Header("Friction / Drag")]
    public float groundDrag = 6f;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask groundMask;
    private bool grounded;

    [Header("Sprint & Stamina")]
    public float maxStamina = 5f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 0.8f;
    private float currentStamina;
    private bool isSprinting;
    private bool isExhausted; 

    [Header("References")]
    public Transform orientation;
    public GameObject console;
    public Transform hand;
    public TMP_Text pickupText;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    private Rigidbody rb;
    private float currentSpeed;

    void Start()
    {
      
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentStamina = maxStamina;
    }

    void Update()
    {
        
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, groundMask);

        HandleInput();
        HandleStamina();
        SpeedControl();

        rb.linearDamping = grounded ? groundDrag : 0f;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && verticalInput > 0 && !isExhausted;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            console.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }    
       
        isSprinting = wantsToSprint && grounded && currentStamina > 0;

        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
    }
    

    private void HandleStamina()
    {
        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isSprinting = false;
                isExhausted = true; 
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
    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.Normalize();

        if (moveDirection.sqrMagnitude < 0.01f)
            return;

        RaycastHit hit;

        bool hasForwardHit = Physics.Raycast(
            transform.position,
            moveDirection,
            out hit,
            0.6f
        );

        if (hasForwardHit)
        {
           
            if ((groundMask.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
              
                moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal).normalized;
            }
        }

        
        float slopeAngle = 0f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, playerHeight * 0.6f))
        {
            slopeAngle = Vector3.Angle(slopeHit.normal, Vector3.up);

            
            if (slopeAngle > 5f)
            {
                
                float slopeBoost = 1f + (slopeAngle / 45f);
                moveDirection *= slopeBoost;
            }

        
            if (slopeAngle < 45f && grounded)
            {
                rb.AddForce(Vector3.up * 4f, ForceMode.Acceleration);
            }
        }

      
        Vector3 force = moveDirection * acceleration * (currentSpeed / walkSpeed);
        rb.AddForce(force, ForceMode.Acceleration);
    }




    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (playerHeight * 0.5f + 0.3f));
    }

    public float GetStamina01()
    {
        return currentStamina / maxStamina;
    }
}
