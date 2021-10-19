using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BVHLoader : MonoBehaviour
{
    public string fileName;

    private void Start()
    {
        var textAsset = Resources.Load(fileName);
        if (!textAsset)
            Debug.LogError("Can't read bvh file: " + fileName);
        else
        {
            //BVHParser parser = new BVHParser(textAsset.text);
            //Debug.Log(parser.root.children);
        }
    }
}
