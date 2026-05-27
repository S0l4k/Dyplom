using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
public class toiletInteract : MonoBehaviour
{
    public EventReference toiletSound;
    private Camera playerCamera;
    private bool canInteract = false;
    public Outline outline;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        playerCamera = Camera.main;
        if (outline != null) outline.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForInteraction();
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {

            AudioManager.Instance.PlaySFX(toiletSound, transform.position);
        }
    }
    void CheckForInteraction()
    {


        bool wasInteracting = canInteract;
        canInteract = false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        // Naprawione wykrywanie (obs³uga dzieci obiektu)
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            if (hit.collider.gameObject == gameObject || hit.transform.IsChildOf(transform))
            {
                canInteract = true;
                if (outline != null) outline.enabled = true;
                
            }
            else
            {
                if (outline != null) outline.enabled = false;
                
            }
        }
        else
        {
            if (outline != null) outline.enabled = false;
           
        }


    }
}
