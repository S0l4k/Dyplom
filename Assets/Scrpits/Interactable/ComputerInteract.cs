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
    private ComputerCursor computerCursor; // ✅ Tylko referencja - nie wymaga ręcznego przypisania

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

        // ✅ AUTOMATYCZNE ZNALEZIENIE KURSORA (działa nawet z wyłączonym canvasem!)
        if (computerCanvas != null)
            computerCursor = computerCanvas.GetComponent<ComputerCursor>();
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

        // ✅ ORYGINALNA LOGIKA BEZ LAYER MASK (działała wcześniej - zostawiam bez zmian)
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
            RuntimeManager.PlayOneShot(turnOnEvent); // ✅ ORYGINAŁ: dźwięk w pętli
            yield return null;
        }

        if (computerCanvas != null)
            computerCanvas.gameObject.SetActive(true);

        // ✅ DODANE: aktywacja customowego kursora (jeśli istnieje)
        if (computerCursor != null)
            computerCursor.Enable();
        else
        {
            // Fallback na standardowy kursor (jak w oryginale)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public IEnumerator ExitComputerSmooth()
    {
        RuntimeManager.PlayOneShot(turnOffEvent);

        // ✅ DODANE: dezaktywacja customowego kursora (jeśli istnieje)
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

        // ✅ ORYGINALNE ZACHOWANIE KURSORA
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Click()
    {
        RuntimeManager.PlayOneShot(clickEvent);
    }
}