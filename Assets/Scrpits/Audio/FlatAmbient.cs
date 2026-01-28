using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FlatAmbient : MonoBehaviour
{
    [SerializeField] private EventReference apartmentAmbientEvent;

    private EventInstance ambientInstance;
    private bool isPlaying;

    private void Start()
    {
        ambientInstance = RuntimeManager.CreateInstance(apartmentAmbientEvent);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isPlaying) return;

        ambientInstance.set3DAttributes(
            RuntimeUtils.To3DAttributes(gameObject)
        );
        ambientInstance.start();
        isPlaying = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isPlaying) return;

        ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        isPlaying = false;
    }

    private void OnDestroy()
    {
        ambientInstance.release();
    }
}
