using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunTimeBezier : MonoBehaviour
{
    public List<GameObject> concretePoints = new List<GameObject>();
    public LineRenderer Lr;
    public float Speed = 1;
    public Material CubeMaterial;
    //private GameObject[] concreteObject = new GameObject[4]; 

    private Vector3 p0, p1, p2, p3;
    public void Init(bool isShowPoint)
    {
        for (int i = 0; i < 4; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(10f, 10f, 10f);
            cube.transform.position = new Vector3(i*10, 0, 0);
            if (CubeMaterial) 
            {
                cube.GetComponent<MeshRenderer>().material = CubeMaterial;
            }
            

            if (!isShowPoint)
            {
                
                cube.GetComponent<MeshRenderer>().enabled = false;
            }
            else 
            {
                cube.AddComponent<DragPoint>();
            }
            
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

    public void LogicalUpdate(bool isShowPoint) 
    {
        if (!isShowPoint)
        {
            foreach (GameObject gm in concretePoints)
            {
                gm.GetComponent<MeshRenderer>().enabled = false;
                if (gm.GetComponent<DragPoint>())
                    gm.GetComponent<DragPoint>().CanDrag = false;
            }

        }
        else 
        {
            foreach (GameObject gm in concretePoints)
            {
                gm.GetComponent<MeshRenderer>().enabled = true;
                if (gm.GetComponent<DragPoint>())
                    gm.GetComponent<DragPoint>().CanDrag = true;
            }
        }
        if (concretePoints.Count <= 0) { return; }

        p0 = concretePoints[0].transform.position;
        p1 = concretePoints[1].transform.position;
        p2 = concretePoints[2].transform.position;
        p3 = concretePoints[3].transform.position;

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
    /// for the arc length
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector3 GetPoint(float t) 
    {
        return Bezier.GetPoint(p0, p1, p2, p3, t);
    }
    public Vector3 GetPointDerivation(float t)
    {
        return Bezier.GetFirstDerivative(p0, p1, p2, p3, t);
    }

    /// <summary>
    /// get the arc length point
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public float ArcLengthProgress(float progress, float length, int stepNum, float speed) 
    {
        float tangetLength = Mathf.Sqrt(Mathf.Pow(GetPointDerivation(progress).x, 2) + Mathf.Pow(GetPointDerivation(progress).y, 2)
                + Mathf.Pow(GetPointDerivation(progress).z, 2));
        progress += length / stepNum / tangetLength * speed;

        return progress;
    }

    /// <summary>
    /// 獲取當前這條spline line 的長度
    /// </summary>
    /// <param name="slice"></param>
    /// <returns></returns>
    public float GetBezierLength(float slice) 
    {
        Vector3 prevPos = Vector3.zero;
        float totalDistance = 0;
        float stepSize = (float)1 / slice;

        //get the arc length
        for (int f = 0; f < slice; f++)
        {
            if (f == 0)
            {
                prevPos = GetPoint(f * stepSize);
            }
            else
            {
                totalDistance += Vector3.Distance(GetPoint(f * stepSize), prevPos);

                prevPos = GetPoint(f * stepSize);
            }

        }

        return totalDistance;
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

        Vector3 tangent = Vector3.ProjectOnPlane(Bezier.GetFirstDerivative(p0, p1, p2, p3, t).normalized, Vector3.up);
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

    public void ClearBezier() 
    {
        foreach (GameObject gm in concretePoints) 
        {
            Destroy(gm);
        }
        concretePoints.Clear();
        Lr.positionCount = 0;
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
