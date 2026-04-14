using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            LoadScene();
        }
    }
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
