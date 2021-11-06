using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class DebugUIController : MonoBehaviour
{
    public string FilePath;
    public string FilePath2;
    public int ModeNumber;

    public string[] StatusMode = { "Concatenate", "SkeletonPathEditing", "ModelPathEditing" };

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

        GUILayout.EndHorizontal();

        /*if (ModeNumber == 0)
        {
            GUILayout.BeginVertical();

            //GUILayout.BeginArea(new Rect(Screen.width * 0.05f, Screen.height * 0.5f, Screen.width * 0.9f, Screen.height * 0.01f));
            if (GUILayout.Button("Load BVH File 1", GUILayout.Width(300), GUILayout.Height(80)))
            {
                FilePath = "";
                FilePath = EditorUtility.OpenFilePanel("Load BVH file 1", "", "");
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Load BVH File 2", GUILayout.Width(300), GUILayout.Height(80)))
            {
                FilePath2 = "";
                FilePath2 = EditorUtility.OpenFilePanel("Load BVH file 2", "", "");
            }
            GUILayout.EndVertical();
        }
        else if (ModeNumber == 1)
        {
            //GUILayout.BeginArea(new Rect(Screen.width * 0.05f, Screen.height * 0.5f, Screen.width * 0.9f, Screen.height * 0.01f));
            if (GUILayout.Button("Load BVH File ", GUILayout.Width(300), GUILayout.Height(80)))
            {
                FilePath = "";
                FilePath2 = "";
                FilePath = EditorUtility.OpenFilePanel("Load BVH file ", "", "");
            }
        }
        else 
        {
            FilePath = Application.dataPath + "/Resources/08_01.bvh";
        }*/
    }
}
