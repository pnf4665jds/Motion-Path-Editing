using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunTimeBezier : MonoBehaviour
{
    public List<GameObject> concretePoints = new List<GameObject>();
    public LineRenderer Lr;
    //private GameObject[] concreteObject = new GameObject[4]; 
    public void Init()
    {
        for (int i = 0; i < 4; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(1f, 1f, 1f);
            cube.transform.position = new Vector3(i*10, 0, 0);

            cube.AddComponent<DragPoint>();
            cube.transform.SetParent(this.transform);
            //concreteObject[i] = cube ;
            concretePoints.Add(cube);
        }

        /***********************************/
        Lr.startWidth = 0.2f;
        Lr.endWidth = 0.2f;
        Lr.startColor = Color.yellow;
        Lr.endColor = Color.yellow;
        Lr.positionCount = 101;

        
    }

    public void LogicalUpdate() 
    {
        if (concretePoints.Count <= 0) { return; }

        Vector3 p0 = concretePoints[0].transform.position;
        Vector3 p1 = concretePoints[1].transform.position;
        Vector3 p2 = concretePoints[2].transform.position;
        Vector3 p3 = concretePoints[3].transform.position;

        Vector3 p = Bezier.GetPoint(p0, p1, p2, p3, 0);
        Lr.SetPosition(0, p);
        for (int t = 1; t <= 100; t++)
        {
            float f = t / 100.0f;
            Lr.SetPosition(t,Bezier.GetPoint(p0, p1, p2, p3, f));
            //p = Bezier.GetPoint(p0, p1, p2, p3, f);
        }
    }

    /// <summary>
    /// 取得Path上的平移矩陣
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Matrix4x4 GetTranslationMatrix(float t)
    {
        Vector3 p0 = concretePoints[0].transform.position;
        Vector3 p1 = concretePoints[1].transform.position;
        Vector3 p2 = concretePoints[2].transform.position;
        Vector3 p3 = concretePoints[3].transform.position;

        Vector3 pos = Bezier.GetPoint(p0, p1, p2, p3, t);
        return Matrix4x4.Translate(pos);
    }

    /// <summary>
    /// 取得Path上的旋轉矩陣
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Matrix4x4 GetRotationMatrix(float t)
    {
        Vector3 p0 = concretePoints[0].transform.position;
        Vector3 p1 = concretePoints[1].transform.position;
        Vector3 p2 = concretePoints[2].transform.position;
        Vector3 p3 = concretePoints[3].transform.position;

        Vector3 tangent = Vector3.ProjectOnPlane(Bezier.GetFirstDerivative(p0, p1, p2, p3, t), Vector3.up);
        Vector3 right = -Vector3.Cross(Vector3.up, tangent);
        Vector3 up = Vector3.up;
        Matrix4x4 rotationMatrix = new Matrix4x4();
        rotationMatrix.SetRow(0, new Vector4(right.x, right.y, right.z, 0));
        rotationMatrix.SetRow(1, new Vector4(up.x, up.y, up.z, 0));
        rotationMatrix.SetRow(2, new Vector4(tangent.x, tangent.y, tangent.z, 0));
        rotationMatrix.SetRow(3, new Vector4(0, 0, 0, 1));

        return rotationMatrix;
    }

    private void drawDirection(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) 
    {
        Bezier.GetFirstDerivative(p0, p1, p2, p3, t);
    }
    void OnDrawGizmos()
    {
        if (concretePoints.Count <= 0) { return; }
        for (int i = 0; i < 4; i+=4)
        {
            Vector3 p0 = concretePoints[i].transform.position;
            Vector3 p1 = concretePoints[i+1].transform.position;
            Vector3 p2 = concretePoints[i+2].transform.position;
            Vector3 p3 = concretePoints[i + 3].transform.position;

            Gizmos.color = Color.gray;
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);


            //Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);

            Gizmos.color = Color.green;
            Vector3 p = Bezier.GetPoint(p0, p1, p2, p3, 0);
            Gizmos.DrawLine(p, p + Bezier.GetFirstDerivative(p0, p1, p2, p3, 0) * 0.2f);

            for (int t = 1; t <= 10; t ++)
            {
                
                float f = t / 10.0f;
                Vector3 pt = Bezier.GetPoint(p0, p1, p2, p3, f);

                Gizmos.DrawLine(pt, pt + Bezier.GetFirstDerivative(p0, p1, p2, p3, f) * 0.2f);
            }

        }
    }
}
