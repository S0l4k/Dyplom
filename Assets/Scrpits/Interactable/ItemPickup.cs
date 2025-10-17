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

    private static bool playerHasItem = false;    
    private static ItemPickup currentTarget = null; 
    void Start()
    {
        playerCamera = Camera.main;

        if (pickupText == null)
            pickupText = FindObjectOfType<TMP_Text>();

        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isHeld) 
            CheckForPickup();
        else
            ShowDropText();

        
        if (canPickup && !playerHasItem && Input.GetKeyDown(KeyCode.E))
            Pickup();

        
        if (isHeld && Input.GetKeyDown(KeyCode.G))
            Drop();
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

    void ShowDropText()
    {
        if (pickupText != null && playerHasItem)
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
}
