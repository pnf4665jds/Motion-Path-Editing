using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleMotionPlayer : MonoBehaviour
{
    public BVHLoader loader { get; set; }
    public RunTimeBezier bezier;
    
    private IEnumerator PlayMotion;
    
    public void Init(string filePath, Vector3 basePos)
    {
        loader = new BVHLoader();
        if(!loader.Init(filePath, Color.blue))
        {
            return;
        }
        Matrix4x4 controlPoints = loader.SolveFitCurve();

        bezier.Init(false);

        Vector3 firstPos = new Vector3(loader.parser.root.channels[0].values[1],
            loader.parser.root.channels[1].values[1],
            loader.parser.root.channels[2].values[1]);

        Vector3 offset = basePos - firstPos;

        bezier.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + offset;
        bezier.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + offset;
        bezier.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + offset;
        bezier.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + offset;

        PlayMotion = Play(loader, bezier, offset);

        StartCoroutine(PlayMotion);
    }

    public void Stop()
    {
        if (PlayMotion != null)
        {
            loader.Stop();
            bezier.ClearBezier();
            StopCoroutine(PlayMotion);
        }
    }

    /// <summary>
    /// ºΩ©ÒMotion
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public IEnumerator Play(BVHLoader loader, RunTimeBezier bezier, Vector3 offset)
    {
        BVHParser parser = loader.parser;
        int frame = 1;

        int[] order = new int[3] { parser.root.channelOrder[3], parser.root.channelOrder[4], parser.root.channelOrder[5] };

        while (true)
        {
            bezier.LogicalUpdate(false);
            if (frame >= parser.frames)
                frame = 1;

            Vector3 rootLocalPos = new Vector3(
            parser.root.channels[0].values[frame],
            parser.root.channels[1].values[frame],
            parser.root.channels[2].values[frame]);

            Quaternion rootLocalRot = ParserTool.Euler2Quat(new Vector3(
                parser.root.channels[3].values[frame],
                parser.root.channels[4].values[frame],
                parser.root.channels[5].values[frame]), order);

            loader.rootJoint.transform.position = rootLocalPos + offset;
            loader.rootJoint.transform.rotation = rootLocalRot;

            foreach (BVHParser.BVHBone child in parser.root.children)
            {
                loader.SetupSkeleton(child, loader.rootJoint, frame);
            }

            frame++;
            yield return new WaitForSeconds(parser.frameTime);
        }
    }
}
