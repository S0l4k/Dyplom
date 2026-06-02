using UnityEngine;
using UnityEngine.UI;

public class DialogCrosshairHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Image dialogCrosshair;
    [SerializeField] private float maxDistance = 3f;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (dialogCrosshair != null) dialogCrosshair.gameObject.SetActive(false);
    }

    void Update()
    {
        if (dialogCrosshair == null || playerCamera == null) return;

        if (GameState.IsTalking)
        {
            dialogCrosshair.gameObject.SetActive(false);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool shouldShow = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            DialogActivator activator = hit.collider.GetComponent<DialogActivator>();
            if (activator == null)
                activator = hit.collider.GetComponentInParent<DialogActivator>();

            if (activator != null && activator.enabled && !activator.isTalking)
            {
                shouldShow = true;
            }
        }

        dialogCrosshair.gameObject.SetActive(shouldShow);
    }
}