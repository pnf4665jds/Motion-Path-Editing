using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// 利用不同的path對應到不同的model
/// </summary>
public class NewMotionPlayer : MonoBehaviour
{
    public bool IsShowPoint { get; set; } = false;
    private string fileName;
    public NewBVHDriver driver;
    public RunTimeBezier path1;
    public RunTimeBezier path2;

    private NewBVHDriver modelDriver1;
    private NewBVHDriver modelDriver2;

    GameObject skeleton1Root;
    GameObject skeleton2Root;

    private float finalT = 0;
    public bool IsArcLength{ get; set; } = false;
    public float speed = 1;

    [Serializable]
    public struct BoneMap
    {
        public string bvh_name;
        public HumanBodyBones humanoid_bone;
        public BoneMap(string bvh_name, HumanBodyBones humanoid_bone) 
        {
            this.bvh_name = bvh_name;
            this.humanoid_bone = humanoid_bone;
        }
    }
    public BoneMap[] boneMaps;
    public BVHParser parseFile()
    {
        string bvhData = File.ReadAllText(fileName);
        return new BVHParser(bvhData);

    }


    private void Start()
    {
        fileName = Application.dataPath + "/Resources/08_01.bvh";

        skeleton1Root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        skeleton2Root = GameObject.CreatePrimitive(PrimitiveType.Cube);

        boneMaps = new BoneMap[18] {
        new BoneMap("Hips",HumanBodyBones.Hips),
        new BoneMap("LeftUpLeg", HumanBodyBones.RightUpperLeg),
        new BoneMap("LeftLeg", HumanBodyBones.RightLowerLeg),
        new BoneMap("LeftFoot", HumanBodyBones.RightFoot),
        new BoneMap("RightUpLeg", HumanBodyBones.LeftUpperLeg),
        new BoneMap("RightLeg", HumanBodyBones.LeftLowerLeg),
        new BoneMap("RightFoot", HumanBodyBones.LeftFoot),
        new BoneMap("Spine", HumanBodyBones.Spine),
        new BoneMap("Spine1", HumanBodyBones.Chest),
        new BoneMap("Neck", HumanBodyBones.Neck),
        new BoneMap("LeftShoulder", HumanBodyBones.RightShoulder),
        new BoneMap("LeftArm", HumanBodyBones.RightUpperArm),
        new BoneMap("LeftForeArm", HumanBodyBones.RightLowerArm),
        new BoneMap("LeftHand", HumanBodyBones.RightHand),
        new BoneMap("RightShoulder", HumanBodyBones.LeftShoulder),
        new BoneMap("RightArm", HumanBodyBones.LeftUpperArm),
        new BoneMap("RightForeArm", HumanBodyBones.LeftLowerArm),
        new BoneMap("RightHand", HumanBodyBones.LeftHand)

    };

        var bp = parseFile();
        string errorMsg = bp.Parse();
        if (errorMsg.Length > 0)
        {
            Debug.LogError("Parse fail: " + errorMsg);
            return;
        }

        modelDriver1 = Instantiate(driver, Vector3.zero, Quaternion.identity);
        modelDriver2 = Instantiate(driver, Vector3.zero, Quaternion.identity);

        modelDriver1.Init(bp, boneMaps);
        modelDriver2.Init(bp, boneMaps);

        Matrix4x4 controlPoints = modelDriver1.SolveFitCurve();

        path1.Init(false);
        path1.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        path1.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        path1.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        path1.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        path2.Init(true);
        path2.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        path2.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        path2.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        path2.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        StartCoroutine(Play(modelDriver1, path1));
    }



    public IEnumerator Play(NewBVHDriver bvd, RunTimeBezier bezier)
    {
        BVHParser parser = bvd.parser;
        int frame = 0;
        
        //List<float> chordLengthParamterList = loader.GetChordLengthParameterList();
        while (true)
        {
            bezier.LogicalUpdate(false);
            if (frame >= parser.frames)
                frame = 1;

            Vector3 rootLocalPos = new Vector3(
            parser.root.channels[0].values[frame],
            parser.root.channels[1].values[frame],
            parser.root.channels[2].values[frame]);


            int[] order = new int[3] { parser.root.channelOrder[3], parser.root.channelOrder[4], parser.root.channelOrder[5] };
            Quaternion rootLocalRot = ParserTool.Euler2Quat(new Vector3(
                parser.root.channels[3].values[frame],
                parser.root.channels[4].values[frame],
                parser.root.channels[5].values[frame]), order);

            skeleton1Root.transform.position = rootLocalPos;
            skeleton1Root.transform.rotation = rootLocalRot;

            bvd.Root().position = skeleton1Root.transform.position;
            //bvd.Root().rotation = rootLocalRot;

            UpdateNewMotion(bezier, skeleton1Root.transform, frame);

            //bvd.MotionUpdateByFrame(frame, bvd.Root().gameObject);
            bvd.MotionUpdateByFrame(frame, skeleton1Root);





            frame++;
            yield return new WaitForSeconds(parser.frameTime);
        }
    }

    /// <summary>
    /// update motion to the second model
    /// </summary>
    /// <param name="bezier"></param>
    /// <param name="mainRoot"></param>
    /// <param name="frame"></param>
    public void UpdateNewMotion(RunTimeBezier bezier, Transform originalRoot, int frame)
    {
        NewBVHDriver driver2 = modelDriver2;
        BVHParser parser = driver2.parser;

        path2.LogicalUpdate(IsShowPoint);
        if (frame >= parser.frames)
            frame = 0;

        float t = (float)frame / parser.frames;

        if (IsArcLength)
        {
            if (finalT > 1)
                finalT -= 1f;
            finalT = path2.ArcLengthProgress(finalT, path2.GetBezierLength(100), parser.frames - 1, speed);
            
        }
        else 
        {
            finalT = t;
        }

        Matrix4x4 TransformMatrix =
            path2.GetTranslationMatrix(finalT) *
            path2.GetRotationMatrix(finalT) *
            bezier.GetRotationMatrix(t).inverse *
            bezier.GetTranslationMatrix(t).inverse;

        Matrix4x4 FinalTransformMatrix = TransformMatrix * originalRoot.transform.localToWorldMatrix;

        skeleton2Root.transform.position = FinalTransformMatrix.ExtractPosition();
        skeleton2Root.transform.rotation = FinalTransformMatrix.ExtractRotation();

        driver2.Root().position = skeleton2Root.transform.position;
        //driver2.Root().rotation = FinalTransformMatrix.ExtractRotation();

        Color color = Color.blue;
        //Debug.DrawLine(driver2.Root().position, driver2.Root().position + driver2.Root().forward * 20, color);
        //Debug.DrawLine(originalRoot.transform.position, originalRoot.transform.position + originalRoot.transform.forward * 20, color);
        


        driver2.MotionUpdateByFrame(frame, skeleton2Root);

    }



}
