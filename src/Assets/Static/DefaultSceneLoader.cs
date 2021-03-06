﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable AccessToStaticMemberViaDerivedType

[InitializeOnLoadAttribute]
public static class DefaultSceneLoader
{
    static DefaultSceneLoader()
    {
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    static void LoadDefaultScene(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorSceneManager.LoadScene(0);
        }
    }
}
#endif
