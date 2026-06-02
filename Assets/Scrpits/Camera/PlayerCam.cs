using UnityEngine;

public class PlayerCam : MonoBehaviour
{

    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    void Start()
    {
        float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", sensX);
        SetSensitivity(savedSens);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

       
        yRotation = orientation.rotation.eulerAngles.y;
        xRotation = transform.localRotation.eulerAngles.x;

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
    void Update()
    {
        float mouseX=Input.GetAxisRaw("Mouse X") *Time.deltaTime * sensX;
        float mouseY=Input.GetAxisRaw("Mouse Y") *Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation= Quaternion.Euler(0, yRotation, 0);
    }
    public void SyncRotationWithCamera()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        xRotation = euler.x;
        yRotation = euler.y;
    }
    public void SetSensitivity(float newSens)
    {
        sensX = newSens;
        sensY = newSens;

        PlayerPrefs.SetFloat("MouseSensitivity", newSens);
        PlayerPrefs.Save();

    }
}
