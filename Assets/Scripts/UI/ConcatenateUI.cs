using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleFileBrowser;
public class ConcatenateUI : MonoBehaviour
{
    private string filePath1;
    private string filePath2;

    public float widthOffset;
    public float heightOffset;

    private FileBrowser.OnCancel cancel;

    public ConcatenateMotionPlayer concatenateMotionPlayer;
    // Start is called before the first frame update
    private void OnGUI()
    {
        GUI.skin.button.fontSize = 30;

        GUILayout.BeginArea(new Rect((Screen.width / 2) - widthOffset, (Screen.height / 2) - heightOffset, 400,500));
        GUILayout.BeginVertical();

        //GUILayout.BeginArea(new Rect(Screen.width * 0.05f, Screen.height * 0.5f, Screen.width * 0.9f, Screen.height * 0.01f));
        if (GUILayout.Button("Load BVH File 1", GUILayout.Width(300), GUILayout.Height(80)))
        {
            filePath1 = "";
            //filePath1 = EditorUtility.OpenFilePanel("Load BVH file 1", "", "");
            FileBrowser.ShowLoadDialog((paths) =>
            {
                filePath1 = paths[0];
                print(filePath1);
                concatenateMotionPlayer.SetupParser1(filePath1);
            }, DoOnCancel, FileBrowser.PickMode.Files);
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Load BVH File 2", GUILayout.Width(300), GUILayout.Height(80)))
        {
            filePath2 = "";
            //filePath2 = EditorUtility.OpenFilePanel("Load BVH file 2", "", "");
            
            FileBrowser.ShowLoadDialog((paths) =>
            {
                filePath2 = paths[0];
                print(filePath2);
                
                concatenateMotionPlayer.SetupParser2(filePath2);
            }, DoOnCancel, FileBrowser.PickMode.Files);
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Start", GUILayout.Width(300), GUILayout.Height(80)))
        {
            concatenateMotionPlayer.Init();
        }

        if (GUILayout.Button("Stop", GUILayout.Width(300), GUILayout.Height(80)))
        {
            concatenateMotionPlayer.Stop();
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DoOnCancel()
    {

    }
}
