using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelUI : MonoBehaviour
{

    public int widthOffset;
    public int heightOffset;
    public NewMotionPlayer motionPlayer;
    private string arcLengthButtonText = "Arc Length is off";

    private void OnGUI()
    {
        GUI.skin.button.fontSize = 30;
        GUILayout.BeginArea(new Rect((Screen.width / 2) - widthOffset, (Screen.height / 2) - heightOffset, 400, 400));

        if (GUILayout.Button(arcLengthButtonText, GUILayout.Width(300), GUILayout.Height(80)))
        {
            if (motionPlayer.IsArcLength)
            {
                arcLengthButtonText = "Arc Length is off";
                motionPlayer.IsArcLength = false;
            }
            else
            {
                arcLengthButtonText = "Arc Length is on";
                motionPlayer.IsArcLength = true;
            }
        }
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 30;
        GUILayout.Label("The ArcLength Speed");
        motionPlayer.speed = GUILayout.HorizontalSlider(motionPlayer.speed, 0, 2, GUILayout.Width(300), GUILayout.Height(80));
        GUILayout.EndArea();
    }
}
