using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class DebugUIController : MonoBehaviour
{
    public int ModeNumber;

    private void Start()
    {
        ModeNumber = 0;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.LoadScene("ConcatenateScene");
    }
    void OnGUI()
    {

        //ModeNumber = GUILayout.Toolbar(ModeNumber, StatusMode);
        GUI.skin.button.fontSize = 30;
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Concatenate", GUILayout.Width(300), GUILayout.Height(80))) 
        {
            ModeNumber = 0;
            SceneManager.LoadScene("ConcatenateScene");
        }

        if (GUILayout.Button("SkeletonPathEditing", GUILayout.Width(300), GUILayout.Height(80)))
        {

            ModeNumber = 1;
            SceneManager.LoadScene("MainScene");

        }
        if(GUILayout.Button("ModelPathEditing", GUILayout.Width(300), GUILayout.Height(80))) 
        {
            ModeNumber = 2;
            SceneManager.LoadScene("TestScene");
        }
        if (GUILayout.Button("Load BVHs", GUILayout.Width(300), GUILayout.Height(80)))
        {
            ModeNumber = 3;
            SceneManager.LoadScene("BVHsScene");
        }

        GUILayout.EndHorizontal();

       
    }
}
