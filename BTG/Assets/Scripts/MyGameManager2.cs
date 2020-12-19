using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGameManager2 : MonoBehaviour
{
    public string [] CommandFiles; 
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
            if (objName=="GM"){  


                if (command == "Sleep"){
                    string paramStr = splitArray[2];
                    print("Sleep for "+ paramStr);
                    float delay = float.Parse(paramStr);
                    yield return new WaitForSeconds(delay);
                }
                if (command == "Goto"){
                    string paramStr = splitArray[2];
                    i= int.Parse(paramStr) -2;
                    continue;
                }



            }else //send commands to other game objects
            {   

                //get game object to send command to    
                go=GameObject.Find(objName);
                if (!go){
                    print("Object " + objName + " not found.");
                    continue;
                }

                print("GM: Sending command " + command + "to " + objName);

                //get our message handler component
                ObjectMessageHandler mhand = go.GetComponent<ObjectMessageHandler>();
                if (!mhand){
                    print("Object" + objName + " missing message handler.");
                    continue;
                }
                bool result = mhand.HandleMessage(command,param);


            }
        }
    }

    void Update()
    {

    }



}
