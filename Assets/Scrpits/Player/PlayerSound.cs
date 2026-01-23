using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] private EventReference footstepEvent;
    private EventInstance footstepInstance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        footstepInstance = RuntimeManager.CreateInstance(footstepEvent);
    }
    public void PlayFootstep()
    {
        int surface = GetSurfaceType();
        footstepInstance.setParameterByName("Surface", surface);
        footstepInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        footstepInstance.start();
    }


    int GetSurfaceType()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            switch (hit.collider.tag)
            {
                case "Floor": return 0;
                case "Stone": return 1;
               
             
            }
        }

        return 0;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            RuntimeManager.PlayOneShot(footstepEvent);
            Debug.Log("PLAY ONE SHOT");
        }
    }

}
