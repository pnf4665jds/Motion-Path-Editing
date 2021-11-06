using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathEditUI : MonoBehaviour
{
    private string filePath;
    public float widthOffset;
    public float heightOffset;

    private void OnGUI()
    {
        GUI.skin.button.fontSize = 30;
        GUILayout.BeginArea(new Rect((Screen.width / 2) - widthOffset, (Screen.height / 2) - heightOffset, 400, 200));
        if (GUILayout.Button("Load BVH File ", GUILayout.Width(300), GUILayout.Height(80)))
        {
            
            filePath = EditorUtility.OpenFilePanel("Load BVH file ", "", "");
            
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Start", GUILayout.Width(300), GUILayout.Height(80)))
        {

        }
        GUILayout.EndArea();
    }
}
