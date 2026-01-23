using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RandomWhispers : MonoBehaviour
{
    [SerializeField] private EventReference whispersEvent;
    [SerializeField] private float minInterval = 5f;
    [SerializeField] private float maxInterval = 15f;

    private float timer;
    private EventInstance currentInstance;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        
        if (currentInstance.isValid())
        {
            PLAYBACK_STATE state;
            currentInstance.getPlaybackState(out state);
            if (state != PLAYBACK_STATE.STOPPED)
            {
                
                return;
            }
        }

      
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            PlayWhisper();
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        timer = Random.Range(minInterval, maxInterval);
    }

    void PlayWhisper()
    {
        
        currentInstance = RuntimeManager.CreateInstance(whispersEvent);
        currentInstance.start();
        currentInstance.release(); 
    }
}
