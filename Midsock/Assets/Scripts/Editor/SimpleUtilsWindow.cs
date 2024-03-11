using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleUtilsWindow : EditorWindow
{
    private const string PrefsPath = "Assets/Settings/Editor/User/SimpleUtilsPrefs.asset";

    private string _lastEditedScenePath;
    private UtilsPrefs _prefs;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Utils", EditorStyles.boldLabel);

        string startupScenePath = _prefs.StartupScenePath;

        if (GUILayout.Button("Play Game"))
        {
            // Close open editor scenes
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            _lastEditedScenePath = SceneManager.GetActiveScene().path;

            EditorSceneManager.OpenScene(startupScenePath, OpenSceneMode.Single);
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Open Persistent Data Path"))
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }

    private void OnBecameInvisible()
    {
        EditorApplication.playModeStateChanged -= ModeChanged;
    }

    private void OnBecameVisible()
    {
        _prefs = AssetDatabase.LoadAssetAtPath<UtilsPrefs>(PrefsPath);
        if (_prefs == null)
        {
            // Create directories if needed
            string directoryPath = Path.GetDirectoryName(PrefsPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            _prefs = CreateInstance<UtilsPrefs>();
            AssetDatabase.CreateAsset(_prefs, PrefsPath);
            AssetDatabase.SaveAssets();
        }

        EditorApplication.playModeStateChanged += ModeChanged;
    }

    private void ModeChanged(PlayModeStateChange param)
    {
        if (param == PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.OpenScene(_lastEditedScenePath, OpenSceneMode.Single);
        }
    }

    [MenuItem("MyTools/Utilities")]
    public static void ShowWindow()
    {
        GetWindow<SimpleUtilsWindow>("MyTools");
    }
}