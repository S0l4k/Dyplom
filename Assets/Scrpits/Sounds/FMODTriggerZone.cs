using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODTriggerZone : MonoBehaviour
{
    public EventReference eventPath;
    public Transform soundSource;

    public bool stopOnExit = false;

    private EventInstance instance;
    private bool isPlaying = false;
    private bool shouldUpdatePosition = false;

    void Start()
    {
        if (soundSource == null)
            soundSource = transform;
    }

    void Update()
    {
        if (shouldUpdatePosition && instance.isValid())
        {
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(soundSource));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlaying) return;

        instance = RuntimeManager.CreateInstance(eventPath);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(soundSource));
        instance.start();

        shouldUpdatePosition = true;
        isPlaying = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!stopOnExit) return;

        StopEvent();
    }

    public void StopEvent()
    {
        if (!isPlaying) return;

        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }

        shouldUpdatePosition = false;
        isPlaying = false;

        Destroy(gameObject);
    }

    public void StopEventOnPickup()
    {
        StopEvent();
    }
}