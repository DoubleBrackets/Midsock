using UnityEngine;
using UnityEngine.SceneManagement;

public class LightSingleton : MonoBehaviour
{
    private Light _light;

    private void Awake()
    {
        _light = gameObject.GetComponent<Light>();
    }

    private void Update()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        _light.enabled = gameObject.scene == activeScene;
    }
}