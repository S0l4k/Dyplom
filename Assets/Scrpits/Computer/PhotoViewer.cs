using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotoViewer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RawImage photoDisplay;
    [SerializeField] private TMP_Text photoNameText;
    [SerializeField] private RectTransform screenToShake; 

    [SerializeField] private ComputerInteract computerInteract;

    [Header("Photos")]
    [SerializeField] private PhotoData[] photos;

    [Header("Effects")]
    [SerializeField] private float screenShakeDuration = 0.3f;
    [SerializeField] private float screenShakeMagnitude = 0.02f; 
    [SerializeField] private float photoShakeMagnitude = 5f;     

    [SerializeField] private EventReference glitch;

    private bool isDemonShowing = false;
    private Vector3 originalPhotoPos;
    private Vector3 originalScreenPos; 

    void Start()
    {
        if (photoDisplay != null)
            originalPhotoPos = photoDisplay.transform.localPosition;

        if (screenToShake != null)
            originalScreenPos = screenToShake.localPosition;
    }

    public void ViewPhoto(int photoIndex)
    {
        if (photoIndex < 0 || photoIndex >= photos.Length)
        {
            return;
        }

        PhotoData photo = photos[photoIndex];
        if (photo == null)
        {
            return;
        }

        bool showDemon = Random.value < photo.demonChance;

        if (photoNameText != null)
        {
            photoNameText.gameObject.SetActive(true);
            photoNameText.text = photo.photoName;
        }

        if (photoDisplay != null)
        {
            Texture2D selectedTexture = showDemon ? photo.demonVersion : photo.normalVersion;
            if (selectedTexture == null)
                Debug.LogError($"[PhotoViewer] Texture is NULL! showDemon: {showDemon}");
            else
                Debug.Log($"[PhotoViewer] Setting texture: {selectedTexture.name}");

            photoDisplay.texture = selectedTexture;
            photoDisplay.gameObject.SetActive(true);
        }

        if (showDemon)
            TriggerDemonVersion(photo);
    }

    private void TriggerDemonVersion(PhotoData photo)
    {
        isDemonShowing = true;

        if (screenToShake != null)
            StartCoroutine(ShakeScreen());

        if (photoDisplay != null)
            StartCoroutine(ShakePhoto());
        RuntimeManager.PlayOneShot(glitch);
    }

    private IEnumerator ShakeScreen()
    {
        if (screenToShake == null) yield break;

        float elapsed = 0f;
        while (elapsed < screenShakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * screenShakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * screenShakeMagnitude;
            screenToShake.localPosition = originalScreenPos + new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        screenToShake.localPosition = originalScreenPos;
    }

    private IEnumerator ShakePhoto()
    {
        if (photoDisplay == null) yield break;

        float elapsed = 0f;
        while (elapsed < screenShakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * photoShakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * photoShakeMagnitude;
            photoDisplay.transform.localPosition = originalPhotoPos + new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        photoDisplay.transform.localPosition = originalPhotoPos;
    }

    public void ClosePhoto()
    {

        if (photoDisplay != null)
        {
            photoDisplay.texture = null;
            photoDisplay.gameObject.SetActive(false);
        }
        if (photoNameText != null)
        {
            photoNameText.text = "";
            photoNameText.gameObject.SetActive(false);
        }

        if (screenToShake != null)
            screenToShake.localPosition = originalScreenPos;

        isDemonShowing = false;
    }

    public void HideDemonEffect()
    {
        if (screenToShake != null)
            screenToShake.localPosition = originalScreenPos;
        isDemonShowing = false;
    }

    public bool IsDemonActive() => isDemonShowing;
}