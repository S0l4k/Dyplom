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

    void Update()
    {
        CheckForRange();
        if (canClick && Input.GetKeyDown(KeyCode.E))
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
                if (outline != null) outline.enabled = true;
                canClick = true;
                return;
            }
            canClick = false;
            if (outline != null) outline.enabled = false;
        }
        else
        {
            canClick = false;
            if (outline != null) outline.enabled = false;
        }
    }

    void Click()
    {
        if (lightOb == null)
            return;

        // ✅ ZAMIENIONE: RuntimeManager -> AudioManager
        AudioManager.Instance.PlaySFX(flashlightEvent);

        lightOb.SetActive(!lightOb.activeSelf);
    }
}