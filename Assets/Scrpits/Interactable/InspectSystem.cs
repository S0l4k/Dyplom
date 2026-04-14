using UnityEngine;

public class InspectSystem : MonoBehaviour
{
    public float rotationSpeed = 100f;

    // ✅ Dodaj to:
    private Transform _target;

    // ✅ Dodaj tę metodę:
    public void SetTarget(Transform target) => _target = target;

    void Update()
    {
        // ✅ Jeśli nie ma targetu, nie rób nic
        if (_target == null) return;

        if (Input.GetMouseButtonDown(0))
            previousMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            float rotX = delta.y * rotationSpeed * Time.deltaTime;
            float rotY = -delta.x * rotationSpeed * Time.deltaTime;

            Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);
            _target.rotation = rotation * _target.rotation;  // ✅ Obracamy target, nie siebie!

            previousMousePosition = Input.mousePosition;
        }
    }

    // ✅ To już masz, zostaw:
    private Vector3 previousMousePosition;
}