using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;  //Use this if WWW is obsolete in Unity version

public class MyGameManager3 : MonoBehaviour
{
    public string [] CommandFiles; //contains list of Scenario files

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
       
        StartCoroutine(loadStreamingAsset(CommandFiles));
        //StartCoroutine(ExecuteCommands(commands));

    }

    //loads local or remote file
    IEnumerator loadStreamingAsset(string [] fileNames)
    {
        foreach (string fileName in fileNames){
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

            string result;

            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                WWW www = new WWW(filePath);
                //UnityWebRequest www = UnityWebRequest.Get(filePath); //use this if WWW is obsolete
                yield return www;
                result = www.text;
            }
            else
            {
                result = System.IO.File.ReadAllText(filePath);
            }

            Debug.Log("Loaded file: " + fileNames);
                    //Start the coroutine we define below named ExampleCoroutine.
            string[] linesInFile = result.Split('\n');
            yield return StartCoroutine(ExecuteCommands(linesInFile));
        }
    }

    private GameObject go;
    private int commandNum=0;
    // Update is called once per frame
    enum IFState {False, Condition, Then, Else};
    IFState IFstate = IFState.False;
    bool IFresult = true;
    IEnumerator ExecuteCommands(string [] commands)
    {
        for (int i = 0; i < commands.Length; i++)
        {
            string objName,command,param;
            command=null;
            param=null;
            commandNum=i;
            string commandLine = commands[commandNum];
            commandLine = commandLine.Trim(); //remove extra newlines or whitespace at end
            print("GM:Execute Command["+ commandNum+ "] = " + commandLine);
    
            string [] splitArray = commandLine.Split(' ');
            objName = splitArray[0];
            if (splitArray.Length>1)
                command = splitArray[1];
            if (splitArray.Length>2)
                param = splitArray[2];


            if (objName.Length<1 || objName[0]=='#')  //skip comments
                continue;

            //execute GameManager commands
            if (objName=="GM"){  //special Game Manager command 
                if (IfStateSwitch(command)) //handle IF-THEN-ELSE-ENDIF commands
                    continue;

                //Handle Game Manager commands
                //Add additional commands below.
                if (command == "Sleep"){    //Sleep for a given time in seconds
                    string paramStr = splitArray[2];
                    print("Sleep for "+ paramStr);
                    float delay = float.Parse(paramStr);
                    yield return new WaitForSeconds(delay);
                }
                if (command == "Goto"){     //GOTO a line number in the Scenario file
                    string paramStr = splitArray[2];
                    i= int.Parse(paramStr) -2;  //array starts at 0, and i increments, so must minus 2
                    yield return null;  //make sure we don't get caught in infinite loop which hangs unity
                    continue;
                }



            }else //send commands to other game objects
            {   
                //check if in conditional blocks (THEN or ELSE)
                if (IFstate== IFState.Then && !IFresult)  //skip then if false
                    continue;
                if (IFstate== IFState.Else && IFresult)  //skip else if true
                    continue;

                bool result = processObjectsCommand(objName,command,param);

                //get return value if in conditional statement (last statement was IF)
                if (IFstate == IFState.Condition){
                    IFresult = result;
                    print("Conditional = "+ result);
                }

            }
        }
    }

    bool IfStateSwitch(string command)
    {                    
                //Handle IF-THEN-ELSE-ENDIF statements
                //IFState keeps track of where we are in the IF statement
                if (IFstate == IFState.Condition){  //If we are in conditional part (IF)
                    if (command == "Then"){         //IF switches us to next state
                        IFstate = IFState.Then;
                        return true;
                    }
                }
                if (IFstate == IFState.Then){       //If we are in THEN statements
                    if (command == "Else"){         //switch state when ELSE statement
                        IFstate = IFState.Else;
                        return true;
                    }
                }
                if (command == "Endif"){            //Leave IFstate when ENDIF is reached
                        IFstate = IFState.False;
                        return true;
                }
                if (command == "If"){       //IF command
                    IFstate = IFState.Condition;
                        return true;
                }
                
                if (IFstate== IFState.Then && !IFresult)  //skip THEN if conditiional is false
                        return true;
                if (IFstate== IFState.Else && IFresult)  //skip ELSE if conditional is true
                        return true;
        return false;
    }

    GameObject [] FindGameObjects(string objName)
    {
        //TODO 
        return null;
    }
    bool processObjectsCommand(string objName,string command,string param)
    {
        GameObject[] Objects;
        bool retval;

        go=GameObject.Find(objName);
        if ((objName == "ALL") || (objName == "ANY"))
        {
                print(" Sending command " + command + " to all");
                //ADD FindGameObjects function to find ALL children objects of objName, 
                //i.e. Room1.Lights.ALL should find:
                //  Room1.Lights.Light1 and Room1.Lights.Light2
                Objects = FindGameObjects(objName);

                if (objName == "ANY"){
                    //ADD CODE TO HANDLE "ANY" 
                    retval = true;

                }else{  //handle ALL
                    retval = true;

                    foreach (GameObject gameObj in Objects)
                    {
                        retval = processObjectCommand(gameObj,command,param);
                    }
                }
        }else{  //only call for named GameObject
            GameObject go=GameObject.Find(objName);
            if (!go){
                print("Object " + objName + " not found.");
                retval = false;
            }
            retval = processObjectCommand(go,command,param);
        }
        return retval;
    }

    //Send a GameObject command to appropriate GameObject(s)
    bool processObjectCommand(GameObject go,string command,string param)
    {
        //get GameObject name to send command to    

        print("GM: Sending command " + command + "to " + go.name);

        //get our message handler component from the Game Object
        ObjectMessageHandler mhand = go.GetComponent<ObjectMessageHandler>();
        if (!mhand){
            print("Object" + go.name + " missing message handler.");
            return false;
        }
        //pass command and parameters to message handler
        bool result = mhand.HandleMessage(command,param); //commands return BOOL for IF statements
        return result;
    }

    void Update()
    {

    }



}
