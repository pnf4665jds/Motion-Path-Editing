using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeCurveDriver : MonoBehaviour
{

    public RunTimeBezier Rt;
    // Start is called before the first frame update
    void Start()
    {
        Rt.Init(true);
    }

    // Update is called once per frame
    void Update()
    {
        Rt.LogicalUpdate();
    }
}
