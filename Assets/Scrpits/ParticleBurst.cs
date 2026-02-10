using UnityEngine;

// ✅ Skrypt do jednorazowego burstu particle'ów
[RequireComponent(typeof(ParticleSystem))]
public class ParticleBurst : MonoBehaviour
{
    private ParticleSystem ps;
    private float duration;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        duration = ps.main.duration;
    }

    // ✅ Odtwórz i zniszcz po zakończeniu
    public void PlayBurst(Vector3 position)
    {
        transform.position = position;
        ps.Play();
        Destroy(gameObject, duration + 0.2f); // +0.2s na bezpieczeństwo
    }
}