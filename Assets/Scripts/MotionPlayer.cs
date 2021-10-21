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

        StartCoroutine(Play(loader));
    }

    /// <summary>
    /// ºΩ©ÒMotion
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public IEnumerator Play(BVHLoader loader)
    {
        BVHParser parser = loader.parser;
        int frame = 0;
        while (true)
        {
            if (frame >= parser.frames)
                frame = 0;
            loader.rootJoint.transform.position = new Vector3(
                parser.root.channels[0].values[frame],
                parser.root.channels[1].values[frame],
                parser.root.channels[2].values[frame]);

            loader.rootJoint.transform.rotation = loader.Euler2Quat(new Vector3(
                parser.root.channels[3].values[frame],
                parser.root.channels[4].values[frame],
                parser.root.channels[5].values[frame]));

            foreach (BVHParser.BVHBone child in parser.root.children)
            {
                loader.SetupSkeleton(child, loader.rootJoint, frame);
            }

            frame++;
            yield return new WaitForSeconds(parser.frameTime);
        }
    }

    private Matrix4x4 GetTransformMatrix(float t)
    {
        Vector3 pos = Bezier.GetPoint(
            mainBezier.concretePoints[0].transform.position,
            mainBezier.concretePoints[1].transform.position,
            mainBezier.concretePoints[2].transform.position,
            mainBezier.concretePoints[3].transform.position,
            t);
        return new Matrix4x4();
    }
}
