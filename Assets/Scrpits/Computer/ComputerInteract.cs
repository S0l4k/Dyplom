using FMODUnity;
using System.Collections;
using TMPro;
using UnityEngine;

public class ComputerInteract : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform computerViewPoint;
    public TMP_Text interactText;
    public GameObject crossair;
    public Outline outline;

    [Header("UI")]
    public Canvas computerCanvas;
    public Canvas playerCanvas;


    [Header("Settings")]
    public float useRange = 3f;
    public float moveSpeed = 3f;
    public float returnSpeed = 3f;

    public bool isUsingComputer = false;
    private bool canUse = false;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    private MonoBehaviour playerMovementScript;
    private MonoBehaviour playerCamScript;
    private ComputerCursor computerCursor; 

    [Header("Audio")]
    [SerializeField] private EventReference turnOnEvent;
    [SerializeField] private EventReference turnOffEvent;
    [SerializeField] private EventReference clickEvent;

    void Start()
    {
        if (computerCanvas != null)
            computerCanvas.gameObject.SetActive(false);

        if (interactText != null)
            interactText.gameObject.SetActive(false);

        playerMovementScript = FindObjectOfType<PlayerController>();
        playerCamScript = FindObjectOfType<PlayerCam>();

        if (computerCanvas != null)
            computerCursor = computerCanvas.GetComponent<ComputerCursor>();
        if (outline != null) outline.enabled = false;
    }

    void Update()
    {
        CheckForComputer();

        if (canUse && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(UseComputer());
    }

    public void EscapeComputer()
    {
        StartCoroutine(ExitComputerSmooth());
    }

    void CheckForComputer()
    {
        if (GameState.InteractionsLocked)
        {
            crossair.SetActive(false);
            if (outline != null) outline.enabled = false;
            canUse = false;

            Ray seizureRay = new Ray(playerCamera.position, playerCamera.forward);
            if (Physics.Raycast(seizureRay, out RaycastHit seizureHit, useRange)
                && seizureHit.collider.gameObject == gameObject)
            {
                GameState.TriggerSeizureEffect = true;
            }
            return;
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, useRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                crossair.SetActive(true);
                canUse = true;
                if (outline != null) outline.enabled = true;
                return;
            }
        }

        crossair.SetActive(false);
        if (outline != null) outline.enabled = false;
        canUse = false;
    }

    IEnumerator UseComputer()
    {
        if (playerCanvas != null)
            playerCanvas.gameObject.SetActive(false);

        isUsingComputer = true;
        canUse = false;
        interactText.gameObject.SetActive(false);

        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (playerCamScript != null) playerCamScript.enabled = false;

        originalCamPosition = playerCamera.position;
        originalCamRotation = playerCamera.rotation;

        float elapsed = 0f;
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * moveSpeed;
            playerCamera.position = Vector3.Lerp(startPos, computerViewPoint.position, elapsed);
            playerCamera.rotation = Quaternion.Slerp(startRot, computerViewPoint.rotation, elapsed);
            RuntimeManager.PlayOneShot(turnOnEvent); 
            yield return null;
        }

        if (computerCanvas != null)
            computerCanvas.gameObject.SetActive(true);

        if (computerCursor != null)
            computerCursor.Enable();
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public IEnumerator ExitComputerSmooth()
    {
        RuntimeManager.PlayOneShot(turnOffEvent);

        if (computerCursor != null)
            computerCursor.Disable();

        if (computerCanvas != null)
            computerCanvas.gameObject.SetActive(false);

        float elapsed = 0f;
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * returnSpeed;
            playerCamera.position = Vector3.Lerp(startPos, originalCamPosition, elapsed);
            playerCamera.rotation = Quaternion.Slerp(startRot, originalCamRotation, elapsed);
            yield return null;
        }

        isUsingComputer = false;

        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (playerCamScript != null) playerCamScript.enabled = true;

        if (playerCanvas != null)
            playerCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Click()
    {
        RuntimeManager.PlayOneShot(clickEvent);
    }
}