using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BVHLoader : MonoBehaviour
{
    // 檔名
    public string fileName = "walk_loop.bvh";

    private Dictionary<BVHParser.BVHBone, GameObject> jointDic = new Dictionary<BVHParser.BVHBone, GameObject>();

    private void Start()
    {
        // Read target file in resources folder
        string filePath = Application.dataPath + "/Resources/" + fileName;
        StreamReader sr = new StreamReader(filePath);
        string bvhText = sr.ReadToEnd();
        sr.Close();

        // Setup parser
        BVHParser parser = new BVHParser(bvhText);

        StartCoroutine(Play(parser));
    }

    /// <summary>
    /// 播放Motion
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    private IEnumerator Play(BVHParser parser)
    {
        GameObject rootSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rootSphere.name = parser.root.name;

        int frame = 0;
        while (true)
        {
            if (frame >= parser.frames)
                frame = 0;
            rootSphere.transform.position = new Vector3(
                parser.root.channels[0].values[frame],
                parser.root.channels[1].values[frame],
                parser.root.channels[2].values[frame]);

            rootSphere.transform.rotation = Euler2Quat(new Vector3(
                parser.root.channels[3].values[frame],
                parser.root.channels[4].values[frame],
                parser.root.channels[5].values[frame]));

            foreach (BVHParser.BVHBone child in parser.root.children)
            {
                SetupSkeleton(child, rootSphere, frame);
            }

            frame++;
            yield return new WaitForSeconds(parser.frameTime);
        }
    }

    /// <summary>
    /// 根據frame設置joint位置
    /// </summary>
    /// <param name="currentBone"></param>
    /// <param name="parent"></param>
    /// <param name="frame"></param>
    private void SetupSkeleton(BVHParser.BVHBone currentBone, GameObject parent, int frame)
    {
        Vector3 offset = new Vector3(currentBone.offsetX, currentBone.offsetY, currentBone.offsetZ);
        Debug.Log(currentBone.name + " offset: " + offset.ToString("f2"));
        Vector3 rotateVector = new Vector3(
            currentBone.channels[3].values[frame],
            currentBone.channels[4].values[frame],
            currentBone.channels[5].values[frame]);

        Quaternion rotation = Euler2Quat(rotateVector);
        Vector3 newOffset = parent.transform.rotation * offset;

        GameObject joint;
        LineRenderer renderer;
        if (!jointDic.TryGetValue(currentBone, out joint))
        {
            joint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            joint.name = currentBone.name + "_Joint";
            joint.AddComponent<LineRenderer>();
            jointDic.Add(currentBone, joint);
        }
        
        joint.transform.position = parent.transform.position + newOffset;
        joint.transform.rotation = parent.transform.rotation * rotation;
        
        // Setup line renderer to draw bone between joint
        renderer = joint.GetComponent<LineRenderer>();
        renderer.material.SetColor("_Color", Color.blue);
        renderer.SetPosition(0, parent.transform.position);
        renderer.SetPosition(1, joint.transform.position);

        foreach (BVHParser.BVHBone child in currentBone.children)
        {
            SetupSkeleton(child, joint, frame);
        }
    }

    private Quaternion Euler2Quat(Vector3 euler)
    {
        return Quaternion.Euler(0, 0, euler.z) * Quaternion.Euler(euler.x, 0, 0) * Quaternion.Euler(0, euler.y, 0);
    }
}
