using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{

    public float speed = 1.0f;
    Transform origPos;
    Transform target;
    bool isMoving;
    // Start is called before the first frame update
    void Start()
    {
        isMoving = false;
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;
        if(isMoving)
        {
            Camera.main.transform.position = Vector3.MoveTowards(origPos.position, target.position, step);
            if(Vector3.Distance(Camera.main.transform.position, target.position) < .01f)
            {
                isMoving = false;
            }
        }
    }

    public void SetTargetandMove(Transform t, bool b)
    {
        origPos = Camera.main.transform;
        target = t;
        isMoving = b;
    }
}
