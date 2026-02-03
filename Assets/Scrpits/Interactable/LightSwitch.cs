using UnityEngine;
using FMODUnity;

public class LightSwitch : MonoBehaviour
{
    private Camera playerCamera;
    private bool canClick = false;
    public GameObject lightOb;
    [SerializeField] private EventReference flashlightEvent;
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
                canClick= true;
                return;
            }
            canClick = false;
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
