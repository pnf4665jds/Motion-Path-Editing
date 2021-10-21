using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier 
{
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t) 
    {
        //return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;

        //(1-t)^2 * p0 + 2*(1-t)t * p1 + t^2 * p2
        return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
    }
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        //return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;

        Matrix4x4 matrix = new Matrix4x4();

        matrix.SetColumn(0, new Vector4(-1, 3, -3, 1));
        matrix.SetColumn(1, new Vector4(3, -6, 3, 0));
        matrix.SetColumn(2, new Vector4(-3, 0, 3, 0));
        matrix.SetColumn(3, new Vector4(1, 4, 1, 0));

        Matrix4x4 pointMatrix = new Matrix4x4();

        pointMatrix.SetColumn(0, new Vector4(p0.x, p0.y, p0.z, 0));
        pointMatrix.SetColumn(1, new Vector4(p1.x, p1.y, p1.z, 0));
        pointMatrix.SetColumn(2, new Vector4(p2.x, p2.y, p2.z, 0));
        pointMatrix.SetColumn(3, new Vector4(p3.x, p3.y, p3.z, 0));

        pointMatrix = pointMatrix * matrix;
        Vector4 v4 = pointMatrix * new Vector4(t * t * t, t * t, t, 1);

        return new Vector3(v4.x/6,v4.y/6,v4.z/6);


        /*return 
            oneMinusT * oneMinusT * oneMinusT * p0 + 
            3f * oneMinusT * oneMinusT * t * p1 +
            3f * oneMinusT * t * t * p2 +
            t * t * t * p3;*/
    }
    public static float Get_B_Zero(float t) 
    {
        return Mathf.Pow((1 - t), 3) / 6;
    }
    public static float Get_B_One(float t)
    {
        return (3 * Mathf.Pow((t), 3)  - 6 * Mathf.Pow((t), 2) + 4 )/ 6;
    }
    public static float Get_B_Two(float t)
    {
        return (-3 * Mathf.Pow((t), 3) + 3 * Mathf.Pow((t), 2) + 3*t + 1) / 6;
    }
    public static float Get_B_Three(float t)
    {
        return (Mathf.Pow((t), 3)) / 6;
    }

    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t) 
    {
        return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
    }
    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }

}
