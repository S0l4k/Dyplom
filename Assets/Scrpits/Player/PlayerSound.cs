using FMODUnity;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private Transform groundCheck;

    public void PlayFootstep()
    {
        int surface = GetSurfaceType();
        var instance = RuntimeManager.CreateInstance(footstepEvent);
        instance.setParameterByName("Surface", surface);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        instance.start();
        instance.release();
    }

    int GetSurfaceType()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, 1.2f))

        {
            Debug.Log("Surface: " + hit.collider.name);

            switch (hit.collider.tag)
            {
                case "Floor": return 0;
                case "Stone": return 1;
            }
        }
        return 0;
    }
}
