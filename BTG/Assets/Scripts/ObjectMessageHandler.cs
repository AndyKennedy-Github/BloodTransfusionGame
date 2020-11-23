using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMessageHandler : MonoBehaviour
{
    public bool jump = false;
    public bool toScale = false;
    public bool follow = false;
    public float movementSpeed = 1f;
    private Vector3 movement;
    public Vector3 scale = new Vector3(5, 5, 5);

    // Start is called before the first frame update
    private void Start()
    {
    }

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public virtual bool HandleMessage(string msg, string param = null)
    {
        print("Message " + msg + " for " + this.name);
        if (msg == "follow")
        {
            if (param != "false"){
                follow = true;
                print("I will follow");
            }else{
                follow = false;
            }
        }
        if (msg == "jump")
        {
            jump = true;
            print("imma jump");
        }
        if (msg == "on")
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
        }
        if (msg == "off")
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
        }
        if (msg == "rotateY")
        {
            float duration = float.Parse(param);
            //float duration = 2f;
            print("Start Rotating Door" + this.name);
            StartCoroutine(RotateMe(duration));
        }
        if (msg == "scale")
        {
            print("hello, I am scaling");
            toScale = true;
            scale = getVector3(param);
            print("The scale is " + scale);

            //do something...
        }
        if (msg == "isOn")
        {
            return this.transform.GetChild(0).gameObject.activeSelf;
        }
        return true;
    }

    //Helper functions
    public Vector3 getVector3(string rString)
    {
        string[] temp = rString.Split(',');
        float x = float.Parse(temp[0]);
        float y = float.Parse(temp[1]);
        float z = float.Parse(temp[2]);
        Vector3 rValue = new Vector3(x, y, z);
        return rValue;
    }

    private IEnumerator RotateMe(float duration)
    {
        Quaternion startRot = transform.rotation;
        float t = 0.0f;
        while (t < duration)
        {
            //print("Rotating "+ transform.rotation);
            t += Time.deltaTime;
            transform.rotation = startRot * Quaternion.AngleAxis(t / duration * 360f, transform.up); //or transform.right if you want it to be locally based
            yield return null;
        }
        transform.rotation = startRot;
    }

    //Jump behavior
    public float jumpVelocity = 8.5f;

    public float fallMult = 2.5f;
    public float lowJumpMult = 2f;
    public Vector3 gravity = new Vector3(0f, -10f, 0f);

    private void Jump()
    {
        if (rb)
        {
            if (jump)
            {
                rb.velocity = Vector3.up * jumpVelocity;
            }
            else
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector3.up + gravity * (fallMult - 1) * Time.deltaTime;
            }
            else if (rb.velocity.y > 0)
            {
                rb.velocity += Vector3.up + gravity * (lowJumpMult - 1) * Time.deltaTime;
            }
            jump = false;
        }
    }

    private void Scale()
    {
        if (toScale)
        {
            transform.localScale = scale;
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        print("at SCALE the scale is = to" + scale);
    }

    private void followPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        Vector3 direction = player.transform.position - transform.position;
        transform.LookAt(player.transform);
        rb.MovePosition((Vector3)transform.position + transform.forward *  Time.fixedDeltaTime);//(direction * movementSpeed * Time.fixedDeltaTime));
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (jump)
        {
            Jump();
            print("I jumped");
        }
        if (toScale)
        {
            Scale();
            print("I have scaled by" + scale);
        }
        if (follow)
        {
            follow = true;
            followPlayer();
        }
    }
}