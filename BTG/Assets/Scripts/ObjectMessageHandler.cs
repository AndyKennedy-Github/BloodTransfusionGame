using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMessageHandler : MonoBehaviour
{
    public bool jump = false;
    public bool toScale = false;
    public bool toMove = false;
    public bool follow = false;
    public float movementSpeed = 1f;
    private Vector3 movement;
    public Vector3 scale = new Vector3(5, 5, 5);
    public Vector3 pos = new Vector3(5, 5, 5);
    public Vector3 offset = new Vector3(0.0f,0.2f,-0.10f);
    string radialMenuResult;

    private Rigidbody rb; //This object's ridid body

    // Start is called before the first frame update
    void Start()
    {

    }


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (centerButton==null){
            centerButton = Resources.Load<Texture2D>("radialSelect");              
        }

        //MenuStart();
        choices = new string[2];
        
        question = "Your Answer?";
        //print(this.name + ": Start: Setting Question: "+ question);

        choices[0] = "YES";
        choices[1] = "NO";
        radialMenuActive = false;
        GameObject go=this.gameObject;
        Vector3 mpos= go.transform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(mpos);

        //print("The screen position is " + screenPos);
        center.x = screenPos.x;
        center.y = Screen.height - screenPos.y;//GUI starts in upper-left, not bottom-left
        MenuSetup();
    }

    public virtual bool HandleMessage(string msg, string param = null)
    {
        print(this.name + ": Handle Message " + msg + " for " + this.name + " with param = "+ param);
        if (msg == "follow")
        {
            if (param != "false"){
                follow = true;
                print("I will follow");
            }else{
                follow = false;
            }
        }

        // JUMP
        if (msg == "jump")
        {
            jump = true;
            print("imma jump");
        }

        /////////////////////////////////////////////////////////////////////

        // ON
        if (msg == "on")
        {
            this.transform.GetChild(0).gameObject.SetActive(true);
        }
        // OFF
        if (msg == "off")
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
        }

        // ROTATEY
        if (msg == "rotatey")
        {
            float duration = float.Parse(param);
            //float duration = 2f;
            print("Start Rotating Door" + this.name);
            StartCoroutine(RotateMe(duration));
        }

        // SCALE
        if (msg == "scale")
        {
            print("hello, I am scaling");
            toScale = true;
            scale = getVector3(param);
            print("The scale is " + scale);

            //do something...
        }

        // MOVETO
        if (msg == "moveto" || msg== "align")
        {
            print("hello, I am moving");
            toMove = true;
            if ((param[0] == '-'  || System.Char.IsDigit (param[0]))){ //moveTo position
                pos = getVector3(param);
                transform.position = pos;
            }else{
                GameObject go=GameObject.Find(param); //moveTo object's position
                print("moving to position of game object "+ go.name);
                pos= go.transform.position;
                if (msg=="align")
                    transform.rotation = go.transform.rotation;
                transform.position = pos;
            }

            print("The position is " + pos);

            //do something...
        }

        // RADIALMENU
        // Form radialMenu Form/SignatureLine
        // Form radialMenu Form/SignatureLine
        if (msg == "menu.on")
        {
            print("Setup and Turn radialMenu on for "+ this.name);
            radialMenuResult="";
            //toMove = true;

            Vector3 mpos;
            GameObject go=this.gameObject;
            mpos= go.transform.position;
            if (param!=null){
                if  (System.Char.IsDigit (param[0])){ //moveTo position
                    mpos = getVector3(param);
                }else{
                    print("getting game object for param " + param);
                    go=GameObject.Find(param); //moveTo object's position
                    print("getting position of game object "+ go.name);
                    mpos= go.transform.position;
                }
            }
            Vector3 screenPos = Camera.main.WorldToScreenPoint(mpos);

            print("The screen position is " + screenPos);
            center.x = screenPos.x;
            center.y = Screen.height - screenPos.y;//GUI starts in upper-left, not bottom-left
            MenuSetup(); 
            radialMenuActive=true;

            //do something...
        }
        if (msg == "menu.question")
        {
            print(this.name + ": mhandler: Setting Question to "+ param);
            question = param;
            print(this.name + ": mhandler: Question: "+ question);
        }
        if (msg == "menu.choices")
        {
            char[] separators = new char[] { ' '};
            string [] tmp = param.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            print("tmp legnth = "+ tmp.Length + ",sep=" + tmp);
            choices = tmp;
            print("choices legnth = "+ choices.Length + ",sep=" + choices);
        }
        if (msg == "menu.done")
        {
            return !radialMenuActive;
        }
        if (msg == "menu.result")
        {
            print("radialMenuResult: param ="+ param);
            print("radialResult= "+ radialMenuResult);
            print("conditional = " + (param == radialMenuResult));
            return (param == radialMenuResult);
            
        }


        //LOOKATME
        // [Object] lookAtMe [Offset]
        // e.g.: /ExamRoom1/Desk lookAtMe  0.0,1.0,1.0   
        if (msg == "lookatme")
        {
            print("lookAtMe");
            //toMove = true;
            Vector3 mpos;
            //BUG Needs to be fixed: DOESNT HANDLE NEGATIVE SIGN
            if (param!=null && (param[0] == '-' || System.Char.IsDigit (param[0]))){ //moveTo position
                offset = getVector3(param);
            }
            {
                GameObject go=this.gameObject;//GameObject.Find(param); //moveTo object's position
                print("getting position of game object "+ go.name);
                mpos= go.transform.position;
            }
            //transform.position = pos;
            Camera.main.transform.position = mpos + offset;
            Camera.main.transform.LookAt(mpos);
        } //lookAtMe

        // LOOKAAT
        // lookAt targetObject offset
        // lookAt targetObject viewerObject(for position)
        if (msg == "lookat")
        {
            print("lookAt");
            //toMove = true;
            Vector3 vpos,tpos;
            /*  Need to support both object and offset*/
            char[] separators = new char[] { ' '};

            string[] temp = param.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            param = temp[0];
            GameObject go=GameObject.Find(param); //moveTo object's position
            print("getting position of target game object "+ go.name);
            tpos= go.transform.position;
            if (temp.Length>1)
            {
                string offsetStr = temp[1];
                if ((offsetStr[0] == '-' ) || System.Char.IsDigit (offsetStr[0])){ //moveTo position
                    pos = getVector3(offsetStr);
                    offset = getVector3(temp[1]);
                    vpos= tpos + offset;
                }else{
                    GameObject vgo=GameObject.Find(offsetStr); //moveTo object's position
                    vpos= vgo.transform.position;
                }
                Camera.main.transform.position = vpos;
            }

            //transform.position = pos;
            Camera.main.transform.LookAt(tpos);
        } //lookat


        if (msg == "ison")
        {
            return this.transform.GetChild(0).gameObject.activeSelf;
        } //isOn

        return true;
    }

    ////////////////////////////////////////////////////////////////////////////
    //Helper functions

    //Get Vector3 in form of either  1.0 2.0 3.0   or 1.0,2.0,3.0
    public Vector3 getVector3(string rString)
    {
        print("getVector3:"+ rString);
        char[] separators = new char[] { ' ', ',' };

        string[] temp = rString.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        //string[] temp = rString.Split(' ');
//        print("getVector3x:"+ temp[0]);
//        print("getVector3y:"+ temp[1]);
//        print("getVector3z:"+ temp[2]);
        float x = float.Parse(temp[0]);
        float y = float.Parse(temp[1]);
        float z = float.Parse(temp[2]);
//        print("getVector3: ("+x+","+y+","+z+")");
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

    private void Move()
    {
        if (toMove)
        {
            transform.position = pos;
        }
        print("at MOVE the pos is = to" + pos);
        toMove=false;
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
        toScale=false;
    }

    private void followPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        Vector3 direction = player.transform.position - transform.position;
        transform.LookAt(player.transform);
        //rb.MovePosition((Vector3)transform.position + transform.forward *  Time.fixedDeltaTime);//(direction * movementSpeed * Time.fixedDeltaTime));
        this.transform.position = ((Vector3)transform.position + transform.forward *  Time.fixedDeltaTime);//(direction * movementSpeed * Time.fixedDeltaTime));
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
        if (toMove)
        {
            Move();
            print("I have moved by" + pos);
        }
        if (follow)
        {
            follow = true;
            followPlayer();
        }
    }

    ///////////////////////////////////////////////////////////////////////

    // GUI Radial Menu Stuff

    public  Vector2 center = new Vector2(500,500); // position of center button
    public int radius = 125;  // pixels radius to center of button;
    public Texture centerButton;  
    // public Texture [] normalButtons;// : Texture[];
    // public Texture [] selectedButtons;// : Texture[];
    public string question;
    public string [] choices;
    

    #pragma strict
    
    private int ringCount;// : int; 
    private Rect centerRect;// : Rect;
    private Rect[] ringRects;// : Rect[];
    private float angle;// : float;
    private bool showButtons = false;
    private int index;// : int;
    private int menusize = 100 ;
    public float menuSizeFraction=0.5f;
    void MenuSetup () {
        menusize = (int) ((float) Screen.height*menuSizeFraction/3.0f);
        radius = menusize * 125/100;
        if (choices == null)
            return;
        ringCount = choices.Length;
        angle = 360.0f / ringCount;
        int centerButton_width = menusize * 3;
        int centerButton_height = menusize * 3;
        centerRect.x = center.x - centerButton_width  * 0.5f;
        centerRect.y = center.y - centerButton_height * 0.5f;
        centerRect.width = centerButton_width;
        centerRect.height = centerButton_height;
        
        ringRects = new Rect[ringCount];
        
        var w = menusize;//normalButtons[0].width;
        var h = menusize;//normalButtons[0].height;
        var rect = new Rect(0,0,w, h);
        
        var v = new Vector2(radius,0);
        
        for (var i = 0; i < ringCount; i++) {
            rect.x = center.x + v.x - w * 0.5f;
            rect.y = center.y + v.y - h * 0.5f;
            ringRects[i] = rect;
            v = Quaternion.AngleAxis(angle, Vector3.forward) * v;
        }
    }
    public bool radialMenuActive = false;



    void OnGUI() {
        if (!radialMenuActive || ringCount==0)
            return;
        var e = Event.current;
        
        if (e.type == EventType.MouseDown && centerRect.Contains(e.mousePosition)) {
            showButtons = true;
            index = -1;
        }    
                
        if (e.type == EventType.MouseUp) {
            if (showButtons) {
                Debug.Log("User selected #"+index);// + ", " + choices[index]);
                if (index>=0){
                    Debug.Log("User selected #"+choices[index]);
                    radialMenuResult = choices[index];
                    radialMenuActive=false;
                }
            }
            showButtons = false;
        }
            
        if (e.type == EventType.MouseDrag) {
            var v = e.mousePosition - center;
            var a = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            a += angle / 2.0f;
            if (a < 0) a = a + 360.0f;
    
            index = (int) (a / angle);
        }

        //    GUIContent content;
        //       content = new GUIContent(question, centerButton, "This is a tooltip");
        GUIStyle style = new GUIStyle();

        style.fontSize = menusize/4; //change the font size        
        style.alignment = TextAnchor.MiddleCenter;
        GUI.DrawTexture(centerRect, centerButton);
        GUI.Label(centerRect, question,style);
        //print(this.name + "'s question is "+ question);

        if (showButtons) {
            for (var i = 0; i < choices.Length; i++) {
                if (i != index){ 
                    GUI.DrawTexture(ringRects[i], centerButton);
                    GUI.Label(ringRects[i], choices[i],style);
                    //GUI.DrawTexture(ringRects[i], normalButtons[i]);
                }else{
                    GUI.DrawTexture(ringRects[i], centerButton);
                    GUI.Box(ringRects[i], choices[i],style);
                }
            }
        }
    }
}