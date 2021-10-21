using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunTimeBezier : MonoBehaviour
{
    private List<GameObject> concretePoints = new List<GameObject>();
    //private GameObject[] concreteObject = new GameObject[4]; 
    public void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            cube.transform.position = new Vector3(i*10, 0, 0);

            cube.AddComponent<DragPoint>();
            //concreteObject[i] = cube ;
            concretePoints.Add(cube);
        }
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

            Gizmos.color = Color.yellow;
            Vector3 p = Bezier.GetPoint(p0, p1, p2, p3, 0);
            for (int t = 1; t <= 100; t ++)
            {
                float f = t / 100.0f;
                Gizmos.DrawLine(p, Bezier.GetPoint(p0, p1, p2, p3, f));
                p = Bezier.GetPoint(p0, p1, p2, p3, f);
            }

        }
    }
}
