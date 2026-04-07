using FMODUnity;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private Transform groundCheck;

    public void PlayFootstep()
    {
        int surface = GetSurfaceType();
        AudioManager.Instance.PlayFootstep(footstepEvent, surface, transform.position);
    }

    int GetSurfaceType()
    {
        // Prosty raycast - IsGrounded() już zagwarantował, że coś trafimy
        if (Physics.Raycast(groundCheck.position, Vector3.down, out RaycastHit hit, 0.5f))
        {
            switch (hit.collider.tag)
            {
                case "Floor": return 0;
                case "Stone": return 1;
                    // Dodaj więcej tagów w razie potrzeby
            }
        }
        return 0; // Domyślna powierzchnia
    }
}