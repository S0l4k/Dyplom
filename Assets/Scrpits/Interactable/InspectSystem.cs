using UnityEngine;

public class InspectSystem : MonoBehaviour
{
    public float rotationSpeed = 100f;
    private Transform _target;
    private bool isInspecting = false;
    public void SetTarget(Transform target)
    {
        if (target == null) return;

        _target = target;
        isInspecting = true;

        GameState.IsInspecting = true;
    }

    public void ExitInspectMode()
    {
        if (!isInspecting) return;

        _target = null;
        isInspecting = false;

        GameState.IsInspecting = false;
        GameState.IsInFlashback = true;
    }

    public void CloseInspectOnly()
    {
        if (!isInspecting) return;

        _target = null;
        isInspecting = false;
        GameState.IsInspecting = false;
    }

    void Update()
    {
        if (_target == null) return;

        if (Input.GetMouseButtonDown(0))
            previousMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            float rotX = delta.y * rotationSpeed * Time.deltaTime;
            float rotY = -delta.x * rotationSpeed * Time.deltaTime;

            Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);
            _target.rotation = rotation * _target.rotation;

            previousMousePosition = Input.mousePosition;
        }
    }

    private Vector3 previousMousePosition;
}