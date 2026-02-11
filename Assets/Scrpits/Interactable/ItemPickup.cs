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
    public TMP_Text pickupText;

    [Header("Flashlight UI (after pickup)")]
    public GameObject flashlightUIRoot;
    public Image flashlightUIImage;
    public TMP_Text flashlightUIText;
    public Sprite flashlightOnSprite;
    public Sprite flashlightOffSprite;

    public StairLoop stairLoop;

    // ✅ NOWE POLA DLA SYSTEMU ŚWIATŁA/LEKÓW
    [Header("Light & Medicine System")]
    public LightController lightController;        // ✅ Twój LightController
    public GameObject medicine;                    // ✅ Obiekt z lekami (początkowo ukryty)

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

        // ✅ KLUCZOWA BLOKADA: NIE POKAZUJ TEKSTU PODNOSZENIA LATARKI PRZED RESPAWNEM DEMONA
        if (isFlashlight && !GameState.DemonRespawnedInApartment)
        {
            HidePickupText();
            canPickup = false;
            return;
        }

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
        // ✅ KLUCZOWA BLOKADA: NIE POZWÓL PODNIEŚĆ LATARKI PRZED RESPAWNEM DEMONA
        if (isFlashlight && !GameState.DemonRespawnedInApartment)
        {
            Debug.Log("[Pickup] ❌ Cannot pick up flashlight yet – demon not respawned in apartment");
            return;
        }

        if (isFlashlight && heldFlashlight != null)
        {
            Debug.Log("[Pickup] Flashlight already held – skipping pickup");
            return;
        }

        if (!isFlashlight && heldItem != null)
            return;

        // ✅ TUTAJ JEST JUŻ BEZPIECZNIE – demon się zrespawnował
        if (isFlashlight)
        {
            Debug.Log("[Pickup] ✅ Flashlight picked up AFTER demon respawn");
            QuestManager.Instance.CompleteQuest("Pick up flashlight");

            // ✅ KLUCZOWE: GASNIE ŚWIATŁA PO PODNIENIU LATARKI
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

    // ✅ NOWA METODA: GASZENIE ŚWIATŁA Z EFEKTEM TYPIENIA
    private IEnumerator LightsFlickerAndTurnOff()
    {
        if (lightController == null)
        {
            Debug.LogError("[ItemPickup] LightController nie przypisany – pomijam gaszenie");
            yield break;
        }

        Debug.Log("[ItemPickup] 🔌 Zaczynam gaszenie świateł po podniesieniu latarki...");

        // ✅ Efekt typkania (3x)
        for (int i = 0; i < 3; i++)
        {
            lightController.TurnOffAllLights();     // ✅ Gasimy
            yield return new WaitForSeconds(0.15f);
            lightController.RestoreLights();        // ✅ Włączamy z powrotem
            yield return new WaitForSeconds(0.15f + (i * 0.1f)); // ✅ Dłuższe pauzy
        }

        // ✅ Ostateczne zgaszenie
        lightController.TurnOffAllLights();
        Debug.Log("[ItemPickup] 💡 Wszystkie światła zgaszone");

        // ✅ SPAWN LEKÓW PO 1 SEKUNDZIE
        yield return new WaitForSeconds(1f);
        if (medicine != null)
        {
            medicine.SetActive(true);
            Debug.Log("[ItemPickup] 💊 Leki aktywowane");
        }
    }
}