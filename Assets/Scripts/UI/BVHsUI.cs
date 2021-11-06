using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BVHsUI : MonoBehaviour
{
    public float widthOffset;
    public float heightOffset;
    private string filePath1;
    private string filePath2;
    private string filePath3;

    private void OnGUI()
    {
        GUI.skin.button.fontSize = 30;

        GUILayout.BeginArea(new Rect((Screen.width / 2) - widthOffset, (Screen.height / 2) - heightOffset, 400, 400));
        if (GUILayout.Button("Load BVH File 1 ", GUILayout.Width(300), GUILayout.Height(80)))
        {

            filePath1 = EditorUtility.OpenFilePanel("Load BVH file ", "", "");

        }
        GUILayout.Space(10);
        if (GUILayout.Button("Load BVH File 2 ", GUILayout.Width(300), GUILayout.Height(80)))
        {

            filePath2 = EditorUtility.OpenFilePanel("Load BVH file ", "", "");

        }
        GUILayout.Space(10);

        if (GUILayout.Button("Load BVH File 3 ", GUILayout.Width(300), GUILayout.Height(80)))
        {

            filePath3 = EditorUtility.OpenFilePanel("Load BVH file ", "", "");

        }

        GUILayout.EndArea();
    }
}
