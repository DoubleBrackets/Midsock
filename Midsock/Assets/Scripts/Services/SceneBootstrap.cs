using GameKit.Utilities.Types;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBootstrap : MonoBehaviour
{
    [SerializeField]
    [Scene]
    private string startupScene;

    // Start is called before the first frame update
    private void Start()
    {
        if (startupScene != null)
        {
            SceneManager.LoadSceneAsync(startupScene, LoadSceneMode.Additive);
        }
    }
}