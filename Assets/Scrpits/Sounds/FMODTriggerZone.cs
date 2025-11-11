using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODTriggerZone : MonoBehaviour
{
    [Header("FMOD Event")]
    [EventRef] public string eventPath = "event:/LoudWhisper";
    public Transform soundSource;

    [Header("Optional")]
    public bool stopOnPlayerExit = false;

    private EventInstance eventInstance;
    private bool isPlaying = false;

    void Start()
    {
        if (soundSource == null) soundSource = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlaying) return;

        eventInstance = RuntimeManager.CreateInstance(eventPath);
        eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(soundSource));
        eventInstance.start();

        isPlaying = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!stopOnPlayerExit) return;

        StopEvent();
    }

    public void StopEvent()
    {
        if (!isPlaying)
        {
            return;
        }

        if (eventInstance.isValid())
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            eventInstance.release();
        }

        isPlaying = false;
    }

    public void StopEventOnPickup()
    {
        StopEvent();
    }
}