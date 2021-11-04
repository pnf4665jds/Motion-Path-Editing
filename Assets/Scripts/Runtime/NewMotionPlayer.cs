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
    public string fileName;
    public NewBVHDriver driver;
    public RunTimeBezier path1;
    public RunTimeBezier path2;

    private NewBVHDriver modelDriver1;
    private NewBVHDriver modelDriver2;

    GameObject skeleton1Root;
    GameObject skeleton2Root;

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
        modelDriver1 = Instantiate(driver, Vector3.zero, Quaternion.identity);
        modelDriver2 = Instantiate(driver, Vector3.zero, Quaternion.identity);

        modelDriver1.Init(bp, boneMaps);
        modelDriver2.Init(bp, boneMaps);

        Matrix4x4 controlPoints = modelDriver1.SolveFitCurve();

        path1.Init();
        path1.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        path1.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        path1.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        path1.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        path2.Init();
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
            bezier.LogicalUpdate();
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

            bvd.Root().position = rootLocalPos;
            bvd.Root().rotation = rootLocalRot;

            UpdateNewMotion(bezier, skeleton1Root.transform, frame);

            bvd.MotionUpdateByFrame(frame, bvd.Root().gameObject);
            /*foreach (BVHParser.BVHBone child in parser.root.children)
            {

                bvd.SetupSkeleton(child, bvd.Root().rotation, 0);
            }*/

            
           

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

        path2.LogicalUpdate();
        if (frame >= parser.frames)
            frame = 0;

        float t = (float)frame / parser.frames;

        Matrix4x4 TransformMatrix =
            path2.GetTranslationMatrix(t) *
            path2.GetRotationMatrix(t) *
            bezier.GetRotationMatrix(t).inverse *
            bezier.GetTranslationMatrix(t).inverse;

        Matrix4x4 FinalTransformMatrix = TransformMatrix * originalRoot.transform.localToWorldMatrix;

        skeleton2Root.transform.position = FinalTransformMatrix.ExtractPosition();
        skeleton2Root.transform.rotation = FinalTransformMatrix.ExtractRotation();
        driver2.Root().position = FinalTransformMatrix.ExtractPosition();
        driver2.Root().rotation = FinalTransformMatrix.ExtractRotation();
        Color color = Color.blue;
        Debug.DrawLine(driver2.Root().position, driver2.Root().position + driver2.Root().forward * 20, color);
        Debug.DrawLine(originalRoot.transform.position, originalRoot.transform.position + originalRoot.transform.forward * 20, color);
        if (driver2.Root().rotation != originalRoot.transform.rotation) 
        {
            print("different" + driver2.Root().rotation   + " , " + originalRoot.transform.rotation);
        }
        //driver2.Root().position = originalRoot.transform.position;
       // driver2.Root().rotation = originalRoot.transform.rotation;

        
        //driver2.UpdateRootMotion(FinalTransformMatrix.ExtractPosition(),
        //FinalTransformMatrix.ExtractRotation(), 1f);


        //driver2.MotionUpdate(frame, driver2.Root().rotation);
        /*foreach(string bname in driver2.bvhHireachy.Keys) 
        {
            if (driver2.bvhHireachy[bname] == "Hips") 
            {
                driver2.MotionUpdate2(bname, driver2.Root(), frame);

            }
        }*/
        /*foreach (BVHParser.BVHBone child in parser.root.children) 
        {

            driver2.SetupSkeleton(child, driver2.Root().rotation, 0);
        }*/

        driver2.MotionUpdateByFrame(frame, driver2.Root().gameObject);

    }




}
