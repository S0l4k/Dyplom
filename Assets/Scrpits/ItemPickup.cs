using UnityEngine;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Item";           
    public Transform handPosition;             
    public Vector3 localPositionOffset;   
    public Vector3 localRotationOffset;   
                                          

    [Header("UI")]
    public Canvas pickupCanvas;               
    public TMP_Text pickupText;                

    private Camera playerCamera;
    private bool canPickup = false;

    void Start()
    {
        if (pickupCanvas != null)
            pickupText.gameObject.SetActive(false);

        playerCamera = Camera.main;
    }

    void Update()
    {
        CheckForPickup();

        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    void CheckForPickup()
    {
        if (!playerCamera) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        float pickupRange = 3f;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (pickupCanvas != null)
                {
                    pickupText.gameObject.SetActive(true);
                    pickupText.text = $"Press E to pick up {itemName}";
                }
                canPickup = true;
                return;
            }
        }

        if (pickupCanvas != null)
            pickupText.gameObject.SetActive(false);

        canPickup = false;
    }
    void Pickup()
    {
        if (handPosition != null)
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
        }

        if (pickupCanvas != null)
            pickupCanvas.gameObject.SetActive(false);

        canPickup = false;
    }

}
