using UnityEngine;
using System.Collections;

public class MenuLamp : MonoBehaviour
{
    [Header("Lamp Settings")]
    public GameObject lampObject; // obiekt lampy do w³¹czania/wy³¹czania
    public float minWait = 2f; // minimalny czas miêdzy migniêciami
    public float maxWait = 5f; // maksymalny czas miêdzy migniêciami
    public int flickerCount = 3; // ile razy lampa mrugnie za jednym razem
    public float flickerSpeed = 0.1f; // czas miêdzy w³¹czeniem a wy³¹czeniem w jednym migniêciu

    private void Start()
    {
        if (lampObject == null)
            lampObject = this.gameObject;

        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // czekaj kilka sekund przed nastêpnym migniêciem
            float waitTime = Random.Range(minWait, maxWait);
            yield return new WaitForSeconds(waitTime);

            // wykonaj seriê szybkich w³¹czeñ/wy³¹czeñ
            for (int i = 0; i < flickerCount; i++)
            {
                lampObject.SetActive(false);
                yield return new WaitForSeconds(flickerSpeed);
                lampObject.SetActive(true);
                yield return new WaitForSeconds(flickerSpeed);
            }
        }
    }
}