using FMOD;
using UnityEngine;

public class koniecminigrytest : MonoBehaviour
{
    public NarrativeInspectTrigger linkedTrigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (linkedTrigger != null)
            linkedTrigger.EndFlashback();
    }
}
