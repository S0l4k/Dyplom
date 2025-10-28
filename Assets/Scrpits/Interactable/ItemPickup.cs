using UnityEngine;
using TMPro;
using Commands;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Item";
    public Transform handPosition;
    public Vector3 localPositionOffset;
    public Vector3 localRotationOffset;

    [Header("UI")]
    public TMP_Text pickupText;

    private Camera playerCamera;
    private bool canPickup = false;
    private bool isHeld = false;
    private bool isHidden = false;

    private static bool playerHasItem = false;
    private static ItemPickup currentTarget = null;

    private Flashlight flashlight;

    void Start()
    {
        playerCamera = Camera.main;

        if (pickupText == null)
            pickupText = FindObjectOfType<TMP_Text>();

        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        flashlight = GetComponent<Flashlight>();
    }

    void Update()
    {
        if (!isHeld)
            CheckForPickup();
        else
            ShowItemUI();

        if (canPickup && !playerHasItem && Input.GetKeyDown(KeyCode.E))
            Pickup();

        if (isHeld && flashlight == null && Input.GetKeyDown(KeyCode.G))
            Drop();

        if (isHeld && flashlight != null && Input.GetKeyDown(KeyCode.F))
            ToggleHidden();
    }

    void CheckForPickup()
    {
        if (!playerCamera || pickupText == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        float pickupRange = 3f;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.gameObject == gameObject && !playerHasItem)
            {
                currentTarget = this;
                ShowPickupText();
                canPickup = true;
                return;
            }
        }

        if (currentTarget == this)
        {
            HidePickupText();
            currentTarget = null;
        }

        canPickup = false;
    }

    [Command("ShowText", "dasdasdasda")]
    public void ShowPickupText()
    {
        pickupText.gameObject.SetActive(true);
        pickupText.text = $"Press E to pick up {itemName}";
    }

    void ShowItemUI()
    {
        if (pickupText == null || !playerHasItem)
            return;

        if (flashlight != null)
        {
            pickupText.gameObject.SetActive(true);
            pickupText.text = isHidden
                ? $"Press F to take out {itemName}"
                : $"Press F to hide {itemName}";
        }
        else
        {
            pickupText.gameObject.SetActive(true);
            pickupText.text = $"Press G to drop {itemName}";
        }
    }

    void HidePickupText()
    {
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
    }

    void Pickup()
    {
        transform.SetParent(handPosition);
        transform.localPosition = localPositionOffset;
        transform.localRotation = Quaternion.Euler(localRotationOffset);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        isHeld = true;
        playerHasItem = true;

        HidePickupText();
        currentTarget = null;
        canPickup = false;

        if (flashlight != null)
        {
            flashlight.TurnOn();
            isHidden = false;
        }
    }

    void Drop()
    {
        transform.SetParent(null);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        isHeld = false;
        playerHasItem = false;

        HidePickupText();
    }

    void ToggleHidden()
    {
        if (flashlight == null)
            return;

        if (isHidden)
        {
            transform.SetParent(handPosition);
            transform.localPosition = localPositionOffset;
            transform.localRotation = Quaternion.Euler(localRotationOffset);
            flashlight.TurnOn();
            isHidden = false;
        }
        else
        {
            flashlight.TurnOff();
            transform.SetParent(null);

            transform.position = new Vector3(0, -1000, 0);
            isHidden = true;
        }

        HidePickupText();
    }
}