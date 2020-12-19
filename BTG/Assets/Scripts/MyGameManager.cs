using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;  //Use this if WWW is obsolete in Unity version


public class CommandSequence{
    public int commandNum=0;
        // Update is called once per frame
    public enum IFState {False, Condition, Then, Else};
    public enum WAITState {False,Condition};
    public IFState IFstate = IFState.False;
    public WAITState WAITstate = WAITState.False;
    public bool IFresult = true;
    public string commandLine;
    public int nestingLevel;

}


public class MyGameManager : MonoBehaviour
{
    public string [] CommandFiles; //contains list of Scenario files
    private int currentNestingLevel=0;

    //array of hardcoded commands for testing
    string [] mycommands = {
        "/Room1/Enemy jump",
        "GM Sleep 1",
        "/Room1/Door rotateY 1.2",
        "GM Sleep 1",
        "/RoomLight off",
        "GM Sleep 1",
        "/RoomLight on",
        "Room1/Room scale 5.0,7.5"

    };

    // Start is called before the first frame update
    // This starts the 
    void Start()
    {
       
        StartCoroutine(exectueScenarioFiles(CommandFiles));
        //StartCoroutine(ExecuteCommands(commands));

    }

    //loads local or remote file
    IEnumerator exectueScenarioFiles(string [] fileNames)
    {
        foreach (string fileName in fileNames){
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

            string result;

            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                //WWW www = new WWW(filePath);
                //yield return www;
                //result = www.text;
                UnityWebRequest webRequest = UnityWebRequest.Get(filePath); //use this if WWW is obsolete
                 yield return webRequest.SendWebRequest();
                 if (webRequest.isNetworkError){
                     print("Web request error:"+ webRequest.error + " for " + filePath);
                     continue;
                 }else{
                     result = webRequest.downloadHandler.text;
                 }

            }
            else
            {
                result = System.IO.File.ReadAllText(filePath);
            }

            Debug.Log("Loaded file: " + fileNames);
                   
            string[] linesInFile = result.Split('\n');
            yield return StartCoroutine(ExecuteCommands(linesInFile));
        }
    }

//    private GameObject go;


    
    string GUIcommandLine,GUIlastcommandLine;

    //Executes array of commands
    IEnumerator ExecuteCommands(string [] commands)
    {
        CommandSequence cs = new CommandSequence();
        cs.nestingLevel = currentNestingLevel;
        for (int i = 0; i < commands.Length; i++)
        {
            string objName,command,cparams;
            command="#";
            cparams=null;
            cs.commandNum=i;
            GUIlastcommandLine = cs.commandLine;
            cs.commandLine = commands[cs.commandNum];
            cs.commandLine = cs.commandLine.Trim(); //remove extra newlines or whitespace at beginning and end
            print("GM:Execute Command["+ cs.commandNum+ "] = " + cs.commandLine);
            GUIcommandLine = cs.commandLine;

            //split up cs.commandLine into objName, command, and cparams
            char [] separators = new char[] { ' ','\t' };
            string [] splitArray = cs.commandLine.Split(separators,StringSplitOptions.RemoveEmptyEntries);
            objName = splitArray[0];  //
            if (splitArray.Length>1)
                command = splitArray[1];  
            print("command="+command);

            if (splitArray.Length>2){ //there are parameters too
                //find where parameters start (after command)
                
                int start = 0;
                int end = cs.commandLine.Length;
                int count = end - start;
                int at = cs.commandLine.IndexOf(command, start, count);
                if (at == -1) continue;
                start = at+command.Length+1;

                //Trim off end comments like this  #this is a comment
                //print("cs.commandLine cparams starts at "+ start);
                string ptmp = cs.commandLine.Substring(start);
                //print("ptmp="+ptmp);
                string [] tmp = ptmp.Split('#');
                cparams = tmp[0].Trim();

            }
            //have to change to lower after params because command is searched to calc params
            command = command.ToLower();//change command to lower case


            if (objName.Length<1 || objName[0]=='#')  //skip comments
                continue;

            //execute GameManager commands
            if (objName=="GM"){  //special Game Manager commands

                if (IFstateSwitch(command,cs)){ //handle IF-THEN-ELSE-ENDIF commands
                    print("cs.IFstate after stateswitch = "+ cs.IFstate);
                    continue;
                }
                //check if in conditional blocks (THEN or ELSE)
                if (cs.IFstate== CommandSequence.IFState.Then && !cs.IFresult){  //skip then if false
                    print("GM:Skipping THEN block Command["+ cs.commandNum+ "] = " + cs.commandLine);
                    continue;
                }
                if (cs.IFstate== CommandSequence.IFState.Else && cs.IFresult){  //skip else if true
                    print("GM:Skipping ELSE block Command["+ cs.commandNum+ "] = " + cs.commandLine);
                    continue;
                }

                if (command == "waitfor"){ //handle WAITFOR commands
                    cs.WAITstate = CommandSequence.WAITState.Condition;
                    continue;
                }

                //Handle Game Manager commands
                //Add additional commands below.
                if (command == "create")
                {

                    print("Create "+ splitArray[2]);
                    GameObject obj = (GameObject)Instantiate(Resources.Load(splitArray[2]));
                    ObjectMessageHandler omh;
                    if (!obj.GetComponent<ObjectMessageHandler>())
                        omh = obj.AddComponent<ObjectMessageHandler>() as ObjectMessageHandler;
                    if (splitArray.Length > 3)
                        obj.name = splitArray[3];
                        
                }
                if (command == "load" || command == "do")
                {
                    string [] sArray = {""};
                    sArray[0] = cparams;  //convert to string array for loadStreamingAsset
                    print("Load "+ sArray[0]);
                    currentNestingLevel++;
                    StartCoroutine(exectueScenarioFiles(sArray));
                    bool coroutineDone = false;
                    while (currentNestingLevel!=cs.nestingLevel){ //keep doing until coroutine done
                        yield return new WaitForSeconds(1.0f);
                    }
                    
                }
                if (command == "wait"){    //Sleep for a given time in seconds
                    string paramStr = splitArray[2];
                    print("Sleep for "+ paramStr);
                    float delay = float.Parse(paramStr);
                    yield return new WaitForSeconds(delay);
                }
                if (command == "goto"){     //GOTO a line number in the Scenario file
                    string paramStr = splitArray[2];
                    i= int.Parse(paramStr) -2;  //array starts at 0, and i increments, so must minus 2
                    yield return null;  //make sure we don't get caught in infinite loop which hangs unity
                    continue;
                }



            }else //Not Game Manager command, so send commands to other game objects
            {   
                //check if in conditional blocks (THEN or ELSE)
                if (cs.IFstate== CommandSequence.IFState.Then && !cs.IFresult){  //skip then if false
                    print("GM:Skipping THEN block Command["+ cs.commandNum+ "] = " + cs.commandLine);
                    continue;
                }
                if (cs.IFstate== CommandSequence.IFState.Else && cs.IFresult){  //skip else if true
                    print("GM:Skipping ELSE block Command["+ cs.commandNum+ "] = " + cs.commandLine);
                    continue;
                }
                print("IFstate before " + command + " = " + cs.IFstate);
                bool result = processObjectsCommand(objName,command,cparams);
                print("returned from processObjCom " + command + ", result= "+ result);
                print("cs.IFstate after = "+ cs.IFstate);

                //get return value if in conditional statement (last statement was IF)
                if (cs.IFstate == CommandSequence.IFState.Condition){
                    cs.IFresult = result;
                    print("IF Conditional = "+ result);
                }
                //get return value if in conditional statement (last statement was IF)
                if (cs.WAITstate == CommandSequence.WAITState.Condition){
                    bool WAITresult = result;
                    //print("Wait ready = "+ result);
                    if (!WAITresult){ //keep doing until result is ready
                        i--;
                        yield return new WaitForSeconds(1.0f);
                    }
                    else
                        cs.WAITstate = CommandSequence.WAITState.False;
                }
            }
        }
        currentNestingLevel--;
    }

    //Handles states for If-Then-Else-Endif blocks
    bool IFstateSwitch(string command,CommandSequence cs)
    {                    
                //Handle IF-THEN-ELSE-ENDIF statements
                //cs.IFstate keeps track of where we are in the IF statement
                if (cs.IFstate == CommandSequence.IFState.Condition){  //If we are in conditional part (IF)
                    if (command == "then"){         //IF switches us to next state
                        cs.IFstate = CommandSequence.IFState.Then;
                        return true;
                    }
                }
                if (cs.IFstate == CommandSequence.IFState.Then){       //If we are in THEN statements
                    if (command == "else"){         //switch state when ELSE statement
                        cs.IFstate = CommandSequence.IFState.Else;
                        return true;
                    }
                }
                if (command == "endif"){            //Leave cs.IFstate when ENDIF is reached
                        cs.IFstate = CommandSequence.IFState.False;
                        return true;
                }
                if (command == "if"){       //IF command
                    cs.IFstate = CommandSequence.IFState.Condition;
                        return true;
                }
                
                if (cs.IFstate== CommandSequence.IFState.Then && !cs.IFresult)  //skip THEN if conditiional is false
                        return true;
                if (cs.IFstate== CommandSequence.IFState.Else && cs.IFresult)  //skip ELSE if conditional is true
                        return true;
        return false;
    }

    private GameObject[] m_gameObjects;

    //Find children objects of objName for commands meant for multiple objects
    //Supports ANY and ALL patterns
    //e.g.: Lights/ALL
    //Returns list of children
    GameObject [] FindGameObjects(string objName)
    {
  
        print("FindGameObjects for "+ objName);
        int start = objName.Length-1;
        int count = objName.Length;
        int at = objName.LastIndexOf("/", start, count);  
        print("LastIndexOf / = "+ at);
        objName = objName.Remove(at);
        print("Multiple Objects under " + objName + "");

        GameObject go=GameObject.Find(objName);  //   /Room1/Enemy/ANY   or /Room/ANY/Enemy/ANY
        m_gameObjects = new GameObject[go.transform.childCount];
        for (int i = 0; i < go.transform.childCount; i++)
        {
            m_gameObjects[i] = go.transform.GetChild(i).gameObject;
        }
        //TODO 
        return m_gameObjects;
//        return null;
    }

    //Process commands meant for multiple objects
    //Supports ANY and ALL patterns
    //e.g.: Lights/ALL
    bool processObjectsCommand(string objName,string command,string cparams)
    {
        GameObject[] Objects;
        bool retval;

        string [] splitArray = objName.Split('/');
        int last= splitArray.Length-1;
        string endObject = splitArray[last];
        if ((endObject == "ALL") || (endObject == "ANY"))
        {
                print(" Sending command " + command + " to all/any");
                //ADD FindGameObjects function to find ALL children objects of objName, 
                //i.e. Room1/Lights/ALL should find:
                //  Room1/Lights/Light1 and Room1/Lights/Light2
                Objects = FindGameObjects(objName);

                if (endObject == "ANY"){

                    //ADD CODE TO HANDLE "ANY" 
                    int randI = UnityEngine.Random.Range(0,Objects.Length);
                    print("ANY picked object "+ Objects[randI].name);
                    retval = processObjectCommand(Objects[randI],command,cparams);
                    //retval = true;

                }else{  //handle ALL
                    retval = true;

                    foreach (GameObject gameObj in Objects)
                    {
                        print("ALL picked object "+ gameObj.name);
                        retval = processObjectCommand(gameObj,command,cparams);
                    }
                }
        }else{  //only call for named GameObject
            GameObject go=GameObject.Find(objName);
            if (!go){
                print("Object " + objName + " not found.");
                retval = false;
            }
            retval = processObjectCommand(go,command,cparams);
        }
        return retval;
    }

    //Send a GameObject command to appropriate GameObject(s)
    bool processObjectCommand(GameObject go,string command,string cparams)
    {
        //get GameObject name to send command to    
        if (go==null){
            print("GM: ProcessObjectCommand: Object missing for command "+command + " "+ cparams);
            return false;
        }
        print("GM: Sending command " + command + " to " + go.name + " with params " + cparams);

        //get our message handler component from the Game Object
        ObjectMessageHandler mhand = go.GetComponent<ObjectMessageHandler>();
        if (!mhand){
            print("Object " + go.name + " missing message handler.");
            return false;
        }
        //pass command and parameters to message handler
        bool result = mhand.HandleMessage(command,cparams); //commands return BOOL for IF statements
        return result;
    }

    void Update()
    {

    }

    void OnGUI() {
        GUI.contentColor = new Color(1.0f,1.0f,1f);

        GUIStyle style = new GUIStyle(GUI.skin.textArea);

        style.fontSize = Screen.height/40; //change the font size 
        if (GUIcommandLine!=null){

           GUI.TextArea(new Rect(10, 10, Screen.width/2, Screen.height/5), GUIlastcommandLine + "\n" + GUIcommandLine,style);
//           GUI.Label(new Rect(10, 10, 100, 20), cs.commandLine );
 
        }
    }


}
