using GameKit.Utilities.Types;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBootstrap : MonoBehaviour
{
    [SerializeField]
    [Scene]
    private string _startupScene;

    // Start is called before the first frame update
    private void Start()
    {
        if (_startupScene != null)
        {
            SceneManager.LoadSceneAsync(_startupScene, LoadSceneMode.Additive);
        }
    }
}