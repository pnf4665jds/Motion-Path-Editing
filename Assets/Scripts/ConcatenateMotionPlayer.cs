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
        secondLoader.Init(fileName2);

        secondParser = secondLoader.parser;

        Motion concatenateMotion = Concatenate();
        StartCoroutine(Play(firstLoader, firstBezier, concatenateMotion));
    }

    /// <summary>
    /// ����Motion
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public IEnumerator Play(BVHLoader loader, RunTimeBezier bezier, Motion concatenateMotion)
    {
        BVHParser parser = loader.parser;
        int frame = 1;

        while (true)
        {
            bezier.LogicalUpdate();
            if (frame >= concatenateMotion.totalFrameNum)
                frame = 1;

            Vector3 rootLocalPos = concatenateMotion.GetRootPosition(frame);

            int[] order = new int[3] { parser.root.channelOrder[3], parser.root.channelOrder[4], parser.root.channelOrder[5] };
            // ��qMotion�����ǥi�ण�P
            Quaternion rootLocalRot = ParserTool.Euler2Quat(concatenateMotion.GetRotation(parser.root.name, frame), order);

            loader.rootJoint.transform.position = rootLocalPos;
            loader.rootJoint.transform.rotation = rootLocalRot;

            foreach (BVHParser.BVHBone child in parser.root.children)
            {
                loader.SetupSkeleton(child, loader.rootJoint, frame, concatenateMotion);
            }

            frame++;
            yield return new WaitForSeconds(parser.frameTime);
        }
    }

    private Motion Concatenate()
    {
        int concatenateStartFrame = firstParser.frames;
        Dictionary<string, string> firstHierarchy = firstParser.getHierachy();
        // ���o�Ĥ@��Motion���̫�@�VData
        Dictionary<string, Vector3> firstKeyframeList = firstParser.getKeyFrameAsVector(concatenateStartFrame - 1);
        Vector3 lastPos = new Vector3(firstKeyframeList["pos"].x, firstKeyframeList["pos"].y, firstKeyframeList["pos"].z);
        Vector3 lastRot = firstKeyframeList[firstParser.root.name];

        // ���o�ĤG��Motion���Ĥ@�VData
        Dictionary<string, Vector3> secondKeyframeList = secondParser.getKeyFrameAsVector(1);
        Vector3 newPos = new Vector3(secondKeyframeList["pos"].x, secondKeyframeList["pos"].y, secondKeyframeList["pos"].z);
        Vector3 newRot = secondKeyframeList[secondParser.root.name];

        Vector3 offsetPos = lastPos - newPos;
        Vector3 offsetRot = lastRot - newRot;

        // �إߤ@��Motion�A�çQ�βĤ@�qMotion��l��
        Motion concatenateMotion = new Motion(firstParser);
       
        for(int i = 1; i < secondParser.frames; i++)
        {
            Dictionary<string, Vector3> newData = new Dictionary<string, Vector3>();
            Dictionary<string, Vector3> concatenateFrameData = secondParser.getKeyFrameAsVector(i);
            // �]���̭��w�g���Ĥ@�qMotion��Data�A�i�H��������
            Vector3 oldRootPos = concatenateFrameData["pos"];
            Vector3 oldRootRot = concatenateFrameData[firstParser.root.name];
            newData.Add("pos", oldRootPos - newPos + lastPos);
            newData.Add(firstParser.root.name, oldRootRot);
            // �N���Froot�H�~���U���`����Data�q�ĤG�qMotion�ƻs�L��
            foreach(string boneName in concatenateFrameData.Keys)
            {
                if (boneName != firstParser.root.name && boneName != "pos")
                {
                    newData.Add(boneName, concatenateFrameData[boneName]);
                }
            }
            concatenateMotion.AddNewFrame(newData);
        }
        concatenateMotion.Smooth(secondParser, concatenateStartFrame - 1, 30);

        return concatenateMotion;
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

    public Motion(BVHParser parser)
    {
        totalFrameNum = parser.frames;
        rootBone = parser.root;
        List<BVHParser.BVHBone> boneList = parser.getBoneList();

        frameDataList = new Dictionary<string, List<Vector3>>();
        frameDataList.Add("pos", new List<Vector3>());
        foreach (BVHParser.BVHBone bone in boneList)
        {
            frameDataList.Add(bone.name, new List<Vector3>());
        }

        for (int i = 0; i < totalFrameNum; i++) {
            Dictionary<string, Vector3> frameData = parser.getKeyFrameAsVector(i);
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
        if(newBoneData.Keys.Count != frameDataList.Keys.Count)
        {
            Debug.Log("New frame count not equal to old frame count");
            return;
        }
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
        totalFrameNum++;
    }

    /// <summary>
    /// Smooth inbetween motion 
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="concatenateStartFrame"></param>
    /// <param name="smoothWindow"></param>
    public void Smooth(BVHParser parser, int concatenateStartFrame, int smoothWindow)
    {
        int smoothStartFrame = concatenateStartFrame - smoothWindow;
        int smoothEndFrame = concatenateStartFrame + smoothWindow;

        List<BVHParser.BVHBone> boneList = parser.getBoneList();
        // ��l��offset list
        Dictionary<string, Vector3> offsetList = new Dictionary<string, Vector3>();
        offsetList.Add("pos", new Vector3());
        boneList.ForEach(bone => offsetList.Add(bone.name, new Vector3()));

        for (int frameIndex = smoothStartFrame; frameIndex < smoothEndFrame + 1; frameIndex++)
        {
            // �p��smooth�d��root���첾
            Vector3 previousPos = frameDataList["pos"][concatenateStartFrame];
            Vector3 newPos = frameDataList["pos"][concatenateStartFrame + 1];
            offsetList["pos"] = newPos - previousPos;

            foreach(BVHParser.BVHBone bone in boneList)
            {
                // �p��smooth�d�򤺨C��bone�������ܤƶq
                Vector3 previousRot = frameDataList[bone.name][concatenateStartFrame];
                Vector3 newRot = frameDataList[bone.name][concatenateStartFrame + 1];
                offsetList[bone.name] = newRot - previousRot;
            }
            if(frameIndex > 0 && frameIndex < totalFrameNum)
            {
                float smoothFactor = SmoothFunction(frameIndex, concatenateStartFrame, smoothWindow);
                frameDataList["pos"][frameIndex] += smoothFactor * offsetList["pos"];
                foreach (BVHParser.BVHBone bone in boneList)
                {
                    frameDataList[bone.name][frameIndex] += smoothFactor * offsetList[bone.name];
                }
            }
        }
    }

    private float SmoothFunction(int curremtFrame, int concatenateFrame, int smoothWindow)
    {
        // reference from maochinn and https://www.cs.toronto.edu/~jacobson/seminar/arikan-and-forsyth-2002.pdf
        float res = 0;
        int diff = curremtFrame - concatenateFrame;
        float diffNorm = (diff + smoothWindow) / smoothWindow;
        if (Mathf.Abs(diff) > smoothWindow)
        {
            res = 0;
        }
        else if (curremtFrame > concatenateFrame - smoothWindow && curremtFrame <= concatenateFrame)
        {
            res = 0.5f * (diffNorm * diffNorm);
        }
        else
        {
            res = -0.5f * (diffNorm * diffNorm) + 2 * diffNorm - 2;
        }

        return res;
    }
}