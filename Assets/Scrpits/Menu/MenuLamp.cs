using UnityEngine;
using System.Collections;

public class MenuLamp : MonoBehaviour
{
    [Header("Lamp Settings")]
    public GameObject lampObject; 
    public float minWait = 2f; 
    public float maxWait = 5f; 
    public int flickerCount = 3; 
    public float flickerSpeed = 0.1f;

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
            float waitTime = Random.Range(minWait, maxWait);
            yield return new WaitForSeconds(waitTime);

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