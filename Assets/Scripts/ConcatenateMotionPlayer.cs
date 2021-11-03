using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcatenateMotionPlayer : MonoBehaviour
{
    public string fileName1 = "walk_loop.bvh";
    public string fileName2 = "dance.bvh";
    public BVHLoader firstLoader;
    public BVHLoader secondLoader;
    public RunTimeBezier firstBezier;
    public RunTimeBezier secondBezier;

    private BVHParser firstParser;
    private BVHParser secondParser;

    private void Start()
    {
        // 建置第一個Motion相關參數
        firstLoader = new BVHLoader();
        firstLoader.Init(fileName1);
        Matrix4x4 controlPoints = firstLoader.SolveFitCurve();

        firstParser = firstLoader.parser;

        firstBezier.Init();
        firstBezier.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        firstBezier.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        firstBezier.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        firstBezier.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        // 建置第二個Motion相關參數
        secondLoader = new BVHLoader();
        secondLoader.Init(fileName1);

        secondParser = secondLoader.parser;

        Concatenate();
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
            Quaternion rootLocalRot = loader.Euler2Quat(new Vector3(
                parser.root.channels[3].values[frame],
                parser.root.channels[4].values[frame],
                parser.root.channels[5].values[frame]), order);

            loader.rootJoint.transform.position = rootLocalPos;
            loader.rootJoint.transform.rotation = rootLocalRot;

            foreach (BVHParser.BVHBone child in parser.root.children)
            {
                loader.SetupSkeleton(child, loader.rootJoint, frame);
            }

            frame++;
            yield return new WaitForSeconds(parser.frameTime);
        }
    }

    private void Concatenate()
    {
        int concatenateStartFrame = firstParser.frames;
        Dictionary<string, string> firstHierarchy = firstParser.getHierachy();
        // 取得第一個Motion的最後一幀Data
        Dictionary<string, Quaternion> firstKeyframeList = firstParser.getKeyFrame(concatenateStartFrame - 1);
        Vector3 lastPos = new Vector3(firstKeyframeList["pos"].x, firstKeyframeList["pos"].y, firstKeyframeList["pos"].z);
        Quaternion lastRot = firstKeyframeList[firstParser.root.name];

        // 取得第二個Motion的第一幀Data
        Dictionary<string, Quaternion> secondKeyframeList = firstParser.getKeyFrame(0);
        Vector3 newPos = new Vector3(secondKeyframeList["pos"].x, secondKeyframeList["pos"].y, secondKeyframeList["pos"].z);
        Quaternion newRot = secondKeyframeList[secondParser.root.name];

        Vector3 offsetPos = lastPos - newPos;
        Quaternion offsetRot = lastRot - newRot;
    }
}
