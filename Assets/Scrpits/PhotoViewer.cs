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
    [SerializeField] private RectTransform screenToShake; // 👈 NOWE: panel/canvas do shake'owania

    [SerializeField] private ComputerInteract computerInteract;

    [Header("Photos")]
    [SerializeField] private PhotoData[] photos;

    [Header("Effects")]
    [SerializeField] private float screenShakeDuration = 0.3f;
    [SerializeField] private float screenShakeMagnitude = 0.02f; // w lokalnych jednostkach Canvasa
    [SerializeField] private float photoShakeMagnitude = 5f;     // w pikselach dla zdjęcia

    [SerializeField] private EventReference glitch;

    private bool isDemonShowing = false;
    private Vector3 originalPhotoPos;
    private Vector3 originalScreenPos; // 👈 zapamiętana pozycja ekranu

    void Start()
    {
        if (photoDisplay != null)
            originalPhotoPos = photoDisplay.transform.localPosition;

        if (screenToShake != null)
            originalScreenPos = screenToShake.localPosition;
    }

    public void ViewPhoto(int photoIndex)
    {
        Debug.Log($"[PhotoViewer] ViewPhoto called! Index: {photoIndex}");

        if (photoIndex < 0 || photoIndex >= photos.Length)
        {
            Debug.LogError($"[PhotoViewer] Invalid index! Length: {photos.Length}");
            return;
        }

        PhotoData photo = photos[photoIndex];
        if (photo == null)
        {
            Debug.LogError($"[PhotoViewer] PhotoData at index {photoIndex} is NULL!");
            return;
        }

        bool showDemon = Random.value < photo.demonChance;
        Debug.Log($"[PhotoViewer] Photo: {photo.photoName}, Demon roll: {showDemon}");

        // 🔓 Odblokuj UI
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

        // 👇 Odpal shake na EKRANIE + na ZDJĘCIU
        if (screenToShake != null)
            StartCoroutine(ShakeScreen());

        if (photoDisplay != null)
            StartCoroutine(ShakePhoto());
        RuntimeManager.PlayOneShot(glitch);
    }

    // 👇 NOWA METODA: shake całego panelu/ekranu
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
        // Przywróć oryginalną pozycję
        screenToShake.localPosition = originalScreenPos;
    }

    // 👇 Poprawiony shake zdjęcia (w pikselach UI)
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
        Debug.Log("[PhotoViewer] Closing photo");

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

        // 👇 Przywróć pozycję ekranu, jeśli shake był w trakcie
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