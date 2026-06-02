using System.Collections;
using UnityEngine;

public class AtticCutsceneTrigger : MonoBehaviour
{
    public AtticQuestController questController;
    public Transform demonLookAtTarget;  
    public float lookAtSpeed = 5f;        
    private bool _triggered = false;
    private Camera _playerCamera;
    private PlayerCam _playerCamScript;

    private void Start()
    {
        _playerCamera = Camera.main;
        _playerCamScript = _playerCamera?.GetComponent<PlayerCam>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (questController == null) return;

        if (!other.CompareTag("Player") && other.GetComponent<PlayerController>() == null)
            return;

        _triggered = true;

        StartCoroutine(LockCameraOnDemon());

        StartCoroutine(StartCutsceneDelayed());
    }

    private IEnumerator LockCameraOnDemon()
    {
        if (_playerCamera == null || demonLookAtTarget == null) yield break;

        if (_playerCamScript != null)
            _playerCamScript.enabled = false;

        Quaternion startRot = _playerCamera.transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(demonLookAtTarget.position - _playerCamera.transform.position, Vector3.up);

        float elapsed = 0f;
        float duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _playerCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            yield return null;
        }

        _playerCamera.transform.rotation = targetRot;
    }

    private IEnumerator StartCutsceneDelayed()
    {
        yield return new WaitForSeconds(1.2f); 
        questController.StartCutscene();
    }
}