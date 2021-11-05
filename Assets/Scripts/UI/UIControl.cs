using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class UIControl : MonoBehaviour
{
    
    public Button LoadButton;

    [Header("BVH")]
    public string FilePath;

    public void LoadFile() 
    {
        var path = EditorUtility.SaveFolderPanel("Load bvh", "", "");
        FilePath = path;
    }
    
}
