using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class NewBVHDriver : MonoBehaviour
{
    [Header("Loader settings")]
    [Tooltip("This is the target avatar for which the animation should be loaded. Bone names should be identical to those in the BVH file and unique. All bones should be initialized with zero rotations. This is usually the case for VRM avatars.")]
    public Animator targetAvatar;

    [Tooltip("If the flag above is disabled, the frame rate given in the BVH file will be overridden by this value.")]
    public float frameRate = 60.0f;
    [Tooltip("If the BVH first frame is T(if not,make sure the defined skeleton is T).")]
    public bool FirstT = true;


    [Tooltip("If the flag above is disabled, the frame rate given in the BVH file will be overridden by this value.")]
    public NewMotionPlayer.BoneMap[] bonemaps; // the corresponding bones between unity and bvh
    public BVHParser parser = null;
    private Animator anim;

    // This function doesn't call any Unity API functions and should be safe to call from another thread
   

    private Dictionary<string, Quaternion> bvhT; //記錄第 0 frame的旋轉值
    private Dictionary<string, Vector3> newBvhT; //記錄第 0 frame的旋轉值
    private Dictionary<string, Vector3> bvhOffset;
    public Dictionary<string, string> bvhHireachy;
    public Dictionary<HumanBodyBones, Quaternion> unityT;
    private Dictionary<string, HumanBodyBones> BVHToAvatar;

    private float scaleRatio = 0.0f;

    private List<float> chordLengthParameter;

    public void Init(BVHParser bp, NewMotionPlayer.BoneMap[] boneMaps)
    {
        BVHToAvatar = new Dictionary<string, HumanBodyBones>();
        for (int i = 0; i < boneMaps.Length; i++) 
        {
            BVHToAvatar.Add(boneMaps[i].bvh_name, boneMaps[i].humanoid_bone);
        }
        this.parser = bp;
        this.bonemaps = boneMaps;

        chordLengthParameter = new List<float>();
        Application.targetFrameRate = (Int16)frameRate;

        bvhT = bp.getKeyFrame(0);

        newBvhT = bp.getKeyFrameAsVector(0);

        bvhOffset = bp.getOffset(1.0f);
        bvhHireachy = bp.getHierachy();

        anim = targetAvatar.GetComponent<Animator>();
        unityT = new Dictionary<HumanBodyBones, Quaternion>();
        foreach (NewMotionPlayer.BoneMap bm in bonemaps)
        {
            unityT.Add(bm.humanoid_bone, anim.GetBoneTransform(bm.humanoid_bone).rotation);
        }

        float unity_leftleg = (anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position - anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position).sqrMagnitude +
            (anim.GetBoneTransform(HumanBodyBones.LeftFoot).position - anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position).sqrMagnitude;
        float bvh_leftleg = 0.0f;
        foreach (NewMotionPlayer.BoneMap bm in bonemaps)
        {
            if (bm.humanoid_bone == HumanBodyBones.LeftUpperLeg || bm.humanoid_bone == HumanBodyBones.LeftLowerLeg)
            {
                bvh_leftleg = bvh_leftleg + bvhOffset[bm.bvh_name].sqrMagnitude;
            }
        }
        scaleRatio = unity_leftleg / bvh_leftleg;
    }
    public float GetScaleRatio() 
    {
        return scaleRatio;
    }
    public Transform Root() 
    {
        return anim.GetBoneTransform(bonemaps[0].humanoid_bone) ;
    }

    public void MotionUpdate(int frameIdx, Quaternion root)
     {
        Dictionary<string, Quaternion> currFrame = parser.getKeyFrame(frameIdx);//frameIdx 2871

         foreach (NewMotionPlayer.BoneMap bm in bonemaps)
         {
              if (bm.humanoid_bone == HumanBodyBones.Hips) {
                Transform currBone = anim.GetBoneTransform(bm.humanoid_bone);
                currBone.rotation = (root * Quaternion.Inverse(bvhT[bm.bvh_name])) * unityT[bm.humanoid_bone];

                }
             if (FirstT)
             {
                
                Transform currBone = anim.GetBoneTransform(bm.humanoid_bone);
                currBone.rotation =(currFrame[bm.bvh_name] * Quaternion.Inverse(bvhT[bm.bvh_name])) * unityT[bm.humanoid_bone] ;
                

            }
             else
             {
                Transform currBone = anim.GetBoneTransform(bm.humanoid_bone);
                currBone.rotation =  currFrame[bm.bvh_name] * unityT[bm.humanoid_bone];
                
            }

         }

         // draw bvh skeleton
         Dictionary<string, Vector3> bvhPos = new Dictionary<string, Vector3>();
         foreach (string bname in currFrame.Keys)
         {
             if (bname == "pos")
             {
                 bvhPos.Add(parser.root.name, new Vector3(currFrame["pos"].x, currFrame["pos"].y, currFrame["pos"].z));
             }
             else
             {
                 if (bvhHireachy.ContainsKey(bname) && bname != parser.root.name)
                 {
                     Vector3 curpos = bvhPos[bvhHireachy[bname]] + currFrame[bvhHireachy[bname]] * bvhOffset[bname];
                     bvhPos.Add(bname, curpos);
                 }
             }
         }

         //anim.GetBoneTransform(HumanBodyBones.Hips).position = new Vector3(bvhPos[parser.root.name].x, bvhPos[parser.root.name].y*scaleRatio, bvhPos[parser.root.name].z);
        drawSleleton(bvhPos);
    }
    public void MotionUpdate2(string currentBone, Transform parent, int frame)
    {
        Dictionary<string, Quaternion> currFrame = parser.getKeyFrame(frame);

        //Quaternion parent_rot = currFrame[parent];
        Quaternion current_rot = currFrame[currentBone];

        

        //get the anim transform
        HumanBodyBones currentJoint;

        if (BVHToAvatar.TryGetValue(currentBone, out currentJoint)) 
        {
            Quaternion T1 = parent.transform.rotation * current_rot;
            Quaternion T2 = parent.transform.rotation * bvhT[currentBone];
            
            //anim.GetBoneTransform(currentJoint).rotation = parent.rotation * current_rot ;
            var rotationc = (current_rot * Quaternion.Inverse(bvhT[currentBone]))* unityT[currentJoint] ;
            //var rotationc = (T1 * Quaternion.Inverse(T2)) * unityT[currentJoint];
            anim.GetBoneTransform(currentJoint).rotation =  rotationc;
            //print(rotationc);
        }

        foreach (string child in bvhHireachy.Keys)
        {
            if (bvhHireachy[child] == currentBone)
            {
                MotionUpdate2(child, anim.GetBoneTransform(currentJoint), frame);
            }
        }
    }

    public void SetupSkeleton(BVHParser.BVHBone currentBone, Transform parent, int frame)
    {

        Vector3 offset = new Vector3(currentBone.offsetX, currentBone.offsetY, currentBone.offsetZ);
        Vector3 rotateVector = new Vector3(
            currentBone.channels[3].values[frame],
            currentBone.channels[4].values[frame],
            currentBone.channels[5].values[frame]);

        int[] order = new int[3] { currentBone.channelOrder[0], currentBone.channelOrder[1], currentBone.channelOrder[2] };
        Quaternion rotation = ParserTool.Euler2Quat(rotateVector, currentBone.channelOrder);


        //get the anim transform
        HumanBodyBones currentJoint;

        if (BVHToAvatar.TryGetValue(currentBone.name, out currentJoint))
        {
            Quaternion T3 = parent.transform.rotation * rotation;
            Quaternion T2 = parent.transform.rotation * ParserTool.Euler2Quat(newBvhT[currentBone.name], currentBone.channelOrder);

            //anim.GetBoneTransform(currentJoint).rotation = parent.rotation * current_rot ;
            //var rotationc = (current_rot * Quaternion.Inverse(bvhT[currentBone]))* unityT[currentJoint] ;
            var rotationc = (T3 * Quaternion.Inverse(T2)) * unityT[currentJoint];
            anim.GetBoneTransform(currentJoint).rotation = rotationc;
            //print(rotationc);

            
        }
        foreach (BVHParser.BVHBone child in currentBone.children)
        {
            SetupSkeleton(child, anim.GetBoneTransform(currentJoint), frame);
        }

    }

    public void UpdateRootMotion(Vector3 pos, Quaternion quat, float scaleRatio) 
    {
        anim.GetBoneTransform(bonemaps[0].humanoid_bone).position = new Vector3(pos.x, pos.y*scaleRatio, pos.z);
        anim.GetBoneTransform(bonemaps[0].humanoid_bone).rotation = quat /** unityT[bonemaps[0].humanoid_bone]*/;
    }
    private void drawSleleton(Dictionary<string, Vector3> bvhPos) 
    {
        foreach (string bname in bvhHireachy.Keys)
        {
            Color color = new Color(1.0f, 0.0f, 0.0f);
            Debug.DrawLine(bvhPos[bname], bvhPos[bvhHireachy[bname]], color);
        }
    }



    public void SetUpArcLengthParameter()
    {
        float QX, QY, QZ;
        float QX_old, QY_old, QZ_old;
        // 計算chord-length
        float d = 0, t = 0;
        for (int i = 1; i < parser.frames; i++)
        {
            Vector3 point1 = new Vector3(parser.root.channels[0].values[i - 1], parser.root.channels[1].values[i - 1], parser.root.channels[2].values[i - 1]);
            Vector3 point2 = new Vector3(parser.root.channels[0].values[i], parser.root.channels[1].values[i], parser.root.channels[2].values[i]);
            d += Vector3.Distance(point1, point2);
        }

        chordLengthParameter.Add(0);
        for (int i = 1; i < parser.frames; i++)
        {
            QX = parser.root.channels[0].values[i];
            QY = parser.root.channels[1].values[i];
            QZ = parser.root.channels[2].values[i];

            QX_old = parser.root.channels[0].values[i - 1];
            QY_old = parser.root.channels[1].values[i - 1];
            QZ_old = parser.root.channels[2].values[i - 1];

            t = chordLengthParameter[i - 1] + Vector3.Distance(new Vector3(QX_old, QY_old, QZ_old), new Vector3(QX, QY, QZ)) / d;
            chordLengthParameter.Add(t);
        }
        chordLengthParameter[chordLengthParameter.Count - 1] = 1;
    }

    /// <summary>
    /// 找到fit motion data的cubic b-spline控制點
    /// </summary>
    public Matrix4x4 SolveFitCurve()
    {
        SetUpArcLengthParameter();
        float frameTime = 1.0f / parser.frames;
        //parser.root.channels[0].values[0]
        Matrix4x4 matA = Matrix4x4.zero;
        Matrix4x4 matB = Matrix4x4.zero;

        float B0, B1, B2, B3;
        float QX, QY, QZ;

        float t;

        for (int i = 0; i < parser.frames; i++)
        {
            QX = parser.root.channels[0].values[i];
            QY = parser.root.channels[1].values[i];
            QZ = parser.root.channels[2].values[i];

            t = chordLengthParameter[i];

            B0 = Bezier.Get_B_Zero(t);
            B1 = Bezier.Get_B_One(t);
            B2 = Bezier.Get_B_Two(t);
            B3 = Bezier.Get_B_Three(t);

            // 設定A矩陣
            matA.m00 += B0 * B0;
            matA.m01 += B0 * B1;
            matA.m02 += B0 * B2;
            matA.m03 += B0 * B3;
            matA.m10 += B1 * B0;
            matA.m11 += B1 * B1;
            matA.m12 += B1 * B2;
            matA.m13 += B1 * B3;
            matA.m20 += B2 * B0;
            matA.m21 += B2 * B1;
            matA.m22 += B2 * B2;
            matA.m23 += B2 * B3;
            matA.m30 += B3 * B0;
            matA.m31 += B3 * B1;
            matA.m32 += B3 * B2;
            matA.m33 += B3 * B3;

            // 設定B矩陣
            matB.m00 += B0 * QX;
            matB.m01 += B0 * QY;
            matB.m02 += B0 * QZ;
            matB.m10 += B1 * QX;
            matB.m11 += B1 * QY;
            matB.m12 += B1 * QZ;
            matB.m20 += B2 * QX;
            matB.m21 += B2 * QY;
            matB.m22 += B2 * QZ;
            matB.m30 += B3 * QX;
            matB.m31 += B3 * QY;
            matB.m32 += B3 * QZ;
        }

        return matA.inverse * matB;
    }
}
