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
        // �ظm�Ĥ@��Motion�����Ѽ�
        firstLoader = new BVHLoader();
        firstLoader.Init(fileName1);
        Matrix4x4 controlPoints = firstLoader.SolveFitCurve();

        firstParser = firstLoader.parser;

        firstBezier.Init();
        firstBezier.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        firstBezier.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        firstBezier.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        firstBezier.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        // �ظm�ĤG��Motion�����Ѽ�
        secondLoader = new BVHLoader();
        secondLoader.Init(fileName1);

        secondParser = secondLoader.parser;

        Concatenate();
    }

    /// <summary>
    /// ����Motion
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

            frame++;
            yield return new WaitForSeconds(parser.frameTime);
        }
    }

    private void Concatenate()
    {
        int concatenateStartFrame = firstParser.frames;
        Dictionary<string, string> firstHierarchy = firstParser.getHierachy();
        // ���o�Ĥ@��Motion���̫�@�VData
        Dictionary<string, Vector3> firstKeyframeList = firstParser.getKeyFrameAsVector(concatenateStartFrame - 1);
        Vector3 lastPos = new Vector3(firstKeyframeList["pos"].x, firstKeyframeList["pos"].y, firstKeyframeList["pos"].z);
        Vector3 lastRot = firstKeyframeList[firstParser.root.name];

        // ���o�ĤG��Motion���Ĥ@�VData
        Dictionary<string, Vector3> secondKeyframeList = secondParser.getKeyFrameAsVector(0);
        Vector3 newPos = new Vector3(secondKeyframeList["pos"].x, secondKeyframeList["pos"].y, secondKeyframeList["pos"].z);
        Vector3 newRot = secondKeyframeList[secondParser.root.name];

        Vector3 offsetPos = lastPos - newPos;
        Vector3 offsetRot = lastRot - newRot;

        Motion newMotion = new Motion(firstParser);
        for(int i = 0; i < secondParser.frames; i++)
        {

        }
    }
}

/// <summary>
/// �o��class�Ψ��x�s�i�H�Q��ʪ�Motion Data
/// </summary>
public class Motion
{
    public int totalFrameNum;   // �`Frame�ƶq

    private Dictionary<string, List<Vector3>> frameDataList; // �C�Ӱ��[������List�AList�x�s�C�V������ȡCPos�S�O�x�sroot����m
    private BVHParser.BVHBone rootBone;

    public Motion()
    {

    }

    public Motion(BVHParser parser)
    {
        totalFrameNum = parser.frames;
        rootBone = parser.root;
        List<BVHParser.BVHBone> boneList = parser.getBoneList();

        frameDataList["pos"] = new List<Vector3>();
        foreach (BVHParser.BVHBone bone in boneList)
        {
            frameDataList[bone.name] = new List<Vector3>();
        }

        for (int i = 0; i < totalFrameNum; i++) {
            var frameData = parser.getKeyFrameAsVector(i);
            Vector3 rootPos = frameData["pos"];
            frameDataList["pos"].Add(rootPos);
            foreach(BVHParser.BVHBone bone in parser.getBoneList())
            {
                Vector3 rot = frameData[bone.name];
                frameDataList[bone.name].Add(rot);
            }
        }
    }

    public Vector3 GetRootPosition(int frame)
    {
        return frameDataList["pos"][frame];
    }

    public Vector3 GetRotation(string name, int frame)
    {
        return frameDataList[name][frame];
    }

    /// <summary>
    /// �s�W�@��Frame�����
    /// </summary>
    /// <param name="newBoneData"></param>
    public void AddNewFrame(Dictionary<string, Vector3> newBoneData)
    {
        foreach(string bone in newBoneData.Keys)
        {
            
            if (newBoneData.TryGetValue(bone, out Vector3 data))
            {
                frameDataList[bone].Add(data);
            }
            else
            {
                Debug.LogError("Add new frame failed! Can't find " + bone);
            }
        }
    }
}