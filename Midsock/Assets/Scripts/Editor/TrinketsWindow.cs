using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrinketsWindow : EditorWindow
{
    private const string PrefsPath = "Assets/Settings/Editor/User/SimpleUtilsPrefs.asset";

    private bool _cleanSOsOnPlay = true;

    private string _lastEditedScenePath;
    private UtilsPrefs _prefs;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Trinket Utils", EditorStyles.boldLabel);

        string startupScenePath = _prefs.StartupScenePath;

        if (GUILayout.Button("Play Game"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            _lastEditedScenePath = SceneManager.GetActiveScene().path;

            if (!SceneManager.GetSceneByPath(startupScenePath).isLoaded)
            {
                EditorSceneManager.OpenScene(startupScenePath, OpenSceneMode.Single);
            }

            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

        EditorGUILayout.Space(10);

        _cleanSOsOnPlay = EditorGUILayout.Toggle("clean SO on Play", _cleanSOsOnPlay);

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

    [MenuItem("Tools/Trinkets")]
    public static void ShowWindow()
    {
        GetWindow<TrinketsWindow>("Trinkets");
    }

    private void ModeChanged(PlayModeStateChange param)
    {
        if (param == PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.OpenScene(_lastEditedScenePath, OpenSceneMode.Single);
        }
        else if (param == PlayModeStateChange.EnteredPlayMode)
        {
            if (_cleanSOsOnPlay)
            {
                ResetSOValues();
            }
        }
    }

    private void ResetSOValues()
    {
        // reset SOs
        if (_cleanSOsOnPlay)
        {
            // Reset SO
            string[] so = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/ScriptableObjects" });
            foreach (string guid in so)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                (asset as IValueResettable)?.ResetValues();
            }
        }
    }
}