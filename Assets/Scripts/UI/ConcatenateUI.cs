using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class ConcatenateUI : MonoBehaviour
{
    private string filePath1;
    private string filePath2;

    public float widthOffset;
    public float heightOffset;

    public ConcatenateMotionPlayer concatenateMotionPlayer;
    // Start is called before the first frame update
    private void OnGUI()
    {
        GUI.skin.button.fontSize = 30;

        GUILayout.BeginArea(new Rect((Screen.width / 2) - widthOffset, (Screen.height / 2) - heightOffset, 400, 300));
        GUILayout.BeginVertical();

        //GUILayout.BeginArea(new Rect(Screen.width * 0.05f, Screen.height * 0.5f, Screen.width * 0.9f, Screen.height * 0.01f));
        if (GUILayout.Button("Load BVH File 1", GUILayout.Width(300), GUILayout.Height(80)))
        {
            filePath1 = "";
            filePath1 = EditorUtility.OpenFilePanel("Load BVH file 1", "", "");
            concatenateMotionPlayer.Stop();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Load BVH File 2", GUILayout.Width(300), GUILayout.Height(80)))
        {
            filePath2 = "";
            filePath2 = EditorUtility.OpenFilePanel("Load BVH file 2", "", "");
            concatenateMotionPlayer.Stop();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Start", GUILayout.Width(300), GUILayout.Height(80)))
        {
            concatenateMotionPlayer.Init(filePath1, filePath2);
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
