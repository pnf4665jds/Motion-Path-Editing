using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionPlayer : MonoBehaviour
{
    // ¿…¶W
    public string fileName = "walk_loop.bvh";
    public BVHLoader loader { get; set; }
    public BVHLoader secondLoader { get; set; }

    public RunTimeBezier mainBezier;
    public RunTimeBezier secondBezier;

    private void Start()
    {
        loader = new BVHLoader();
        loader.Init(fileName);
        Matrix4x4 controlPoints = loader.SolveFitCurve();

        mainBezier.Init();
        mainBezier.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        mainBezier.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        mainBezier.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        mainBezier.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        secondLoader = new BVHLoader();
        secondLoader.Init(fileName);
        secondBezier.Init();
        secondBezier.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        secondBezier.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        secondBezier.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        secondBezier.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        StartCoroutine(Play(loader, mainBezier));
    }

    /// <summary>
    /// ºΩ©ÒMotion
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
                frame = 0;

            Vector3 rootLocalPos = new Vector3(
            parser.root.channels[0].values[frame],
            parser.root.channels[1].values[frame],
            parser.root.channels[2].values[frame]);

            Quaternion rootLocalRot = loader.Euler2Quat(new Vector3(
                parser.root.channels[3].values[frame],
                parser.root.channels[4].values[frame],
                parser.root.channels[5].values[frame]));

            float t = chordLengthParamterList[frame];

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
        BVHLoader loader = secondLoader;
        BVHParser parser = loader.parser;

        secondBezier.LogicalUpdate();
        if (frame >= parser.frames)
            frame = 0;

        float t = (float)frame / parser.frames;

        Matrix4x4 TransformMatrix =
            secondBezier.GetTranslationMatrix(t) *
            secondBezier.GetRotationMatrix(t) *
            bezier.GetRotationMatrix(t).inverse *
            bezier.GetTranslationMatrix(t).inverse;

        Matrix4x4 FinalTransformMatrix = TransformMatrix * mainRoot.transform.localToWorldMatrix;
        loader.rootJoint.transform.position = FinalTransformMatrix.ExtractPosition();
        //Debug.Log("Time: " + t + "  Pos: " + TransformMatrix.ExtractPosition());
        loader.rootJoint.transform.rotation = FinalTransformMatrix.ExtractRotation();

        foreach (BVHParser.BVHBone child in parser.root.children)
        {
            loader.SetupSkeleton(child, loader.rootJoint, frame);
        }
    }
}
