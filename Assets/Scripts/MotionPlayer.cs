using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionPlayer : MonoBehaviour
{
    // 檔名
    public string fileName = "walk_loop.bvh";
    public BVHLoader loader { get; set; }
    public BVHLoader secondLoader { get; set; }

    public RunTimeBezier mainBezier;
    public RunTimeBezier secondBezier;

    private float finalT = 0;

    private void Start()
    {
        loader = new BVHLoader();
        loader.Init(fileName);
        Matrix4x4 controlPoints = loader.SolveFitCurve();

        mainBezier.Init(false);
        mainBezier.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        mainBezier.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        mainBezier.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        mainBezier.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        // 關掉其中一條Curve的控制點
        mainBezier.concretePoints.ForEach(p => p.SetActive(false));

        secondLoader = new BVHLoader();
        secondLoader.Init(fileName);
        secondBezier.Init(true);
        secondBezier.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        secondBezier.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        secondBezier.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        secondBezier.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        StartCoroutine(Play(loader, mainBezier));
    }

    /// <summary>
    /// 播放Motion
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public IEnumerator Play(BVHLoader loader, RunTimeBezier bezier)
    {
        BVHParser parser = loader.parser;
        int frame = 0;
        List<float> chordLengthParamterList = loader.GetChordLengthParameterList();
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

            loader.rootJoint.transform.position = rootLocalPos;
            loader.rootJoint.transform.rotation = rootLocalRot;
  
            foreach (BVHParser.BVHBone child in parser.root.children)
            {
                loader.SetupSkeleton(child, loader.rootJoint, frame);
            }

            UpdateNewMotion(bezier, loader.rootJoint, frame);

            frame++;
            yield return new WaitForSeconds(parser.frameTime);
        }
    }

    public void UpdateNewMotion(RunTimeBezier bezier, GameObject mainRoot, int frame)
    {
        BVHParser parser = secondLoader.parser;

        secondBezier.LogicalUpdate();
        if (frame >= parser.frames)
            frame = 0;

        float t = (float)frame / parser.frames;
        finalT = secondBezier.ArcLengthProgress(finalT, secondBezier.GetBezierLength(100));
        if (finalT > 1)
            finalT -= 1f;

        Matrix4x4 TransformMatrix =
            secondBezier.GetTranslationMatrix(finalT) *
            secondBezier.GetRotationMatrix(finalT) *
            bezier.GetRotationMatrix(t).inverse *
            bezier.GetTranslationMatrix(t).inverse;

        Matrix4x4 FinalTransformMatrix = TransformMatrix * mainRoot.transform.localToWorldMatrix;
        secondLoader.rootJoint.transform.position = FinalTransformMatrix.ExtractPosition();
        //Debug.Log("Time: " + t + "  Pos: " + TransformMatrix.ExtractPosition());
        secondLoader.rootJoint.transform.rotation = FinalTransformMatrix.ExtractRotation();

        //loader.rootJoint.transform.position = mainRoot.transform.position;
        //Debug.Log("Time: " + t + "  Pos: " + TransformMatrix.ExtractPosition());
        //loader.rootJoint.transform.rotation = mainRoot.transform.rotation;

        foreach (BVHParser.BVHBone child in parser.root.children)
        {
            secondLoader.SetupSkeleton(child, secondLoader.rootJoint, frame);
        }
    }
}
