using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParserTool
{
    /// <summary>
    /// 依照ZXY的順序將Quaternion乘起來
    /// </summary>
    /// <param name="euler"></param>
    /// <returns></returns>
    public static Quaternion Euler2Quat(Vector3 euler, int[] order)
    {
        if (order[0] == 5 && order[1] == 4 && order[2] == 3)
            return Quaternion.Euler(0, 0, euler.z) * Quaternion.Euler(0, euler.y, 0) * Quaternion.Euler(euler.x, 0, 0);
        else if (order[0] == 5 && order[1] == 3 && order[2] == 4)
            return Quaternion.Euler(0, 0, euler.z) * Quaternion.Euler(euler.x, 0, 0) * Quaternion.Euler(0, euler.y, 0);
        else if (order[0] == 4 && order[1] == 3 && order[2] == 5)
            return Quaternion.Euler(0, euler.y, 0) * Quaternion.Euler(0, 0, euler.z) * Quaternion.Euler(euler.x, 0, 0);
        else if (order[0] == 4 && order[1] == 5 && order[2] == 3)
            return Quaternion.Euler(0, euler.y, 0) * Quaternion.Euler(0, 0, euler.z) * Quaternion.Euler(euler.x, 0, 0);
        else if (order[0] == 3 && order[1] == 4 && order[2] == 5)
            return Quaternion.Euler(euler.x, 0, 0) * Quaternion.Euler(0, euler.y, 0) * Quaternion.Euler(0, 0, euler.z);
        else
            return Quaternion.Euler(euler.x, 0, 0) * Quaternion.Euler(0, 0, euler.z) * Quaternion.Euler(0, euler.y, 0);
    }
}
