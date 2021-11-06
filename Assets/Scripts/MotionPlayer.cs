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
    public bool isArcLength { get; set; } = false;
    public float speed { get; set; } = 1;

    private float finalT = 0;
    private float tempFinalT = 0;
    private Vector3 offsetPos;  // 紀錄Motion頭尾幀的位移量
    private Quaternion lastRot, firstRot; // 紀錄Motion頭尾幀的旋轉變化量
    private GameObject tempObject;  // 協助Smooth的物件
    private GameObject tempObject2;
    private Vector3 lastRootPos, firstRootPos;
    private Quaternion lastRootRot, firstRootRot;
    private IEnumerator PlayMotion;
    private int stepNum;
    private bool first = true;

    public void Init(string filePath)
    {
        tempObject = new GameObject();
        tempObject2 = new GameObject();

        loader = new BVHLoader();
        loader.Init(filePath, Color.blue);
        Matrix4x4 controlPoints = loader.SolveFitCurve();

        stepNum = loader.parser.frames - 1;

        mainBezier.Init(false);
        mainBezier.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        mainBezier.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        mainBezier.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        mainBezier.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        // 關掉其中一條Curve的控制點
        mainBezier.concretePoints.ForEach(p => p.SetActive(false));

        secondLoader = new BVHLoader();
        secondLoader.Init(filePath, Color.red);
        secondBezier.Init(true);
        secondBezier.concretePoints[0].transform.position = new Vector3(controlPoints.m00, controlPoints.m01, controlPoints.m02) + new Vector3(0, 0, 0);
        secondBezier.concretePoints[1].transform.position = new Vector3(controlPoints.m10, controlPoints.m11, controlPoints.m12) + new Vector3(0, 0, 0);
        secondBezier.concretePoints[2].transform.position = new Vector3(controlPoints.m20, controlPoints.m21, controlPoints.m22) + new Vector3(0, 0, 0);
        secondBezier.concretePoints[3].transform.position = new Vector3(controlPoints.m30, controlPoints.m31, controlPoints.m32) + new Vector3(0, 0, 0);

        PlayMotion = Play(loader, mainBezier);

        StartCoroutine(PlayMotion);
    }

    public void Stop()
    {
        if (PlayMotion != null)
        {
            loader.Stop();
            secondLoader.Stop();
            mainBezier.ClearBezier();
            secondBezier.ClearBezier();
            StopCoroutine(PlayMotion);
        }
        finalT = 0;
        tempFinalT = 0;
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

        int[] order = new int[3] { parser.root.channelOrder[3], parser.root.channelOrder[4], parser.root.channelOrder[5] };

        firstRootPos = new Vector3(
               parser.root.channels[0].values[0],
               parser.root.channels[1].values[0],
               parser.root.channels[2].values[0]);

        firstRootRot = ParserTool.Euler2Quat(new Vector3(
            parser.root.channels[3].values[0],
            parser.root.channels[4].values[0],
            parser.root.channels[5].values[0]), order);

        lastRootPos = new Vector3(
               parser.root.channels[0].values[parser.frames - 1],
               parser.root.channels[1].values[parser.frames - 1],
               parser.root.channels[2].values[parser.frames - 1]);

        lastRootRot = ParserTool.Euler2Quat(new Vector3(
            parser.root.channels[3].values[parser.frames - 1],
            parser.root.channels[4].values[parser.frames - 1],
            parser.root.channels[5].values[parser.frames - 1]), order);

        while (true)
        {
            bezier.LogicalUpdate();
            if (frame >= parser.frames)
                frame = 0;

            Vector3 rootLocalPos = new Vector3(
            parser.root.channels[0].values[frame],
            parser.root.channels[1].values[frame],
            parser.root.channels[2].values[frame]);
            //Debug.Log(frame);

            //int[] order = new int[3] { parser.root.channelOrder[3], parser.root.channelOrder[4], parser.root.channelOrder[5] };
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

        float t = (float)frame / (parser.frames - 1);
        if (isArcLength)
        {
            if (finalT > 1)
                finalT -= 1f;
            finalT = secondBezier.ArcLengthProgress(finalT, secondBezier.GetBezierLength(100), stepNum, speed);

        }
        else
        {
            finalT = t;
        }

        Matrix4x4 TransformMatrix =
            secondBezier.GetTranslationMatrix(finalT) *
            secondBezier.GetRotationMatrix(finalT) *
            bezier.GetRotationMatrix(t).inverse *
            bezier.GetTranslationMatrix(t).inverse;

        // 計算這次的頭尾幀差距
        if (first || frame == parser.frames - 1 - 30)
        {
            first = false;
            for(int i = 0; i < parser.frames; i++)
            {
                tempFinalT = secondBezier.ArcLengthProgress(tempFinalT, secondBezier.GetBezierLength(100), stepNum, speed);
            }
            float nextFinalT = secondBezier.ArcLengthProgress(tempFinalT, secondBezier.GetBezierLength(100), stepNum, speed);

            Matrix4x4 lastTransformMatrix =
            secondBezier.GetTranslationMatrix(tempFinalT) *
            secondBezier.GetRotationMatrix(tempFinalT) *
            bezier.GetRotationMatrix(1).inverse *
            bezier.GetTranslationMatrix(1).inverse;

            Matrix4x4 firstTransformMatrix =
            secondBezier.GetTranslationMatrix(nextFinalT) *
            secondBezier.GetRotationMatrix(nextFinalT) *
            bezier.GetRotationMatrix(0).inverse *
            bezier.GetTranslationMatrix(0).inverse;

            tempObject.transform.position = lastRootPos;
            tempObject.transform.rotation = lastRootRot;
            tempObject2.transform.position = firstRootPos;
            tempObject2.transform.rotation = firstRootRot;

            Vector3 lastPos = (lastTransformMatrix * tempObject.transform.localToWorldMatrix).ExtractPosition();
            Vector3 firstPos = (firstTransformMatrix * tempObject2.transform.localToWorldMatrix).ExtractPosition();

            //lastRot = (lastTransformMatrix * tempObject.transform.localToWorldMatrix).ExtractRotation();
            //firstRot = (firstTransformMatrix * tempObject2.transform.localToWorldMatrix).ExtractRotation();

            offsetPos = firstPos - lastPos;
        }

        //tempObject.transform.position += SelfConcatenateSmoothFunction(frame, parser.frames - 1, 30) * offsetPos;

        Matrix4x4 FinalTransformMatrix = TransformMatrix * mainRoot.transform.localToWorldMatrix;
        //Matrix4x4 FinalTransformMatrix = TransformMatrix * mainRoot.transform.localToWorldMatrix;
        secondLoader.rootJoint.transform.position = FinalTransformMatrix.ExtractPosition();
        secondLoader.rootJoint.transform.rotation = FinalTransformMatrix.ExtractRotation();

        if (isArcLength)
        {
            float factor = SelfConcatenateSmoothFunction(frame, parser.frames - 1, 30);

            secondLoader.rootJoint.transform.position += factor * offsetPos;
            //if (factor > 0)
                //secondLoader.rootJoint.transform.rotation *= Quaternion.Slerp(lastRot, firstRot, SelfConcatenateSmoothFunction(frame, parser.frames - 1, 30));
            //else
                //secondLoader.rootJoint.transform.rotation *= Quaternion.Slerp(firstRot, lastRot, SelfConcatenateSmoothFunction(frame, parser.frames - 1, 30) * -1);
        }

        foreach (BVHParser.BVHBone child in parser.root.children)
        {
            secondLoader.SetupSkeleton(child, secondLoader.rootJoint, frame);
        }
    }

    private float SelfConcatenateSmoothFunction(int curremtFrame, int concatenateFrame, int smoothWindow)
    {
        // reference from maochinn and https://www.cs.toronto.edu/~jacobson/seminar/arikan-and-forsyth-2002.pdf
        if(curremtFrame < smoothWindow)
        {
            curremtFrame += (concatenateFrame + 1);
        }

        float res = 0;
        int diff = curremtFrame - concatenateFrame;
        float diffNorm = ((float)(diff + smoothWindow)) / smoothWindow;
        if (Mathf.Abs(diff) >= smoothWindow)
        {
            res = 0;
        }
        else if (curremtFrame > (concatenateFrame - smoothWindow) && curremtFrame <= concatenateFrame)
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
