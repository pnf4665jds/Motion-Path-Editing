using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BVHLoader : MonoBehaviour
{
    // ¿…¶W
    public string fileName = "walk_loop.bvh";

    private void Start()
    {
        // Read target file in resources folder
        string filePath = Application.dataPath + "/Resources/" + fileName;
        StreamReader sr = new StreamReader(filePath);
        string bvhText = sr.ReadToEnd();
        sr.Close();

        // Setup parser
        BVHParser parser = new BVHParser(bvhText);

        GameObject rootSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rootSphere.name = parser.root.name;
        rootSphere.transform.position = new Vector3(
            parser.root.channels[0].values[0],
            parser.root.channels[1].values[0],
            parser.root.channels[2].values[0]);

        foreach (BVHParser.BVHBone child in parser.root.children)
        {
            SetupSkeleton(child, rootSphere);
        }
    }

    private void SetupSkeleton(BVHParser.BVHBone currentBone, GameObject parent)
    {
        Vector3 offset = new Vector3(currentBone.offsetX, currentBone.offsetY, currentBone.offsetZ);
        Vector3 rotateVector = new Vector3(
            currentBone.channels[3].values[0],
            currentBone.channels[4].values[0],
            currentBone.channels[5].values[0]);

        Vector3 newOffset = Quaternion.Euler(rotateVector) * offset;

        GameObject boneCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        boneCapsule.transform.localScale = new Vector3(1, newOffset.magnitude / 2, 1);
        GameObject jointSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        jointSphere.transform.parent = boneCapsule.transform;
        boneCapsule.transform.position = parent.transform.position + newOffset / 2;
        boneCapsule.transform.rotation = Quaternion.Euler(rotateVector);
        boneCapsule.name = currentBone.name + "_Bone";
        boneCapsule.transform.parent = parent.transform;

        
        jointSphere.transform.localScale = new Vector3(1, 1 / boneCapsule.transform.localScale.y, 1);
        jointSphere.transform.position = boneCapsule.transform.position + newOffset / 2;

        jointSphere.name = currentBone.name + "_Joint";

        foreach (BVHParser.BVHBone child in currentBone.children)
        {
            SetupSkeleton(child, jointSphere);
        }
    }
}
