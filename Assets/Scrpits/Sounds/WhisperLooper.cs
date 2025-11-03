using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class LoudWhisperLooper : MonoBehaviour
{
    [EventRef]
    public string WhisperEvent; // np. "event:/Whispers"

    public float interval = 10f;
    private float nextPlayTime;

    void Start()
    {
        ScheduleNextPlay();
    }

    void Update()
    {
        if (Time.time >= nextPlayTime)
        {
            PlayWhisper();
            ScheduleNextPlay();
        }
    }

    void PlayWhisper()
    {
        EventInstance instance = RuntimeManager.CreateInstance(WhisperEvent);

        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));

        instance.start();
        instance.release();
    }

    void ScheduleNextPlay()
    {
        nextPlayTime = Time.time + interval;
    }
}