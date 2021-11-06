using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BVHsUI : MonoBehaviour
{
    public float widthOffset;
    public float heightOffset;
    public SingleMotionPlayer singleMotionPlayer1;
    public SingleMotionPlayer singleMotionPlayer2;
    public SingleMotionPlayer singleMotionPlayer3;

    private string filePath1;
    private string filePath2;
    private string filePath3;
    private Vector3 basePos = new Vector3(-3f, 15f, -101f);

    private void OnGUI()
    {
        GUI.skin.button.fontSize = 30;

        GUILayout.BeginArea(new Rect((Screen.width / 2) - widthOffset, (Screen.height / 2) - heightOffset, 400, 400));
        if (GUILayout.Button("Load BVH File 1 ", GUILayout.Width(300), GUILayout.Height(80)))
        {
            singleMotionPlayer1.Stop();
            filePath1 = EditorUtility.OpenFilePanel("Load BVH file ", "", "");
            singleMotionPlayer1.Init(filePath1, basePos + Vector3.left * 40);
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Load BVH File 2 ", GUILayout.Width(300), GUILayout.Height(80)))
        {
            singleMotionPlayer2.Stop();
            filePath2 = EditorUtility.OpenFilePanel("Load BVH file ", "", "");
            singleMotionPlayer2.Init(filePath2, basePos);
        }
        GUILayout.Space(10);

        if (GUILayout.Button("Load BVH File 3 ", GUILayout.Width(300), GUILayout.Height(80)))
        {
            singleMotionPlayer3.Stop();
            filePath3 = EditorUtility.OpenFilePanel("Load BVH file ", "", "");
            singleMotionPlayer3.Init(filePath3, basePos + Vector3.right * 40);
        }

        GUILayout.EndArea();
    }
}
