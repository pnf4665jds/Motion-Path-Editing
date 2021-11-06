using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BVHLoader
{
    public BVHParser parser { get; set; }
    public GameObject rootJoint { get; set; }

    private Dictionary<BVHParser.BVHBone, GameObject> jointDic;
    private GameObject parentObject;
    private List<float> chordLengthParameter;
    private Color skeletonColor;
    public void Init(string filePath, Color color)
    {   
        string bvhText = File.ReadAllText(filePath); ;

        // Setup parser
        parser = new BVHParser(bvhText);

        parentObject = new GameObject();
        parentObject.name = "Skeleton";

        rootJoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rootJoint.transform.parent = parentObject.transform;
        rootJoint.name = parser.root.name;

        chordLengthParameter = new List<float>();
        jointDic = new Dictionary<BVHParser.BVHBone, GameObject>();

        skeletonColor = color;
    }

    public void Stop()
    {
        if(parentObject)
            Object.Destroy(parentObject);
    }

    /// <summary>
    /// 根據frame設置joint位置
    /// </summary>
    /// <param name="currentBone"></param>
    /// <param name="parent"></param>
    /// <param name="frame"></param>
    public void SetupSkeleton(BVHParser.BVHBone currentBone, GameObject parent, int frame)
    {
        Vector3 offset = new Vector3(currentBone.offsetX, currentBone.offsetY, currentBone.offsetZ);
        Vector3 rotateVector = new Vector3(
            currentBone.channels[3].values[frame],
            currentBone.channels[4].values[frame],
            currentBone.channels[5].values[frame]);

        // joint的order記錄在index0, 1, 2
        int[] order = new int[3] { currentBone.channelOrder[0], currentBone.channelOrder[1], currentBone.channelOrder[2] };
        Quaternion rotation = ParserTool.Euler2Quat(rotateVector, order);
        Vector3 newOffset = parent.transform.rotation * offset;

        GameObject joint;
        LineRenderer renderer;
        if (!jointDic.TryGetValue(currentBone, out joint))
        {
            joint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            joint.transform.parent = parent.transform;
            joint.name = currentBone.name + "_Joint";
            joint.AddComponent<LineRenderer>();
            jointDic.Add(currentBone, joint);
        }
        
        // joint global position = parent global position + (parent global rotation * offset)
        joint.transform.position = parent.transform.position + newOffset;
        // joint global rotation = parent global rotation * joint local rotation
        joint.transform.rotation = parent.transform.rotation * rotation;
        
        // 利用LineRenderer畫Bone
        renderer = joint.GetComponent<LineRenderer>();
        renderer.material.SetColor("_Color", skeletonColor);
        renderer.SetPosition(0, parent.transform.position);
        renderer.SetPosition(1, joint.transform.position);

        foreach (BVHParser.BVHBone child in currentBone.children)
        {
            SetupSkeleton(child, joint, frame);
        }
    }

    /// <summary>
    /// 根據frame設置joint位置
    /// </summary>
    /// <param name="currentBone"></param>
    /// <param name="parent"></param>
    /// <param name="frame"></param>
    public void SetupSkeleton(BVHParser.BVHBone currentBone, GameObject parent, int frame, Motion concatenateMotion)
    {
        Vector3 offset = new Vector3(currentBone.offsetX, currentBone.offsetY, currentBone.offsetZ);
        Vector3 rotateVector = concatenateMotion.GetRotation(currentBone.name, frame);

        // joint的order記錄在index0, 1, 2
        int[] order = new int[3] { currentBone.channelOrder[0], currentBone.channelOrder[1], currentBone.channelOrder[2] };
        Quaternion rotation = ParserTool.Euler2Quat(rotateVector, order);
        Vector3 newOffset = parent.transform.rotation * offset;

        GameObject joint;
        LineRenderer renderer;
        if (!jointDic.TryGetValue(currentBone, out joint))
        {
            joint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            joint.transform.parent = parent.transform;
            joint.name = currentBone.name + "_Joint";
            joint.AddComponent<LineRenderer>();
            jointDic.Add(currentBone, joint);
        }

        // joint global position = parent global position + (parent global rotation * offset)
        joint.transform.position = parent.transform.position + newOffset;
        // joint global rotation = parent global rotation * joint local rotation
        joint.transform.rotation = parent.transform.rotation * rotation;

        // 利用LineRenderer畫Bone
        renderer = joint.GetComponent<LineRenderer>();
        renderer.material.SetColor("_Color", skeletonColor);
        renderer.SetPosition(0, parent.transform.position);
        renderer.SetPosition(1, joint.transform.position);

        foreach (BVHParser.BVHBone child in currentBone.children)
        {
            SetupSkeleton(child, joint, frame, concatenateMotion);
        }
    }

    public void SetUpArcLengthParameter()
    {
        float QX, QY, QZ;
        float QX_old, QY_old, QZ_old;
        // 計算chord-length
        float d = 0, t = 0;
        for (int i = 1; i < parser.frames; i++)
        {
            Vector3 point1 = new Vector3(parser.root.channels[0].values[i - 1], parser.root.channels[1].values[i - 1], parser.root.channels[2].values[i - 1]);
            Vector3 point2 = new Vector3(parser.root.channels[0].values[i], parser.root.channels[1].values[i], parser.root.channels[2].values[i]);
            d += Vector3.Distance(point1, point2);
        }

        chordLengthParameter.Add(0);
        for (int i = 1; i < parser.frames; i++)
        {
            QX = parser.root.channels[0].values[i];
            QY = parser.root.channels[1].values[i];
            QZ = parser.root.channels[2].values[i];

            QX_old = parser.root.channels[0].values[i - 1];
            QY_old = parser.root.channels[1].values[i - 1];
            QZ_old = parser.root.channels[2].values[i - 1];

            t = chordLengthParameter[i - 1] + Vector3.Distance(new Vector3(QX_old, QY_old, QZ_old), new Vector3(QX, QY, QZ)) / d;
            chordLengthParameter.Add(t);
        }
        chordLengthParameter[chordLengthParameter.Count - 1] = 1;
    }

    /// <summary>
    /// 找到fit motion data的cubic b-spline控制點
    /// </summary>
    public Matrix4x4 SolveFitCurve()
    {
        SetUpArcLengthParameter();
        float frameTime = 1.0f / parser.frames;
        //parser.root.channels[0].values[0]
        Matrix4x4 matA = Matrix4x4.zero;
        Matrix4x4 matB = Matrix4x4.zero;

        float B0, B1, B2, B3;
        float QX, QY, QZ;

        float t;

        for (int i = 0; i < parser.frames; i++)
        {
            QX = parser.root.channels[0].values[i];
            QY = parser.root.channels[1].values[i];
            QZ = parser.root.channels[2].values[i];

            t = chordLengthParameter[i];

            B0 = Bezier.Get_B_Zero(t);
            B1 = Bezier.Get_B_One(t);
            B2 = Bezier.Get_B_Two(t);
            B3 = Bezier.Get_B_Three(t);

            // 設定A矩陣
            matA.m00 += B0 * B0;
            matA.m01 += B0 * B1;
            matA.m02 += B0 * B2;
            matA.m03 += B0 * B3;
            matA.m10 += B1 * B0;
            matA.m11 += B1 * B1;
            matA.m12 += B1 * B2;
            matA.m13 += B1 * B3;
            matA.m20 += B2 * B0;
            matA.m21 += B2 * B1;
            matA.m22 += B2 * B2;
            matA.m23 += B2 * B3;
            matA.m30 += B3 * B0;
            matA.m31 += B3 * B1;
            matA.m32 += B3 * B2;
            matA.m33 += B3 * B3;

            // 設定B矩陣
            matB.m00 += B0 * QX;
            matB.m01 += B0 * QY;
            matB.m02 += B0 * QZ;
            matB.m10 += B1 * QX;
            matB.m11 += B1 * QY;
            matB.m12 += B1 * QZ;
            matB.m20 += B2 * QX;
            matB.m21 += B2 * QY;
            matB.m22 += B2 * QZ;
            matB.m30 += B3 * QX;
            matB.m31 += B3 * QY;
            matB.m32 += B3 * QZ;
        }

        return matA.inverse * matB;
    }

    public List<float> GetChordLengthParameterList()
    {
        return chordLengthParameter;
    }
}
