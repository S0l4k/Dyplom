using UnityEngine;

public class PlayerCam : MonoBehaviour
{

    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

       
        yRotation = orientation.rotation.eulerAngles.y;
        xRotation = transform.localRotation.eulerAngles.x;

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
    // Update is called once per frame
    void Update()
    {
        float mouseX=Input.GetAxisRaw("Mouse X") *Time.deltaTime * sensX;
        float mouseY=Input.GetAxisRaw("Mouse Y") *Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation= Quaternion.Euler(0, yRotation, 0);
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Debug.Log(">>> CURRENT ROT " + transform.rotation.eulerAngles);
        }
    }
    public void SyncRotationWithCamera()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        xRotation = euler.x;
        yRotation = euler.y;
    }
}
