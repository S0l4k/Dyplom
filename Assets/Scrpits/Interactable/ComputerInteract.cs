using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class ComputerInteract : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform computerViewPoint;
    public TMP_Text interactText;

    [Header("UI")]
    public Canvas computerCanvas;  
    public Canvas playerCanvas;    

    [Header("Settings")]
    public float useRange = 3f;
    public float moveSpeed = 3f;
    public float returnSpeed = 3f; 
    public LayerMask computerMask;

    private bool isUsingComputer = false;
    private bool canUse = false;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    private MonoBehaviour playerMovementScript;
    private MonoBehaviour playerCamScript;

    void Start()
    {
        if (computerCanvas != null)
            computerCanvas.gameObject.SetActive(false);

        if (interactText != null)
            interactText.gameObject.SetActive(false);

        playerMovementScript = FindObjectOfType<PlayerController>();
        playerCamScript = FindObjectOfType<PlayerCam>();
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
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, useRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                interactText.gameObject.SetActive(true);
                interactText.text = "Press E to use computer";
                canUse = true;
                return;
            }
        }

        interactText.gameObject.SetActive(false);
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
            yield return null;
        }

        if (computerCanvas != null)
            computerCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public IEnumerator ExitComputerSmooth()
    {
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
}
