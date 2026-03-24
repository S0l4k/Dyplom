using UnityEngine;
using FMODUnity;

public class LightSwitch : MonoBehaviour
{
    private Camera playerCamera;
    private bool canClick = false;
    public GameObject lightOb;
    [SerializeField] private EventReference flashlightEvent;
    public Outline outline;
    void Start()
    {
        playerCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForRange();
        if(canClick && Input.GetKeyDown(KeyCode.E))
        { Click(); }
    }

    void CheckForRange()
    {
        if (!playerCamera)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        float clickRange = 3f;

        if (Physics.Raycast(ray, out hit, clickRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (outline != null) outline.enabled = true;  // ✅ Włącz
                canClick = true;
                return;
            }
            // ❌ Raycast trafił, ale w COŚ INNEGO – wyłącz outline
            canClick = false;
            if (outline != null) outline.enabled = false;
        }
        else
        {
            // ❌ Raycast NIC nie trafił – wyłącz outline
            canClick = false;
            if (outline != null) outline.enabled = false;
        }

    }

    void Click()
    {
        if (lightOb == null)
            return;

        RuntimeManager.PlayOneShot(flashlightEvent);
        lightOb.SetActive(!lightOb.activeSelf);
    }
}
