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
        // ✅ ZAMIENIONE: RuntimeManager -> AudioManager
        ambientInstance = AudioManager.Instance.CreateAmbientInstance(apartmentAmbientEvent);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlaying) return;

        // ✅ ZAMIENIONE: bezpośrednie set3DAttributes + start -> AudioManager
        AudioManager.Instance.StartAmbientInstance(ambientInstance, transform.position);
        isPlaying = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isPlaying) return;

        // ✅ ZAMIENIONE: bezpośrednie stop -> AudioManager
        AudioManager.Instance.StopAmbientInstance(ref ambientInstance, true);
        isPlaying = false;
    }

    private void OnDestroy()
    {
        // ✅ ZAMIENIONE: bezpośrednie release -> AudioManager
        AudioManager.Instance.StopAmbientInstance(ref ambientInstance, true);
    }
}