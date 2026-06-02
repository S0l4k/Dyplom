using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Commands;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Item";
    public Transform handPosition;
    public Vector3 localPositionOffset;
    public Vector3 localRotationOffset;
    public bool isFlashlight = false;

    [Header("Pickup UI (on look)")]
    public GameObject pickupText;

    [Header("Flashlight UI (after pickup)")]
    public GameObject flashlightUIRoot;
    public Image flashlightUIImage;
    public TMP_Text flashlightUIText;
    public Sprite flashlightOnSprite;
    public Sprite flashlightOffSprite;

    [Header("Outline")]
    public Outline outline;

    public StairLoop stairLoop;

    [Header("Light & Medicine System")]
    public LightController lightController;
    public GameObject medicine;

    [Header("Attic Quest Integration")]
    public AtticQuestController atticQuestController;

    private static ItemPickup heldItem = null;
    private static ItemPickup heldFlashlight = null;
    private static ItemPickup currentTarget = null;

    private static ItemPickup _heldAtticCandle = null;

    private Camera playerCamera;
    private bool canPickup = false;
    public bool isHeld = false;
    private bool isHidden = false;

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

        if (outline != null) outline.enabled = false;
    }

    void Update()
    {
        if (!isHeld)
            CheckForPickup();

     

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

        if (isFlashlight && !GameState.DemonRespawnedInApartment)
        {
            HidePickupText();
            canPickup = false;
            if (outline != null) outline.enabled = false;
            return;
        }

        if (GameState.InteractionsLocked && !isHeld)
        {
            if (outline != null) outline.enabled = false;
            HidePickupText();
            canPickup = false;
            Ray seizureRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(seizureRay, out RaycastHit seizureHit, 3f)
                && seizureHit.collider.gameObject == gameObject)
            {
                GameState.TriggerSeizureEffect = true;
            }
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                currentTarget = this;
                if (outline != null) outline.enabled = true;
                ShowPickupText();
                canPickup = true;
                return;
            }
        }

        HidePickupText();
        canPickup = false;
        if (outline != null) outline.enabled = false;
    }

    [Command("ShowText", "Shows pickup text")]
    public void ShowPickupText()
    {
        pickupText.gameObject.SetActive(true);
        
    }

    void HidePickupText()
    {
        pickupText.gameObject.SetActive(false);
    }

    void Pickup()
    {
        if (isFlashlight && !GameState.DemonRespawnedInApartment)
        {
            return;
        }

        if (isFlashlight && heldFlashlight != null)
        {
            return;
        }

        if (!isFlashlight && heldItem != null)
            return;

        if (outline != null) outline.enabled = false;

        if (isFlashlight)
        {
            QuestManager.Instance.CompleteQuest("Find flashlight");
            StartCoroutine(LightsFlickerAndTurnOff());
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
            GameState.LoopSequenceActive = true;
            if (stairLoop != null)
            {
                stairLoop.loopCount = 0;
            }
        }

        if (atticQuestController != null && gameObject.CompareTag("AtticCandle"))
        {
            _heldAtticCandle = this;
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

        if (_heldAtticCandle == this)
            _heldAtticCandle = null;

        if (isFlashlight)
            heldFlashlight = null;
        else
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

    private IEnumerator LightsFlickerAndTurnOff()
    {
        if (lightController == null)
        {
            yield break;
        }


        for (int i = 0; i < 3; i++)
        {
            lightController.TurnOffAllLights();
            yield return new WaitForSeconds(0.15f);
            lightController.RestoreLights();
            yield return new WaitForSeconds(0.15f + (i * 0.1f));
        }

        lightController.TurnOffAllLights();

        yield return new WaitForSeconds(1f);
        if (medicine != null)
        {
            QuestManager.Instance.AddQuest("Find meds");
            medicine.SetActive(true);
        }
    }
    public static bool HeldItemIsAtticCandle()
    {
        return _heldAtticCandle != null && _heldAtticCandle.isHeld;
    }
}