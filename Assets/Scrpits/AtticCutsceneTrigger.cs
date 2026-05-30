using System.Collections;
using UnityEngine;

public class AtticCutsceneTrigger : MonoBehaviour
{
    public AtticQuestController questController;
    public Transform demonLookAtTarget;  // ✅ Przeciągnij demona lub punkt, na który ma patrzeć kamera
    public float lookAtSpeed = 5f;        // ✅ Prędkość obracania kamery
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
        Debug.Log("[AtticTrigger] 🎬 Player entered trigger – starting cutscene");

        // ✅ OBRÓĆ KAMERĘ NA DEMONA PRZED STARTEM CUTSCENKI
        StartCoroutine(LockCameraOnDemon());

        // ✅ Start cutscenki z małym opóźnieniem, żeby kamera zdążyła się obrócić
        StartCoroutine(StartCutsceneDelayed());
    }

    private IEnumerator LockCameraOnDemon()
    {
        if (_playerCamera == null || demonLookAtTarget == null) yield break;

        // ✅ Zablokuj skrypt PlayerCam, żeby nie nadpisywał rotacji
        if (_playerCamScript != null)
            _playerCamScript.enabled = false;

        Quaternion startRot = _playerCamera.transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(demonLookAtTarget.position - _playerCamera.transform.position, Vector3.up);

        float elapsed = 0f;
        float duration = 1f; // Czas obrotu kamery

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _playerCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            yield return null;
        }

        _playerCamera.transform.rotation = targetRot;
        Debug.Log("[AtticTrigger] 🎥 Camera locked on demon");
    }

    private IEnumerator StartCutsceneDelayed()
    {
        yield return new WaitForSeconds(1.2f); // ✅ Poczekaj aż kamera się obróci
        questController.StartCutscene();
    }
}