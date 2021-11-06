using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragPoint : MonoBehaviour
{

    private Vector3 mOffset;
    private float mZCoord;
    private float yAxisValue;

    public bool CanDrag = true;

    /*void Start() 
    {
        yAxisValue = this.transform.position.y;
    }*/
    /*private void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
    }*/

    private Vector3 GetMouseAsWorldPoint() 
    {
        //get mouse x,y
        Vector3 mousePoint = Input.mousePosition;

        //z coordinate of game object on screen
        mousePoint.z = mZCoord;
        mousePoint.y = yAxisValue;

        //Convert it to world points

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    /*private void OnMouseDrag()
     {
         transform.position = GetMouseAsWorldPoint() + mOffset;

     }*/
    Vector3 dist;
    Vector3 startPos;
    float posX;
    float posZ;
    float posY;
    void OnMouseDown()
    {
        
        startPos = transform.position;
        dist = Camera.main.WorldToScreenPoint(transform.position);
        posX = Input.mousePosition.x - dist.x;
        posY = Input.mousePosition.y - dist.y;
        posZ = Input.mousePosition.z - dist.z;
    }

    void OnMouseDrag()
    {
        if (CanDrag) 
        {
            float disX = Input.mousePosition.x - posX;
            float disY = Input.mousePosition.y - posY;
            float disZ = Input.mousePosition.z - posZ;
            Vector3 lastPos = Camera.main.ScreenToWorldPoint(new Vector3(disX, disY, disZ));
            transform.position = new Vector3(lastPos.x, startPos.y, lastPos.z);
        }
        
    }

}
