using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools.Utils;

public class UtilsWindow : EditorWindow
{
    private UtilsPrefs prefs;
    
    private const string prefsPath = "Assets/Scripts/Editor/UtilsPrefs.asset";
    
    [MenuItem("MyTools/Utilities")]
    public static void ShowWindow()
    {
        GetWindow<UtilsWindow>("MyTools");
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
}