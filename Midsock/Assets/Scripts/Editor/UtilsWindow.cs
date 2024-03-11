using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class UtilsWindow : EditorWindow
{
    private const string prefsPath = "Assets/Scripts/Editor/UtilsPrefs.asset";
    private UtilsPrefs prefs;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Utils", EditorStyles.boldLabel);

        var startupScenePath = prefs.StartupScenePath;

        if (GUILayout.Button("Play Game"))
        {
            // Close open editor scenes
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            EditorSceneManager.OpenScene(startupScenePath, OpenSceneMode.Single);
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
    }

    private void OnBecameVisible()
    {
        prefs = AssetDatabase.LoadAssetAtPath<UtilsPrefs>(prefsPath);
        if (prefs == null)
        {
            prefs = CreateInstance<UtilsPrefs>();
            AssetDatabase.CreateAsset(prefs, prefsPath);
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("MyTools/Utilities")]
    public static void ShowWindow()
    {
        GetWindow<UtilsWindow>("MyTools");
    }
}