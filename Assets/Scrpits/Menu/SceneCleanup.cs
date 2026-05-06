using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCleanup : MonoBehaviour
{
    void Awake()
    {
        // 🔥 Reset timeScale na wszelki wypadek (naprawia white screen)
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // 🔥 Wymuś reload animatorów dla wszystkich postaci w scenie
        FixTposeCharacters();
    }

    private void FixTposeCharacters()
    {
        var animators = FindObjectsOfType<Animator>(true); // true = szukaj też w nieaktywnych
        foreach (var anim in animators)
        {
            if (anim != null && anim.isActiveAndEnabled)
            {
                // 🔁 Rebind przywraca połączenie Animator ↔ Avatar
                anim.Rebind();

                // 🎭 Jeśli masz blend tree / warstwy, wymuś aktualizację
                anim.Update(0f);
            }
        }
    }

    // 🔥 Opcjonalnie: wyczyść "zawieszone" singletony przy ładowaniu MainMenu
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            // Przykład: jeśli masz statyczną klasę GameState
            // GameState.ResetForMenu(); 
            // (musisz dodać taką metodę w swojej klasie)
        }
    }
}