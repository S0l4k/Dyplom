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
    public bool isFlashlight = false;


    [Header("Pickup UI (on look)")]
    public TMP_Text pickupText;

    [Header("Flashlight UI (after pickup)")]
    public GameObject flashlightUIRoot;
    public Image flashlightUIImage;
    public TMP_Text flashlightUIText;
    public Sprite flashlightOnSprite;
    public Sprite flashlightOffSprite;

    public StairLoop stairLoop;


    private Camera playerCamera;
    private bool canPickup = false;
    private bool isHeld = false;
    private bool isHidden = false;

    private static ItemPickup heldItem = null;
    private static ItemPickup heldFlashlight = null;
    private static ItemPickup currentTarget = null;

    private Flashlight flashlight;

    void Start()
    {
        if (isFlashlight)
            heldFlashlight = null;

        if (!isFlashlight)
            heldItem = null;

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
        if (!isHeld)
            CheckForPickup();

        if (isHeld)
            ShowItemUI();

        if (canPickup && Input.GetKeyDown(KeyCode.E))
            Pickup();

        if (isHeld && !isFlashlight && Input.GetKeyDown(KeyCode.G))
            Drop();

        if (isHeld && isFlashlight && Input.GetKeyDown(KeyCode.F))
            ToggleHidden();
    }

    void CheckForPickup()
    {
        if (!playerCamera || pickupText == null)
            return;

        if (isFlashlight && heldFlashlight != null) return;
        if (!isFlashlight && heldItem != null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                currentTarget = this;
                ShowPickupText();
                canPickup = true;
                return;
            }
        }

        HidePickupText();
        canPickup = false;
    }

    [Command("ShowText", "Shows pickup text")]

    public void ShowPickupText()
    {
        pickupText.gameObject.SetActive(true);
        pickupText.text = $"Press E to pick up {itemName}";
    }

    void HidePickupText()
    {
        pickupText.gameObject.SetActive(false);
    }

    void Pickup()
    {
        if (isFlashlight && heldFlashlight != null)
        {
            Debug.Log("[Pickup] Flashlight already held – skipping pickup");
            return;
        }

        if (!isFlashlight && heldItem != null)
            return;

        
        if (isFlashlight)
        {
            Debug.Log("[Pickup] Flashlight picked up -> sending quest");
            QuestManager.Instance.CompleteQuest("Pick up flashlight");
        }

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
        canPickup = false;
        HidePickupText();

        if (isFlashlight)
            heldFlashlight = this;
        else
            heldItem = this;

        if (flashlight != null)
        {
            flashlight.TurnOn();
            isHidden = false;
            flashlightUIRoot.SetActive(true);
            flashlightUIImage.sprite = flashlightOnSprite;
            flashlightUIText.text = "Press F";
        }
        if (itemName == "Food")
        {
            // Włączamy loop sequence w GameState
            GameState.LoopSequenceActive = true;

            // Jeśli masz przypisany StairLoop, zresetuj licznik
            if (stairLoop != null)
            {
                stairLoop.loopCount = 0;
                Debug.Log("[ItemPickup] Loop sequence activated after picking up food!");
            }
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
        heldItem = null;

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

        flashlightUIImage.sprite = isHidden ? flashlightOffSprite : flashlightOnSprite;
    }

    void ShowItemUI()
    {
        if (!isFlashlight)
        {
            pickupText.gameObject.SetActive(true);
            pickupText.text = $"Press G to drop {itemName}";
        }
    }
}
