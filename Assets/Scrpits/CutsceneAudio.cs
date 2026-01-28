using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class CutsceneAudio : MonoBehaviour
{
    [SerializeField] private EventReference jumpscareEvent;
    [SerializeField] private EventReference ambientEvent;
    [SerializeField] private EventReference crossEvent;
    [SerializeField] private EventReference crawlingEvent;
    private EventInstance ambientInstance;

    private void Start()
    {
        ambientInstance = RuntimeManager.CreateInstance(ambientEvent);
    }

    public void PlayJumpScare()
    {
        RuntimeManager.PlayOneShot(jumpscareEvent);
    }

    public void PlayCrossSqueak()
    {
        RuntimeManager.PlayOneShot(crossEvent);
    }

    public void PlayCrawling()
    {
        RuntimeManager.PlayOneShot(crawlingEvent);
    }

    public void PlayAmbient()
    {
        ambientInstance.start();
    }

    public void StopAmbient()
    {
        ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambientInstance.release();
    }
}