using UnityEngine;
using System.Collections;

public class WindowJumpscareTrigger : MonoBehaviour
{
    [Header("References")]
    public GameNarrativeManager narrativeManager;
    public Transform demonSpawnPoint;  

    [Header("Jumpscare Settings")]
    [Tooltip("Offset w górę, żeby kamera patrzyła na twarz demona, a nie stopy (np. 1.5)")]
    public Vector3 demonLookAtOffset = new Vector3(0f, 1.5f, 0f);

    [Tooltip("Czy używać offsetu przy patrzeniu na demona")]
    public bool useLookAtOffset = true;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    [HideInInspector]
    public bool hasTriggered = false;

    private PlayerController playerController;
    private PlayerCam playerCam;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        playerCam = FindObjectOfType<PlayerCam>();
        gameObject.SetActive(false); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        if (narrativeManager == null) return;

        hasTriggered = true;
        StartCoroutine(TriggerJumpscare());
    }

    public void ActivateTrigger() => gameObject.SetActive(true);

    private IEnumerator TriggerJumpscare()
    {
        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;

        if (narrativeManager.demonFinalJumpscareObject != null && demonSpawnPoint != null)
        {
            narrativeManager.demonFinalJumpscareObject.transform.position = demonSpawnPoint.position;
            narrativeManager.demonFinalJumpscareObject.transform.rotation = demonSpawnPoint.rotation;
            narrativeManager.demonFinalJumpscareObject.SetActive(true);
        }

        Transform playerCamera = Camera.main?.transform;
        if (narrativeManager.demonFinalJumpscareObject != null && playerCamera != null)
        {
            Transform demonTransform = narrativeManager.demonFinalJumpscareObject.transform;
            Vector3 lookAtTarget = demonTransform.position;

            if (useLookAtOffset)
            {
                lookAtTarget = demonTransform.TransformPoint(demonLookAtOffset);
            }

            yield return StartCoroutine(narrativeManager.SmoothLookAt(
                playerCamera,
                CreateTempLookTarget(lookAtTarget),  
                narrativeManager.cameraTurnToDemonDuration
            ));
        }

        if (!narrativeManager.demonJumpscareRevealSFX.IsNull && playerCamera != null)
        {
            AudioManager.Instance.PlaySFX(
                narrativeManager.demonJumpscareRevealSFX,
                playerCamera.position
            );
        }

        yield return new WaitForSeconds(1.5f);

        if (narrativeManager.screenFader != null)
        {
            yield return StartCoroutine(narrativeManager.screenFader.FadeOut(narrativeManager.jumpscareFadeDuration));
        }

        if (!narrativeManager.windowBreakAndFallSFX.IsNull && playerCamera != null)
        {
            AudioManager.Instance.PlaySFX(narrativeManager.windowBreakAndFallSFX, playerCamera.position);
        }

        yield return new WaitForSeconds(narrativeManager.blackScreenDuration);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    private Transform CreateTempLookTarget(Vector3 position)
    {
        GameObject temp = new GameObject("TempJumpscareLookTarget");
        temp.transform.position = position;
        temp.transform.rotation = Quaternion.identity;
        Destroy(temp, 2f); 
        return temp.transform;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;

        if (narrativeManager?.demonFinalJumpscareObject != null && useLookAtOffset)
        {
            Vector3 headPos = narrativeManager.demonFinalJumpscareObject.transform.TransformPoint(demonLookAtOffset);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(headPos, 0.15f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(narrativeManager.demonFinalJumpscareObject.transform.position, headPos);
        }
    }
#endif
}