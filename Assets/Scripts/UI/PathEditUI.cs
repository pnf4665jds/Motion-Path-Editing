using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleFileBrowser;

public class PathEditUI : MonoBehaviour
{
    private string filePath;
    public float widthOffset;
    public float heightOffset;
    public MotionPlayer motionPlayer;

    private string arcLengthButtonText = "Arc Length is off";
    private string editModeButtonText = "Edit mode is off";

    private void OnGUI()
    {
        GUI.skin.button.fontSize = 30;
        GUI.skin.label.fontSize = 30;

        GUILayout.BeginArea(new Rect((Screen.width / 2) - widthOffset, (Screen.height / 2) - heightOffset, 400, 600));
        if (GUILayout.Button("Load BVH File ", GUILayout.Width(300), GUILayout.Height(80)))
        {

            //filePath = EditorUtility.OpenFilePanel("Load BVH file ", "", "");
           
            FileBrowser.ShowLoadDialog((paths) =>
            {
                filePath = paths[0];
                motionPlayer.Stop();
                motionPlayer.SetupParser(filePath);
            }, DoOnCancel, FileBrowser.PickMode.Files);
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Start", GUILayout.Width(300), GUILayout.Height(80)))
        {
            motionPlayer.Init();
        }
        GUILayout.Space(10);
        if (GUILayout.Button(arcLengthButtonText, GUILayout.Width(300), GUILayout.Height(80)))
        {
            if (motionPlayer.isArcLength)
            {
                arcLengthButtonText = "Arc Length is off";
                motionPlayer.isArcLength = false;
            }
            else
            {
                arcLengthButtonText = "Arc Length is on";
                motionPlayer.isArcLength = true;
            }
        }
        GUILayout.Space(10);
        GUILayout.Label("Speed");
        motionPlayer.speed = GUILayout.HorizontalSlider(motionPlayer.speed, 0, 2, GUILayout.Width(300), GUILayout.Height(30));
        GUILayout.Space(10);
        if (GUILayout.Button(editModeButtonText, GUILayout.Width(300), GUILayout.Height(80)))
        {
            if (motionPlayer.showControlPoint)
            {
                editModeButtonText = "Edit mode is off";
                motionPlayer.showControlPoint = false;
            }
            else
            {
                editModeButtonText = "Edit mode is on";
                motionPlayer.showControlPoint = true;
            }
        }
        GUILayout.EndArea();
    }

    private void DoOnCancel()
    {

    }
}
