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
        Debug.Log($"[FMODTriggerZone] Trigger Entered by {other.name}");
        if (!other.CompareTag("Player")) return;
        if (isPlaying) return;

        Debug.Log("[FMODTriggerZone] Player entered trigger — starting event: " + eventPath);
        eventInstance = RuntimeManager.CreateInstance(eventPath);
        eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(soundSource));
        eventInstance.start();

        isPlaying = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!stopOnPlayerExit) return;

        Debug.Log("[FMODTriggerZone] Player left trigger — stopping event (exit).");
        StopEvent();
    }

    public void StopEvent()
    {
        if (!isPlaying)
        {
            Debug.Log("[FMODTriggerZone] StopEvent() called but event is not playing.");
            return;
        }

        if (eventInstance.isValid())
        {
            Debug.Log("[FMODTriggerZone] Stopping FMOD event with fadeout.");
            eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            eventInstance.release();
        }
        else
        {
            Debug.LogWarning("[FMODTriggerZone] eventInstance invalid when trying to stop.");
        }

        isPlaying = false;
    }

    public void StopEventOnPickup()
    {
        StopEvent();
    }
}