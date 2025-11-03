using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class LoudWhisper2D : MonoBehaviour
{
    [EventRef]
    public string WhisperEvent = "event:/Whispers"; // <- tu wpisz nazwê eventu

    public float interval = 10f; // czas miêdzy odtworzeniami
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

        instance.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.zero));

        instance.start();
        instance.release();
    }

    void ScheduleNextPlay()
    {
        nextPlayTime = Time.time + interval;
    }
}