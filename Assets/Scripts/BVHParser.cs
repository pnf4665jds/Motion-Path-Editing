﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class BVHParser
{
    public int frames = 0;
    public float frameTime = 1000f / 60f;
    public BVHBone root;
    private List<BVHBone> boneList;

    static private char[] charMap = null;
    private float[][] channels;
    private string bvhText;
    private int pos = 0;

    public class BVHBone
    {
        public string name;
        public List<BVHBone> children;
        public float offsetX, offsetY, offsetZ;
        public int[] channelOrder;
        public int channelNumber;
        public BVHChannel[] channels;

        private BVHParser bp;

        // 0 = Xpos, 1 = Ypos, 2 = Zpos, 3 = Xrot, 4 = Yrot, 5 = Zrot
        public struct BVHChannel
        {
            public bool enabled;
            public float[] values;
        }

        public BVHBone(BVHParser parser, bool rootBone)
        {
            bp = parser;
            bp.boneList.Add(this);
            channels = new BVHChannel[6];
            channelOrder = new int[6] { 0, 1, 2, 5, 3, 4 };
            children = new List<BVHBone>();

            bp.skip();
            if (rootBone)
            {
                bp.assureExpect("ROOT");
            }
            else
            {
                bp.assureExpect("JOINT");
            }
            bp.assure("joint name", bp.getString(out name));
            bp.skip();
            bp.assureExpect("{");
            bp.skip();
            bp.assureExpect("OFFSET");
            bp.skip();
            bp.assure("offset X", bp.getFloat(out offsetX));
            bp.skip();
            bp.assure("offset Y", bp.getFloat(out offsetY));
            bp.skip();
            bp.assure("offset Z", bp.getFloat(out offsetZ));
            bp.skip();
            bp.assureExpect("CHANNELS");

            bp.skip();
            bp.assure("channel number", bp.getInt(out channelNumber));
            bp.assure("valid channel number", channelNumber >= 1 && channelNumber <= 6);

            for (int i = 0; i < channelNumber; i++)
            {
                bp.skip();
                int channelId;
                bp.assure("channel ID", bp.getChannel(out channelId));
                channelOrder[i] = channelId;
                channels[channelId].enabled = true;
            }

            char peek = ' ';
            do
            {
                float ignored;
                bp.skip();
                bp.assure("child joint", bp.peek(out peek));
                switch (peek)
                {
                    case 'J':
                        BVHBone child = new BVHBone(bp, false);
                        children.Add(child);
                        break;
                    case 'E':
                        bp.assureExpect("End Site");
                        bp.skip();
                        bp.assureExpect("{");
                        bp.skip();
                        bp.assureExpect("OFFSET");
                        bp.skip();
                        bp.assure("end site offset X", bp.getFloat(out ignored));
                        bp.skip();
                        bp.assure("end site offset Y", bp.getFloat(out ignored));
                        bp.skip();
                        bp.assure("end site offset Z", bp.getFloat(out ignored));
                        bp.skip();
                        bp.assureExpect("}");
                        break;
                    case '}':
                        bp.assureExpect("}");
                        break;
                    default:
                        bp.assure("child joint", false);
                        break;
                }
            } while (peek != '}');
        }
    }

    private bool peek(out char c)
    {
        c = ' ';
        if (pos >= bvhText.Length)
        {
            return false;
        }
        c = bvhText[pos];
        return true;
    }

    private bool expect(string text)
    {
        foreach (char c in text)
        {
            if (pos >= bvhText.Length || (c != bvhText[pos] && bvhText[pos] < 256 && c != charMap[bvhText[pos]]))
            {
                return false;
            }
            pos++;
        }
        return true;
    }

    private bool getString(out string text)
    {
        text = "";
        while (pos < bvhText.Length && bvhText[pos] != '\n' && bvhText[pos] != '\r')
        {
            text += bvhText[pos++];
        }
        text = text.Trim();

        return (text.Length != 0);
    }

    private bool getChannel(out int channel)
    {
        channel = -1;
        if (pos + 1 >= bvhText.Length)
        {
            return false;
        }
        switch (bvhText[pos])
        {
            case 'x':
            case 'X':
                channel = 0;
                break;
            case 'y':
            case 'Y':
                channel = 1;
                break;
            case 'z':
            case 'Z':
                channel = 2;
                break;
            default:
                return false;
        }
        pos++;
        switch (bvhText[pos])
        {
            case 'p':
            case 'P':
                pos++;
                return expect("osition");
            case 'r':
            case 'R':
                pos++;
                channel += 3;
                return expect("otation");
            default:
                return false;
        }
    }

    private bool getInt(out int v)
    {
        bool negate = false;
        bool digitFound = false;
        v = 0;

        // Read sign
        if (pos < bvhText.Length && bvhText[pos] == '-')
        {
            negate = true;
            pos++;
        }
        else if (pos < bvhText.Length && bvhText[pos] == '+')
        {
            pos++;
        }

        // Read digits
        while (pos < bvhText.Length && bvhText[pos] >= '0' && bvhText[pos] <= '9')
        {
            v = v * 10 + (int)(bvhText[pos++] - '0');
            digitFound = true;
        }

        // Finalize
        if (negate)
        {
            v *= -1;
        }
        if (!digitFound)
        {
            v = -1;
        }
        return digitFound;
    }

    // Accuracy looks okay
    private bool getFloat(out float v)
    {
        bool negate = false;
        bool digitFound = false;
        int i = 0;
        v = 0f;
        // Read sign
        if (pos < bvhText.Length && bvhText[pos] == '-')
        {
            negate = true;
            pos++;
        }
        else if (pos < bvhText.Length && bvhText[pos] == '+')
        {
            pos++;
        }
        // Read digits before decimal point
        while (pos < bvhText.Length && bvhText[pos] >= '0' && bvhText[pos] <= '9')
        {
            v = v * 10 + (float)(bvhText[pos++] - '0');
            digitFound = true;
        }

        // Read decimal point
        if (pos < bvhText.Length && (bvhText[pos] == '.' || bvhText[pos] == ','))
        {
            pos++;
            // Read digits after decimal
            float fac = 0.1f;
            while (pos < bvhText.Length && bvhText[pos] >= '0' && bvhText[pos] <= '9' && i < 128)
            {
                v += fac * (float)(bvhText[pos++] - '0');
                fac *= 0.1f;
                digitFound = true;
            }
        }

        // Finalize
        if (negate)
        {
            v *= -1f;
        }

        if (pos < bvhText.Length && bvhText[pos] == 'e')
        {
            string scienceNum = "10";
            while (pos < bvhText.Length && bvhText[pos] != ' ' && bvhText[pos] != '\t' && bvhText[pos] != '\n' && bvhText[pos] != '\r')
            {
                scienceNum = scienceNum + bvhText[pos];
                pos++;
            }
            v = v * (float)Double.Parse(scienceNum);
        }
        if (!digitFound)
        {
            v = float.NaN;
        }
        return digitFound;
    }

    private void skip()
    {
        while (pos < bvhText.Length && (bvhText[pos] == ' ' || bvhText[pos] == '\t' || bvhText[pos] == '\n' || bvhText[pos] == '\r'))
        {
            pos++;
        }
    }

    private void skipInLine()
    {
        while (pos < bvhText.Length && (bvhText[pos] == ' ' || bvhText[pos] == '\t'))
        {
            pos++;
        }
    }

    private void newline()
    {
        bool foundNewline = false;
        skipInLine();
        while (pos < bvhText.Length && (bvhText[pos] == '\n' || bvhText[pos] == '\r'))
        {
            foundNewline = true;
            pos++;
        }
        assure("newline", foundNewline);
    }

    private string assure(string what, bool result)
    {
        if (!result)
        {
            string errorRegion = "";
            for (int i = Math.Max(0, pos - 15); i < Math.Min(bvhText.Length, pos + 15); i++)
            {
                if (i == pos - 1)
                {
                    errorRegion += ">>>";
                }
                errorRegion += bvhText[i];
                if (i == pos + 1)
                {
                    errorRegion += "<<<";
                }
            }
            return "Failed to parse BVH data at position " + pos + ". Expected " + what + " around here: " + errorRegion;
        }
        return "";
    }

    private string assureExpect(string text)
    {
        return assure(text, expect(text));
    }

    private string parse(bool overrideFrameTime, float time)
    {
        string errorMsg = "";

        // Prepare character table
        if (charMap == null)
        {
            charMap = new char[256];
            for (int i = 0; i < 256; i++)
            {
                if (i >= 'a' && i <= 'z')
                {
                    charMap[i] = (char)(i - 'a' + 'A');
                }
                else if (i == '\t' || i == '\n' || i == '\r')
                {
                    charMap[i] = ' ';
                }
                else
                {
                    charMap[i] = (char)i;
                }
            }
        }

        // Parse skeleton
        skip();
        errorMsg = assureExpect("HIERARCHY");
        if (errorMsg.Length > 0)
            return errorMsg;

        boneList = new List<BVHBone>();
        root = new BVHBone(this, true);

        // Parse meta data
        skip();
        errorMsg = assureExpect("MOTION");
        if(errorMsg.Length > 0)
            return errorMsg;
        skip();
        errorMsg = assureExpect("FRAMES:");
        if (errorMsg.Length > 0)
            return errorMsg;
        skip();
        errorMsg = assure("frame number", getInt(out frames));
        if (errorMsg.Length > 0)
            return errorMsg;
        skip();
        errorMsg = assureExpect("FRAME TIME:");
        if (errorMsg.Length > 0)
            return errorMsg;
        skip();
        errorMsg = assure("frame time", getFloat(out frameTime));
        if (errorMsg.Length > 0)
            return errorMsg;

        if (overrideFrameTime)
        {
            frameTime = time;
        }

        // Prepare channels
        int totalChannels = 0;
        foreach (BVHBone bone in boneList)
        {
            totalChannels += bone.channelNumber;
        }
        int channel = 0;
        channels = new float[totalChannels][];
        foreach (BVHBone bone in boneList)
        {
            for (int i = 0; i < bone.channelNumber; i++)
            {
                channels[channel] = new float[frames];
                bone.channels[bone.channelOrder[i]].values = channels[channel++];
            }
        }

        // Parse frames
        for (int i = 0; i < frames; i++)
        {
            newline();
            for (channel = 0; channel < totalChannels; channel++)
            {
                skipInLine();
                errorMsg = assure("channel value", getFloat(out channels[channel][i]));
                if (errorMsg.Length > 0)
                    return errorMsg;
            }
        }

        return "";
    }

    public BVHParser(string bvhText)
    {
        this.bvhText = bvhText;
    }

    public string Parse()
    {
        return parse(false, 0f);
    }

    public BVHParser(string bvhText, float time)
    {
        this.bvhText = bvhText;

        parse(true, time);
    }


    private Quaternion eul2quat(float z, float y, float x)
    {
        z = z * Mathf.Deg2Rad;
        y = y * Mathf.Deg2Rad;
        x = x * Mathf.Deg2Rad;

        // 动捕数据是ZYX，但是unity是ZXY
        float[] c = new float[3];
        float[] s = new float[3];
        c[0] = Mathf.Cos(x / 2.0f); c[1] = Mathf.Cos(y / 2.0f); c[2] = Mathf.Cos(z / 2.0f);
        s[0] = Mathf.Sin(x / 2.0f); s[1] = Mathf.Sin(y / 2.0f); s[2] = Mathf.Sin(z / 2.0f);

        return new Quaternion(
            c[0] * c[1] * s[2] - s[0] * s[1] * c[2],
            c[0] * s[1] * c[2] + s[0] * c[1] * s[2],
            s[0] * c[1] * c[2] - c[0] * s[1] * s[2],
            c[0] * c[1] * c[2] + s[0] * s[1] * s[2]
            );
    }

    /// <summary>
    /// 回傳每個joint的parent bone name
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string> getHierachy()
    {
        Dictionary<string, string> hierachy = new Dictionary<string, string>();
        foreach (BVHBone bb in boneList)
        {
            foreach (BVHBone bbc in bb.children)
            {
                hierachy.Add(bbc.name, bb.name);
            }
        }
        return hierachy;
    }

    /// <summary>
    /// 回傳每個bone的旋轉值，root的pos存放在"pos"這個key內，以Quaternion形式儲存
    /// </summary>
    /// <param name="frameIdx"></param>
    /// <returns></returns>
    public Dictionary<string, Quaternion> getKeyFrame(int frameIdx)
    {
        Dictionary<string, string> hierachy = getHierachy();
        Dictionary<string, Quaternion> boneData = new Dictionary<string, Quaternion>();
        boneData.Add("pos", new Quaternion(
            boneList[0].channels[0].values[frameIdx],
            boneList[0].channels[1].values[frameIdx],
            boneList[0].channels[2].values[frameIdx], 0));

        boneData.Add(boneList[0].name, eul2quat(
                boneList[0].channels[3].values[frameIdx],
                boneList[0].channels[4].values[frameIdx],
                boneList[0].channels[5].values[frameIdx]));
        foreach (BVHBone bb in boneList)
        {
            if (bb.name != boneList[0].name)
            {
                Quaternion localrot = eul2quat(bb.channels[3].values[frameIdx],
                    bb.channels[4].values[frameIdx],
                    bb.channels[5].values[frameIdx]);
                boneData.Add(bb.name, boneData[hierachy[bb.name]] * localrot);
            }
        }
        return boneData;
    }

    /// <summary>
    /// 回傳每個bone的旋轉值，root的pos存放在"pos"這個key內，以Vector3形式儲存
    /// </summary>
    /// <param name="frameIdx"></param>
    /// <returns></returns>
    public Dictionary<string, Vector3> getKeyFrameAsVector(int frameIdx)
    {
        Dictionary<string, string> hierachy = getHierachy();
        Dictionary<string, Vector3> boneData = new Dictionary<string, Vector3>();
        boneData.Add("pos", new Vector3(
            boneList[0].channels[0].values[frameIdx],
            boneList[0].channels[1].values[frameIdx],
            boneList[0].channels[2].values[frameIdx]));

        boneData.Add(boneList[0].name, new Vector3(
                boneList[0].channels[3].values[frameIdx],
                boneList[0].channels[4].values[frameIdx],
                boneList[0].channels[5].values[frameIdx]));
        foreach (BVHBone bb in boneList)
        {
            if (bb.name != boneList[0].name)
            {
                Vector3 localrot = new Vector3(bb.channels[3].values[frameIdx],
                    bb.channels[4].values[frameIdx],
                    bb.channels[5].values[frameIdx]);
                boneData.Add(bb.name, localrot);
            }
        }
        return boneData;
    }

    public Dictionary<string, Vector3> getOffset(float ratio)
    {
        Dictionary<string, Vector3> offset = new Dictionary<string, Vector3>();
        foreach (BVHBone bb in boneList)
        {
            offset.Add(bb.name, new Vector3(bb.offsetX * ratio, bb.offsetY * ratio, bb.offsetZ * ratio));
        }
        return offset;
    }

    /// <summary>
    /// 取得紀錄所有Bone的List
    /// </summary>
    /// <returns></returns>
    public List<BVHBone> getBoneList()
    {
        return boneList;
    }
}

