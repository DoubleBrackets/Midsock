using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing.Scened;
using GameKit.Utilities.Types;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBootstrap : MonoBehaviour
{
    [SerializeField, Scene]
    private string startupScene;
    
    // Start is called before the first frame update
    void Start()
    {
        if (startupScene != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(startupScene, LoadSceneMode.Additive);
        }
    }
}
