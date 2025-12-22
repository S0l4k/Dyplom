using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Commands;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Item";
    public Transform handPosition;
    public Vector3 localPositionOffset;
    public Vector3 localRotationOffset;

    [Header("FMOD Trigger Link")]
    public FMODTriggerZone linkedTrigger;

    [Header("Pickup UI (on look)")]
    public TMP_Text pickupText;

    [Header("Flashlight UI (after pickup)")]
    public GameObject flashlightUIRoot;
    public Image flashlightUIImage;
    public TMP_Text flashlightUIText;
    public Sprite flashlightOnSprite;
    public Sprite flashlightOffSprite;

    private Camera playerCamera;
    private bool canPickup = false;
    private bool isHeld = false;
    private bool isHidden = false;
    public bool isFlashlight = false;

    private static bool playerHasItem = false;
    private static ItemPickup currentTarget = null;

    private Flashlight flashlight;

    void Start()
    {
        playerCamera = Camera.main;

        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        if (flashlightUIRoot != null)
            flashlightUIRoot.SetActive(false);

        flashlight = GetComponent<Flashlight>();

        if (flashlightUIImage != null && flashlightOffSprite != null)
            flashlightUIImage.sprite = flashlightOffSprite;
    }

    void Update()
    {
        if (!isHeld && !playerHasItem)
            CheckForPickup();

        if (isHeld)
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

    [Command("ShowText", "Shows pickup text")]
    public void ShowPickupText()
    {
        if (pickupText != null)
        {
            pickupText.gameObject.SetActive(true);
            pickupText.text = $"Press E to pick up {itemName}";
        }
    }

    void ShowItemUI()
    {
        if (!isHeld)
            return;

        if (flashlight != null)
        {
            if (pickupText != null)
                pickupText.gameObject.SetActive(false);

            if (flashlightUIRoot != null)
            {
                flashlightUIRoot.SetActive(true);

                if (flashlightUIText != null)
                    flashlightUIText.text = "Press F";

                if (flashlightUIImage != null)
                    flashlightUIImage.sprite = isHidden ? flashlightOffSprite : flashlightOnSprite;
            }
        }
        else
        {
            if (pickupText != null)
            {
                pickupText.gameObject.SetActive(true);
                pickupText.text = $"Press G to drop {itemName}";
            }
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

            if (flashlightUIRoot != null)
                flashlightUIRoot.SetActive(true);

            if (flashlightUIImage != null)
                flashlightUIImage.sprite = flashlightOnSprite;

            if (flashlightUIText != null)
                flashlightUIText.text = "Press F";
        }

        if (linkedTrigger != null)
        {
            Debug.Log("[ItemPickup] Picking up item -> stopping linked FMOD trigger.");
            linkedTrigger.StopEvent();
        }
        if (isFlashlight)
        {
            QuestManager.Instance.CompleteQuest("Pick up flashlight");
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

        if (flashlightUIRoot != null)
            flashlightUIRoot.SetActive(false);
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

        if (flashlightUIImage != null)
            flashlightUIImage.sprite = isHidden ? flashlightOffSprite : flashlightOnSprite;
    }
}