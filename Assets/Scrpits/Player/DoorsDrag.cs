using UnityEngine;
using UnityEngine.UI;

public class DoorsDrag : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera cam;
    [Tooltip("Maksymalna odległość interakcji (ściany będą blokować!)", order = 1)]
    [SerializeField] float maxDistance = 3.5f;

    [Header("Crosshair UI")]
    [SerializeField] Image doorCrosshair;
    [SerializeField] Sprite openHandSprite;
    [SerializeField] Sprite closedHandSprite;

    [Header("Door Control")]
    [Tooltip("Zwiększ wartość, aby drzwi były lżejsze (np. 2.0 - 5.0). 1.0 = domyślnie.")]
    [SerializeField] float mouseDragStrength = 1f; // ✅ NOWE

    private Transform hoveredDoor;
    private Transform selectedDoor;
    private GameObject dragPointGameObject;
    private int leftDoor = 0;

    private bool isHoveringDoor = false;
    private bool isDragging = false;

    void Start()
    {
        if (doorCrosshair != null) doorCrosshair.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isDragging)
        {
            CheckHover();
            if (isHoveringDoor && Input.GetMouseButtonDown(0))
                StartDragging(hoveredDoor);
        }
        else
        {
            HandleDragging();
            if (Input.GetMouseButtonUp(0))
                StopDragging();
        }

        UpdateCrosshair();
    }

    private void CheckHover()
    {
        isHoveringDoor = false;
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxDistance))
        {
            if (hit.collider.GetComponent<HingeJoint>() != null)
            {
                isHoveringDoor = true;
                hoveredDoor = hit.transform;
            }
        }
    }

    private void UpdateCrosshair()
    {
        if (doorCrosshair == null) return;

        if (isDragging)
        {
            doorCrosshair.gameObject.SetActive(true);
            doorCrosshair.sprite = closedHandSprite;
        }
        else if (isHoveringDoor)
        {
            doorCrosshair.gameObject.SetActive(true);
            doorCrosshair.sprite = openHandSprite;
        }
        else
        {
            doorCrosshair.gameObject.SetActive(false);
        }
    }

    private void StartDragging(Transform door)
    {
        selectedDoor = door;
        isDragging = true;
    }

    private void StopDragging()
    {
        isDragging = false;

        if (selectedDoor != null)
        {
            HingeJoint joint = selectedDoor.GetComponent<HingeJoint>();
            if (joint != null)
            {
                JointMotor motor = joint.motor;
                motor.targetVelocity = 0;
                joint.motor = motor;
            }
        }

        selectedDoor = null;
        if (dragPointGameObject != null)
        {
            Destroy(dragPointGameObject);
            dragPointGameObject = null;
        }
    }

    private void HandleDragging()
    {
        if (selectedDoor == null) return;

        HingeJoint joint = selectedDoor.GetComponent<HingeJoint>();
        if (joint == null) return;

        if (dragPointGameObject == null)
        {
            dragPointGameObject = new GameObject("DragHelper");
            dragPointGameObject.transform.parent = selectedDoor;
            dragPointGameObject.transform.localPosition = Vector3.zero;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        dragPointGameObject.transform.position = ray.GetPoint(Vector3.Distance(selectedDoor.position, transform.position));
        dragPointGameObject.transform.rotation = selectedDoor.rotation;

        float delta = Mathf.Pow(Vector3.Distance(dragPointGameObject.transform.position, selectedDoor.position), 3);

        if (selectedDoor.GetComponent<MeshRenderer>() != null)
        {
            leftDoor = selectedDoor.GetComponent<MeshRenderer>().localBounds.center.x > selectedDoor.localPosition.x ? 1 : -1;
        }

        float speedMultiplier = 60000;
        JointMotor motor = joint.motor;

        // ✅ KLUCZOWE: Zdejmij limit siły, żeby drzwi nie były "ciężkie"
        motor.force = Mathf.Infinity;

        // ✅ Zastosuj mouseDragStrength do prędkości
        float baseVelocity = delta * speedMultiplier * Time.deltaTime * leftDoor * mouseDragStrength;

        if (Mathf.Abs(selectedDoor.parent.forward.z) > 0.5f)
        {
            motor.targetVelocity = dragPointGameObject.transform.position.x > selectedDoor.position.x
                ? -baseVelocity
                : baseVelocity;
        }
        else
        {
            motor.targetVelocity = dragPointGameObject.transform.position.z > selectedDoor.position.z
                ? -baseVelocity
                : baseVelocity;
        }

        joint.motor = motor;
    }
}