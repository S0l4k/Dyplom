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
        ambientInstance = AudioManager.Instance.CreateAmbientInstance(apartmentAmbientEvent);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlaying) return;
        AudioManager.Instance.StartAmbientInstance(ambientInstance, transform.position);
        isPlaying = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isPlaying) return;
        AudioManager.Instance.StopAmbientInstance(ref ambientInstance, true);
        isPlaying = false;
    }

    private void OnDestroy()
    {
        AudioManager.Instance.StopAmbientInstance(ref ambientInstance, true);
    }
}