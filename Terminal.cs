using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Terminal : MonoBehaviour {

    [SerializeField] InputField termInput;
    [SerializeField] Text termOutput;
    [SerializeField] Text fileOutput;
    [SerializeField] Text homeIPText;
    [SerializeField] Text locConText;
    [SerializeField] Image bkgControl;
    [SerializeField] public Button strSubmit;
    [SerializeField] public Dropdown strFactionSelect;

    public InputField strSubmitInput;

    private static int NODE_SIZE = 64;
    float inpTimer = 0.05f;
    private float timePassed = 0f;
    public string selfIP;
    public Computer connectedComp;
    public string extConIP;
    public bool connected;
    private bool hackTimerEnabled;

    private float cpuSpeed = 1f;
    public FileSystem termFSys;
    private Folder currentDir;
    private string[] cmdArgs;
    private string helpStr;
    public NetworkMap netmap;
    public List<FileDropServer> fileDrops;

    public static int protocolMissionProgress;
    public static int specMissionProgress;
    public static int dracoMissionProgress;

    private bool hasOSPreinit;
    private bool isBootSplash;
    private bool skipBootSequence;
    private bool termFinishedBoot;

    public bool hasSSHExe;
    public bool hasFTPExe;
    public bool hasSMTPExe;
    public bool hasHTTPExe;
    public bool hasSSLExe;
    public bool hasIMAPExe;

    Text[] portStats;
    Text[] portInfos;
    Text portStatIP;
    Text pNeededText;
    private string[] portsNeeded;

    private Sprite bkgWhite;
    private Sprite bkgRed;
    private Sprite bkgGreen;
    private Sprite bkgBlue;
    private Sprite bkgPurple;
    private Sprite bkgYellow;
    private Sprite bootSplash;
    private Sprite emailRead;
    private Sprite emailUnread;

    private GameObject termCanvas;
    private Canvas missionCanvas;
    private Canvas emailCanvas;
    private Button emailButton;
    private Button notesButton;
    private Button exitButton;

    public Terminal getTerm()
    {
        return this;
    }

    private List<Sprite> bkgImages = new List<Sprite>();

    /*
     TO-DO'S AND IDEAS:
        -make it so that there is a command history saved (as string[] likely) that user can use up-arrow to select previous entries
        -continue making the faction missions and materializing the story
		-make it so that various points in the game could change the color of the background (tense moments could be symbolized with yellow, danger with red, calm with blue or similar, and etc)
		
    LONG-TERM:
        -figure out how to do save/load of game state to file
    */

    public static bool ContainsAny(string str, params string[] subs) //useful for checking if string contains any of a number of substrings
    {
        foreach (string sub in subs)
        {
            if (str.Contains(sub))
                return true;
        }
        return false;
    }

    //--LAN functions--//
    IEnumerator getLANScan()
    {
        if(!connectedComp.isLAN)
        {
            write("fnet> connected computer not part of a LAN!\n\n");
            yield break;
        }
        write("fnet> found links to current LAN node:\n");
        foreach (string lanIP in connectedComp.lanLinks)
        {
            Computer c = netmap.FindPC(lanIP);
            write("  -> " + c.lanType + " at " + lanIP);
        }
        yield return null;
    }

    IEnumerator unlockLockV2()
    {
        if (!connectedComp.isLAN)
        {
            write("fnet> connected computer not part of a LAN!\n\n");
            yield break;
        }
        else if (!connectedComp.lanType.Equals("admin"))
        {
            write("fnet> computer does not have administrator privileges!\n\n");
            yield break;
        }
        write("fnet> unlocking lock-v2.");
        for (int i = 0; i < 2; i++)
        {
            write(".");
            yield return new WaitForSeconds(0.5f);
        }
        connectedComp.cLan.getLockv2().lv2Locked = false;
        write("\n\nfnet> lock-v2 unlocked!\n\n");
    }
    //------------------//

    IEnumerator bootSplashEnum()
    {
        Text bootSplashText = GameObject.Find("bootSplashDot").GetComponent<Text>();
        int counter = 0;
        while (isBootSplash)
        {
            if (counter == 3)
            {
                yield return new WaitForSeconds(1f);
                bootSplashText.text = "";
                isBootSplash = false;
                initOS(); //init the terminal OS
            }
            else
            {
                yield return new WaitForSeconds(1f);
                bootSplashText.text = bootSplashText.text + ".";
            }
            counter++;
        }
    }

    IEnumerator canvasFade(Canvas c, float t)
    {
        float g = 0f;
        var cg = c.GetComponent<CanvasGroup>();
        float a, b;
        bool inte = false;
        if (!cg.interactable) {
            a = 0f; //fade in
            b = 1f;
            c.enabled = true;
            cg.interactable = true;

        } else {
            a = 1f; //fade out
            b = 0f;
            inte = true;
        }

        while (g < t) //for duration specified as 't'
        {
            g += Time.deltaTime;
            c.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(a, b, g / t); //linear interpolate between a and b by t
            yield return null;
        }
        if (inte)
        {
            cg.interactable = false;
            c.enabled = false;
        }
    }

    public void toggleMissionCanvasDisplay()    //opens and closes the notepad/mission display via button press
    {
        StartCoroutine(canvasFade(missionCanvas, 0.25f));
    }

    public void toggleEmailCanvasDisplay()
    {
        StartCoroutine(canvasFade(emailCanvas, 0.25f));
    }

    void Start()    //code is run at the initialization of the scene/game
    {
        EmailParser.init();

        //the UI element containing a notepad and mission viewer
        missionCanvas = GameObject.Find("MissionCanvas").GetComponent<Canvas>();
        missionCanvas.GetComponent<CanvasGroup>().alpha = 0f;
        missionCanvas.GetComponent<CanvasGroup>().interactable = false;
        //the UI element containing the fmail system viewer
        emailCanvas = GameObject.Find("EmailCanvas").GetComponent<Canvas>();
        emailCanvas.GetComponent<CanvasGroup>().alpha = 0f;
        emailCanvas.GetComponent<CanvasGroup>().interactable = false;

        //at start of game, there is no mission progress (SPEC and DraCO are -1 so that you can't jump into those)
        protocolMissionProgress = 0;
        specMissionProgress = -1;
        dracoMissionProgress = -1;

        strSubmitInput = GameObject.Find("strSubmitField").GetComponent<InputField>();

        termFinishedBoot = false;

        netmap = GameObject.Find("termHandler").AddComponent<NetworkMap>();

        //**dev - set to true**//
        skipBootSequence = true;   //by default, skipBootSequence should be set to false for new games (and true for loads -- check if save-file exists)

        //hide the terminal canvas, the init routine needs to run first
        termCanvas = GameObject.Find("Canvas");
        if (!skipBootSequence)
            termCanvas.GetComponent<CanvasGroup>().alpha = 0f;

        //init resources
        bkgWhite = Resources.Load<Sprite>("bkgs/fnet-BkgWhite");
        bkgRed = Resources.Load<Sprite>("bkgs/fnet-BkgRed");
        bkgGreen = Resources.Load<Sprite>("bkgs/fnet-BkgGreen");
        bkgBlue = Resources.Load<Sprite>("bkgs/fnet-BkgBlue");
        bkgPurple = Resources.Load<Sprite>("bkgs/fnet-BkgPurple");
        bkgYellow = Resources.Load<Sprite>("bkgs/fnet-BkgYellow");
        bootSplash = Resources.Load<Sprite>("initOS");
        emailRead = Resources.Load<Sprite>("email");
        emailUnread = Resources.Load<Sprite>("emailNote");

        bkgImages.Add(bkgWhite);    //0 - white
        bkgImages.Add(bkgRed);      //1 - red
        bkgImages.Add(bkgGreen);    //2 - green
        bkgImages.Add(bkgBlue);     //3 - blue
        bkgImages.Add(bkgPurple);   //4 - purple
        bkgImages.Add(bkgYellow);   //5 - yellow

        //initialize buttons
        notesButton = GameObject.Find("notesButton").GetComponent<Button>();
        emailButton = GameObject.Find("emailButton").GetComponent<Button>();
        exitButton = GameObject.Find("exitButton").GetComponent<Button>();

        notesButton.onClick.AddListener(checkNoteButton);
        emailButton.onClick.AddListener(checkEmailButton);
        exitButton.onClick.AddListener(checkExitButton);

        //first things first, initialize all the other things

        if (!skipBootSequence)
        {
            isBootSplash = true;
            bkgControl.sprite = bootSplash;     //start with bootsplash init screen
            StartCoroutine(bootSplashEnum());
        }
        else
            initOS();   //on skip, go directly to OS initialization routine
    }

    void Update()
    {
        timePassed += Time.deltaTime;
        if (!missionCanvas.GetComponent<CanvasGroup>().interactable)
        {
            termInput.ActivateInputField();
            checkInput();
        }
    }

    public void updateEmailGraphic(bool newUnread)
    {
        if (newUnread)
            emailButton.GetComponent<Image>().sprite = emailUnread;
        else
            emailButton.GetComponent<Image>().sprite = emailRead;
    }

    private void checkNoteButton()  //open/close the notepad/mission details viewer
    {
        if (!missionCanvas.GetComponent<CanvasGroup>().interactable)
            termInput.DeactivateInputField();
        else
            termInput.ActivateInputField();

        toggleMissionCanvasDisplay();
    }

    private void checkEmailButton()
    {
        toggleEmailCanvasDisplay();
    }

    private void checkExitButton()
    {
        Application.Quit();
    }

    public void write(string s) //only 51 lines of text can fit in termOutput
    {
        int numLB = 0;  //get number of line breaks in current term text and new string
        foreach (char c in termOutput.text)
            if (c == '\n') numLB++;
        foreach (char c in s)
            if (c == '\n') numLB++;

        if(numLB > 51)  //if there are more line breaks than term text can display
        {
            for(int i = 0; i < (numLB - 51); ++i)   //for every additional line break at the top
            {
                int lineBr = termOutput.text.IndexOf('\n');
                termOutput.text = termOutput.text.Substring(lineBr + "\n".Length);  //remove that line
            }
        }

        termOutput.text += "" + s;
    }

    public void writeFOut(string d)
    {
        fileOutput.text = "" + d;
    }

    private void cmdHist()
    {
        write("\n>" + termInput.text);
    }

    IEnumerator osInitRoutine()
    {
        portsNeeded = new string[7]
        {
            "-- hooks needed: 0 --",
            "-- hooks needed: 1 --",
            "-- hooks needed: 2 --",
            "-- hooks needed: 3 --",
            "-- hooks needed: 4 --",
            "-- hooks needed: 5 --",
            "-- hooks needed: 6 --"
        };

        pNeededText = GameObject.Find("portsNeeded").GetComponent<Text>();
        pNeededText.text = "";
        bkgControl.sprite = bkgImages[0]; //set background image to white (default)

        if (!skipBootSequence)
        {
            var g = 0f;
            while (g < 2f) //fade in the terminal UI
            {
                g += Time.deltaTime;
                termCanvas.GetComponent<CanvasGroup>().alpha += (1f * Time.deltaTime);
                yield return null; //wait a frame
            }

            yield return new WaitForSeconds(0.5f);
        }
        
        hackTimerEnabled = false;

        selfIP = NetworkMap.generateRandomIP();
        extConIP = null;
        if(!skipBootSequence)
            yield return new WaitForSeconds(0.5f);
        locConText.text = "Location: " + "Home-PC$127.0.0.1";

        if(!skipBootSequence)
            yield return new WaitForSeconds(0.5f);
        homeIPText.text = "Home IP: " + selfIP;

        portInfos = new Text[]
        {
            GameObject.Find("p22Stat").GetComponent<Text>(),
            GameObject.Find("p21Stat").GetComponent<Text>(),
            GameObject.Find("p25Stat").GetComponent<Text>(),
            GameObject.Find("p80Stat").GetComponent<Text>(),
            GameObject.Find("p443Stat").GetComponent<Text>(),
            GameObject.Find("p220Stat").GetComponent<Text>()
        };
        portStats = new Text[]
        {
            GameObject.Find("port22T").GetComponent<Text>(),
            GameObject.Find("port21T").GetComponent<Text>(),
            GameObject.Find("port25T").GetComponent<Text>(),
            GameObject.Find("port80T").GetComponent<Text>(),
            GameObject.Find("port443T").GetComponent<Text>(),
            GameObject.Find("port220T").GetComponent<Text>()
        };
        portStatIP = GameObject.Find("PortIPStatText").GetComponent<Text>();

        termFSys = new FileSystem();
        currentDir = termFSys.root;
        connectedComp = null;
        connected = false;

        hasSSHExe = false;
        hasFTPExe = false;
        hasSMTPExe = false;
        hasHTTPExe = false;
        hasSSLExe = false;
        hasIMAPExe = false;

        termFSys.root.files.Add(new FileEntry("readme.txt",     //add a readme file to start the player off -- directs to tutorial PC first
            "// Welcome to the fnet! //\n\n" +
            "You're here for the Central Database, right? Well, you will need to learn how to break into " +
            "other computers.\n\nFirst, you'll want to connect to them via 'connect [ip]. Then, use 'passhack', which does what it says -- retrieves the admin password" +
            " through open ports, if it needs any.\n\n" +
            "You'll want to start here: 192.168.0.1\n\n" + //the tutorial PC's IP address
            "--E")); //plot twist --> E's data server has the last of the six codes on it >:D

        Button btn = strSubmit.GetComponent<Button>();
        btn.onClick.AddListener(misUpload);

        hasOSPreinit = true;
        //last thing to be executed: the startup routine that looks cool
        StartCoroutine(termInitRoutine());
    }

    private void initOS() //initialize the OS
    {
        hasOSPreinit = false;
        StartCoroutine(osInitRoutine());
    }

    IEnumerator termInitRoutine() //cool-looking terminal fnet initialization thing
    {
        if (!skipBootSequence)  //if not skipping boot sequence
        {
            int t = 0;
            write("sys> establishing fnet central database uplink");
            for (t = 0; t < 3; ++t)
            {
                write(".");
                yield return new WaitForSeconds(0.5f);
            }
            write("success!\n");
            yield return new WaitForSeconds(0.2f);
            write("fnet> verifying variable-node link ciphers");
            for (t = 0; t < 3; ++t)
            {
                write(".");
                yield return new WaitForSeconds(1f / 6f);
            }
            for (t = 1; t < 5; ++t)
            {
                write("\nVNL Key " + t + ": " + Computer.generateBinaryString(24));
                yield return new WaitForSeconds(0.125f);
            }
            write("\nfnet> ok!");
            yield return new WaitForSeconds(1f);

            //finish initialization
            termOutput.text = "";
            write("sys> ---init complete!---\nsys> input 'help' for list of commands\n\n");
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            termOutput.text = "";
            write("sys> ---init complete!---\nsys> input 'help' for list of commands\n\n");
        }

        PassGen.init();             //initialize the password gen class
        PortExploits.populate();    //populate the port exploits class that defines everything port-wise for computers
        netmap.initFNet(this);  //init the network

        fileDrops = netmap.fileDropNodes;   //init the file drop server references for the three factions

        termInput.Select();
        termInput.ActivateInputField();
    }

    IEnumerator rmDelEnum(float time, bool rmAll)    //enumerator for deleting all files in a directory
    {
        if (currentDir.files.Count > 0) //check if there are any files to delete
        {
            Folder dir = currentDir;    //set dir to current dir, in case player navigates away whilst rm is running
            var fileName = (rmAll) ? dir.files[0].fname : cmdArgs[1];   //if rmAll, get first file in folder; else, get file name from input
            if (rmAll)  //set to delete all files
            {
                write("\nDeleting all files.");
                while (dir.files.Count > 0)
                {
                    yield return new WaitForSeconds(time);  //wait for time
                    write(".");
                    fileName = dir.files[0].fname;
                    dir.files.RemoveAt(0);

                    if (dir.fname != "log")
                    {
                        if (connected)
                            connectedComp.logEvent(fileName, 0);  //log the deletion of current file in '/log', if current dir is not '/log'
                        else
                            logEvent(fileName, 0); //create log at home vs. remote computer
                    }
                }
            }
            else //set to delete one file
            {
                FileEntry file = dir.searchForFile(fileName);
                if (file.fname.Equals(fileName)) //if file exists in folder
                {
                    write("\nDeleting \"" + file.fname + "\"...");
                    yield return new WaitForSeconds(time);
                    dir.files.Remove(file);

                    if (dir.fname != "log")
                    {
                        if (connected)
                            connectedComp.logEvent(fileName, 0);
                        else
                            logEvent(fileName, 0); 
                    }
                }
            }
            write("Done!\n");
        }
        else write("\nThere are no files to delete!\n");
    }

	IEnumerator rmDelFolder() //delete folder in dir
	{
		Folder dir = currentDir;
        Folder rmF = null;
		var fName = cmdArgs[2].Replace("/","");
		
		if(dir == termFSys.root || dir == connectedComp.files.root) //check if in root directory
			if(fName != "bin" || fName != "home" || fName != "log" || fName != "sys")
				rmF = dir.searchForFolder(fName);
			else
				write("\n--Cannot delete core folder in root dir!");
		else
			rmF = dir.searchForFolder(fName);
		
		if(rmF != null)
		{
			write("\nDeleting /" + rmF.fname + "...");
			yield return new WaitForSeconds(0.5f);
			dir.folders.Remove(rmF);
            write("Done!\n");
		}
	}
	
    IEnumerator scpRemoteFile() //remote file download enumerator
    {
        var fName = cmdArgs[1];
        write("\nfnet: searching for file...");
        yield return new WaitForSeconds(0.5f);
        foreach (FileEntry f in currentDir.files)
            if (f.fname.Equals(fName))
            {
                if (f.fname.Contains(".exe"))   //if downloading an executable
                {
                    termFSys.root.searchForFolder("bin").files.Add(new FileEntry(f.fname, f.fdata));
                    write("\nfnet: file copied to \\bin folder\n");
                }
                else
                {
                    termFSys.root.searchForFolder("home").files.Add(new FileEntry(f.fname, f.fdata));
                    write("\nfnet: file copied to \\home folder\n");
                }

            }
    }

    IEnumerator connectToComputer(string ipConnect) //IP connection enumerator
    {
        if (connected)
            compDisconnect();

        write("\nfnet: looking up " + ipConnect);
        for (int i = 0; i < netmap.nodes.Count; ++i)    //for each node in the fnet
        {
            write(".");
            yield return new WaitForSeconds(0.1f);
            if (netmap.nodes[i].getIP().Equals(ipConnect))
            {
                write("\nfnet: found! connecting...");
                connectedComp = netmap.nodes[i].thisComputer;
                break;
            }
            else continue;
        }
        if (connectedComp != null)
        {
            if (!connectedComp.isNodeVisible)   //if node not visible on netmap yet
            {  
                connectedComp.isNodeVisible = true;
                netmap.updateDrawNodes();
            }

            currentDir = connectedComp.files.root;
            connected = true;
            connectedComp.connectedIP = selfIP;
            connectedComp.logEvent(null, 2);    //computer will log connection
            if (connectedComp.termIsAdmin)
                netmap.changeNodeCircle(connectedComp, "admin");    //change node color to white
            else
                netmap.changeNodeCircle(connectedComp, "target");   //change node color to red

            locConText.text = "Location: " + connectedComp.name + "$" + connectedComp.IP;
            write("\nfnet: connected to " + connectedComp.name + "$" + connectedComp.IP + "\n");

            if (connectedComp.hasBeenScanned)
            {
                portStatIP.text = connectedComp.IP;
                for (int y = 0; y < portStats.Length; ++y)
                {
                    if (connectedComp.type == 1) //type is PC (4 ports)
                    {
                        if (y > 3) break;   //don't go beyond 4 ports
                        portStats[y].text = "" + PortExploits.totPortPC[y] + ":";

                        portInfos[y].color = (connectedComp.ports[PortExploits.totPortPC[y]] == false) ? Color.red : Color.green;
                        portInfos[y].text = (connectedComp.ports[PortExploits.totPortPC[y]] == false) ? "CLOSED" : "OPEN";
                    }
                    else //type is SERVER (6 ports)
                    {
                        portStats[y].text = "" + PortExploits.totPortSV[y] + ":";

                        portInfos[y].color = (connectedComp.ports[PortExploits.totPortSV[y]] == false) ? Color.red : Color.green;
                        portInfos[y].text = (connectedComp.ports[PortExploits.totPortSV[y]] == false) ? "CLOSED" : "OPEN";
                    }
                }
                pNeededText.text = portsNeeded[connectedComp.portsNeededToHack];
            }
        }
        else write("\nfnet: could not find IP!\n");
    }

    private void compDisconnect()   //disconnect from connected-to computer
    {
        if (connected)
        {
            netmap.changeNodeCircle(connectedComp, "disconnect");   //set node back to normal
            connectedComp.connectedIP = null;
            connected = false;
            connectedComp = null;

            for(int i = 0; i < portStats.Length; ++i)
            {
                portStats[i].text = "";
                portInfos[i].text = "";
            }
            portStatIP.text = "";
            pNeededText.text = "";

            currentDir = termFSys.root;
            locConText.text = "Location: " + "Home-PC$127.0.0.1";
            write("\nfnet: disconnected\n");
        }
        else
        {
            write("\nfnet: not connected to a remote system!\n");
        }
    }

    IEnumerator portCrackRoutine(int port)  //crack a closed port
    {
        switch (port)
        {
            case 22:
                write("\nfnet: initializing SSH cracker...");
                yield return new WaitForSeconds(0.5f);
				
                //hacking bits and terminal feedback
				
                write("\nfnet: brute-forcing SSH auths on port " + port);
                for(int i = 0; i < 3; ++i)  //3-second crack
                {
                    write(".");
                    yield return new WaitForSeconds(1.0f);
                }
                connectedComp.ports[22] = true; //port is now open
                connectedComp.logEvent("22", 4);
                write("\nfnet: success! SSH auth port open\n");
                connectedComp.numPortsOpen++;
                portInfos[0].color = Color.green;
                portInfos[0].text = "OPEN";
                break;

            case 21:
                write("\nfnet: starting FTP bounce protocol...");
                yield return new WaitForSeconds(0.5f);

                //hacking bits and terminal feedback

                write("\nfnet: bouncing information through port " + port);
                for (int i = 0; i < 3; ++i)  //3-second crack
                {
                    write(".");
                    yield return new WaitForSeconds(1.0f);
                }
                connectedComp.ports[21] = true; //port is now open
                connectedComp.logEvent("21", 4);
                write("\nfnet: success! gained access through FTP port\n");
                connectedComp.numPortsOpen++;
                portInfos[1].color = Color.green;
                portInfos[1].text = "OPEN";
                break;

            case 25:
				write("\nfnet: probing SMTP services on port...");
                yield return new WaitForSeconds(0.5f);

                //hacking bits and terminal feedback

                write("\nfnet: overloading service through port " + port);
                for (int i = 0; i < 3; ++i)  //3-second crack
                {
                    write(".");
                    yield return new WaitForSeconds(1.0f);
                }
                connectedComp.ports[25] = true; //port is now open
                connectedComp.logEvent("25", 4);
                write("\nfnet: success! SMTP port available\n");
                connectedComp.numPortsOpen++;
                portInfos[2].color = Color.green;
                portInfos[2].text = "OPEN";
                break;

            case 80:
				write("\nfnet: looking for webserver...");
                yield return new WaitForSeconds(0.5f);

                //hacking bits and terminal feedback

                write("\nfnet: sending worm through port " + port);
                for (int i = 0; i < 3; ++i)  //3-second crack
                {
                    write(".");
                    yield return new WaitForSeconds(1.0f);
                }
                connectedComp.ports[80] = true; //port is now open
                connectedComp.logEvent("80", 4);
                write("\nfnet: success! gained access through FTP port\n");
                connectedComp.numPortsOpen++;
                portInfos[3].color = Color.green;
                portInfos[3].text = "OPEN";
                break;

            case 443:
				write("\nfnet: looking for secure sockets...");
                yield return new WaitForSeconds(0.5f);

                //hacking bits and terminal feedback

                write("\nfnet: tricking system into using bad cert via port " + port);
                for (int i = 0; i < 3; ++i)  //3-second crack
                {
                    write(".");
                    yield return new WaitForSeconds(1.0f);
                }
                connectedComp.ports[443] = true; //port is now open
                connectedComp.logEvent("443", 4);
                write("\nfnet: success! SSL port vulnerable\n");
                connectedComp.numPortsOpen++;
                portInfos[4].color = Color.green;
                portInfos[4].text = "OPEN";
                break;

            case 220:
				write("\nfnet: searching for info sent via IMAP...");
                yield return new WaitForSeconds(0.5f);

                //hacking bits and terminal feedback

                write("\nfnet: running protocol analyzer on port " + port);
                for (int i = 0; i < 3; ++i)  //3-second crack
                {
                    write(".");
                    yield return new WaitForSeconds(1.0f);
                }
                connectedComp.ports[220] = true; //port is now open
                connectedComp.logEvent("220", 4);
                write("\nfnet: success! IMAP root login exposed\n");
                connectedComp.numPortsOpen++;
                portInfos[5].color = Color.green;
                portInfos[5].text = "OPEN";
                break;
        }
    }

    IEnumerator portScan()  //nmap utility -- neat GUI display!
    {
        portStatIP.text = "";
        for(int h = 0; h < portStats.Length; ++h)
        {
            portStats[h].text = "";
            portInfos[h].text = "";
        }
        write("\nfnet: running probe on all open connections...");
        yield return new WaitForSeconds(0.5f);
        portStatIP.text = connectedComp.IP;
		yield return new WaitForSeconds(0.5f);
		if(connectedComp.type == 1)
			write("\nfnet: found 4 ports! scanning.");
		else
			write("\nfnet: found 6 ports! scanning.");
		
        StartCoroutine(xcPortStat());
    }
	
	IEnumerator xcPortStat() //portStat utility to follow after IEnum portScan() finishes
	{
        for(int y = 0; y < portStats.Length; ++y)
		{
			if(connectedComp.type == 1) //type is PC (4 ports)
			{
                if (y > 3) break; //don't go beyond 4 ports
				write(".");
				portStats[y].text = "" + PortExploits.totPortPC[y].ToString() + ":";
				yield return new WaitForSeconds(0.4f);

                //set color and text based on whether port is open or not
                portInfos[y].color = (connectedComp.ports[PortExploits.totPortPC[y]] == false) ? Color.red : Color.green;
                portInfos[y].text = (connectedComp.ports[PortExploits.totPortPC[y]]==false) ? "CLOSED" : "OPEN";

				yield return new WaitForSeconds(0.1f);
			}
			else //type is SERVER (6 ports)
			{
				write(".");
				portStats[y].text = "" + PortExploits.totPortSV[y].ToString() + ":";
				yield return new WaitForSeconds(0.4f);
                portInfos[y].color = (connectedComp.ports[PortExploits.totPortSV[y]] == false) ? Color.red : Color.green;
                portInfos[y].text = (connectedComp.ports[PortExploits.totPortSV[y]]==false) ? "CLOSED" : "OPEN";
				yield return new WaitForSeconds(0.1f);
			}
		}
        yield return new WaitForSeconds(0.5f);
        pNeededText.text = portsNeeded[connectedComp.portsNeededToHack];
        connectedComp.hasBeenScanned = true;
		write("\nfnet: portScan complete.");
	}

    IEnumerator passHack() //password hacking utility, usable once all required ports are open
    {
        write("\nfnet: checking port-hook availability...");
        yield return new WaitForSeconds(1f);
        if (connectedComp.checkOpen())  //if ports open equal number of ports needed
            write("done!");
        else
        {
            write("\nfnet: port-hook unable to be established, check ports!");
            yield break;    //exit coroutine
        }

        write("\nfnet: loading wordlist...");
        List<string> passlist = new List<string>();
        foreach (string s in PassGen.passwords)
        {
            passlist.Add(s);
            yield return new WaitForSeconds(0.005f);
        }
        write("done!");
        yield return new WaitForSeconds(0.5f);

        write("\nfnet: trying all in wordlist");
        hackTimerEnabled = true;
        StartCoroutine(passHackTimer());    //start the GUI timer
        StartCoroutine(passHackConsoleTimer()); //start the console timer
        for(int i = 0; i < passlist.Count; ++i)
        {
            yield return new WaitForSeconds(0.00213675f); //the maximum time this should take is 5.4 seconds
            if (connectedComp.adminPass.Equals(passlist[i]))
            {
                hackTimerEnabled = false; //this should stop the timer coroutine
                write("\nfnet: password found! // \"" + passlist[i] + "\"");
                yield return new WaitForSeconds(0.1f);
                connectedComp.termIsAdmin = true;
                write("\nfnet: gained admin access to " + connectedComp.IP + "\n");
                netmap.changeNodeCircle(connectedComp, "admin");
                yield break;
            }
        }
    }

    IEnumerator passHackConsoleTimer() //timer for the passHack function that puts dots after the trying all in wordlist
    {
        while (hackTimerEnabled)
        {
            write(".");
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator passHackTimer() //timer for the passHack function
    {
        float tHack = 0f;
        Text pHackTimer = GameObject.Find("pHackTimer").GetComponent<Text>();
        Text pHTitle = GameObject.Find("pHackTitle").GetComponent<Text>();

        pHTitle.text = "passhack:";
        while (hackTimerEnabled)    //while the timer is going
        {
            tHack += Time.deltaTime;
            pHackTimer.text = "" + tHack.ToString("F2") + " seconds"; //set the GUI timer to the time passed
            yield return null;  //wait for next frame
        }

        //after password has been found --> timer is disabled
        pHTitle.text = "";
        pHackTimer.text = "";
    }

    private void misUpload()    //for when missions ask for an IP address or some other written information
    {
        FileDropServer.strSubmitUpload(strFactionSelect.value);
    }

    private void checkInput()   //check input -- called every frame in Update()
    {
        if(termInput.isFocused && termInput.text != "" && (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter)) && timePassed >= inpTimer) //when user writes command and presses enter
        {
            //check if player has hacking exes before running a command (in case the player does some hacking)
            for(int x = 0; x < termFSys.root.searchForFolder("bin").files.Count; ++x)
            {
				if(!hasSSHExe)
					hasSSHExe = (termFSys.root.searchForFolder("bin").files[x].fname.Equals(PortExploits.cracks[22])) ? true : hasSSHExe;
                if(!hasFTPExe)
					hasFTPExe = (termFSys.root.searchForFolder("bin").files[x].fname.Equals(PortExploits.cracks[21])) ? true : hasFTPExe;
                if(!hasSMTPExe)
					hasSMTPExe = (termFSys.root.searchForFolder("bin").files[x].fname.Equals(PortExploits.cracks[25])) ? true : hasSMTPExe;
                if(!hasHTTPExe)
					hasHTTPExe = (termFSys.root.searchForFolder("bin").files[x].fname.Equals(PortExploits.cracks[80])) ? true : hasHTTPExe;
                if(!hasSSLExe)
					hasSSLExe = (termFSys.root.searchForFolder("bin").files[x].fname.Equals(PortExploits.cracks[443])) ? true : hasSSLExe;
                if(!hasIMAPExe)
					hasIMAPExe = (termFSys.root.searchForFolder("bin").files[x].fname.Equals(PortExploits.cracks[220])) ? true : hasIMAPExe;
            }

            cmdArgs = termInput.text.Split(' ');
            cmdHist(); //writes command to terminal history
            switch(cmdArgs[0]) //big list of commands --> general console stuff, and exe files if they exist on the computer
            {
                #region mv
                case "mv":
                    //move file to another folder (will create folder if it doesn't exist)

                    if (connected) //connected as admin check
                        if (!connectedComp.termIsAdmin)
                        {
                            write("\nfnet: must be admin of remote system!");
                            break;
                        }
                    if (cmdArgs.Length > 2)
                    {
                        if (!cmdArgs[1].Equals("-r"))
                        {
                            var mFile = currentDir.searchForFile(cmdArgs[1]);
                            if (mFile == null)
                            { write("\nsys: file doesn't exist!"); break; }

                            var folder = (cmdArgs[2] == "./") ? currentDir.parentFolder : currentDir.searchForFolder(cmdArgs[2]);
                            if (folder == null)
                                currentDir.folders.Add(new Folder(cmdArgs[2], currentDir));

                            folder.files.Add(new FileEntry(mFile.fname, mFile.fdata));
                            currentDir.files.Remove(mFile);
                            write("\nsys: file moved to \"" + folder.fname + "\".");
                        }
                        else
                        {
                            if (cmdArgs.Length > 3)
                            {
                                var rFile = currentDir.searchForFile(cmdArgs[2]);
                                if (rFile == null)
                                { write("\nsys: file doesn't exist!"); break; }

                                var nFile = new FileEntry(cmdArgs[3], rFile.fdata);
                                currentDir.files.Remove(rFile);
                                currentDir.files.Add(nFile);
                                write("\nsys: file \"" + cmdArgs[2] + "\" renamed to \"" + cmdArgs[3] + "\".");
                            }
                            else write("\nsys: not enough args specified for file rename!");
                        }
                    }
                    else write("\nsys: specify a file and folder to move to, or -r args!");
                    break;
                #endregion

                #region cp
                case "cp":
                    //copy file to another folder

                    if (connected) //connected as admin check
                        if (!connectedComp.termIsAdmin)
                        {
                            write("\nfnet: must be admin of remote system!");
                            break;
                        }
                    if (cmdArgs.Length > 2)
                    {
                        var cFile = currentDir.searchForFile(cmdArgs[1]);
                        if (cFile == null)
                        { write("\nsys: File doesn't exist!"); break; }

                        var folder = (cmdArgs[2] == "./") ? currentDir.parentFolder : currentDir.searchForFolder(cmdArgs[2]);
                        if (folder == null)
                        { write("\nsys: Folder doesn't exist!"); break; }

                        folder.files.Add(new FileEntry(cFile.fname, cFile.fdata));
                        write("\nsys: File copied to " + folder.fname);
                    }
                    else write("\nsys: Specify a file and folder to move to!");
                    break;
                #endregion

                #region rm
                case "rm":
                    //delete file
                    //delete all in current directory with arg '*'

                    if (connected) //connected as admin check
                        if (!connectedComp.termIsAdmin)
                        {
                            write("\nfnet: must be admin of remote system!");
                            break;
                        }
                    if (cmdArgs.Length > 1) //check if second arg is specified
                    {
                        if (cmdArgs[1] == "*")
                            StartCoroutine(rmDelEnum(0.5f, true)); //start a coroutine to delete all files in dir
                        else if (cmdArgs[1].Contains("/"))
                            StartCoroutine(rmDelFolder());
						else
							StartCoroutine(rmDelEnum(0.5f, false)); //start a coroutine to delete specified file in dir
                    }
                    else
                        if(connected)
                            write("\nfnet: Specify '*' or a file to delete!");
                        else
                            write("\nsys: Specify '*' or a file to delete!");

                    break;
                #endregion

                #region cat
                case "cat":
                    //view contents of file
                    var file = currentDir.searchForFile(cmdArgs[1]);
                    if(file != null)
                    {
                        writeFOut(file.fdata);  //write to cat file display panel
                        logEvent(file.fname, 1);
                    }
                    else
                    {
                        write("\nsys: file does not exist!");
                    }
                    break;
                #endregion

                #region scp
                case "scp":
                    if (connected)
                    {
                        if(!connectedComp.termIsAdmin)
                        {
                            write("\nfnet: must be admin of remote system!");
                            break;
                        }
                        if (cmdArgs.Length > 1)
                        {
                            StartCoroutine(scpRemoteFile());
                        }
                        else write("\nfnet: did not specify a file!");
                    }
                    else write("\nfnet: not connected to a remote system!");
                    break;
                #endregion

                #region ls
                case "ls":
                    //list files in directory
                    if (connected) //connected as admin check
                        if (!connectedComp.termIsAdmin)
                        {
                            write("\nfnet: must be admin of remote system!");
                            break;
                        }
                    write("\n" + currentDir.getContents());
                    break;
                #endregion

                #region cd
                case "cd":
                    //set current directory

                    if (connected) //connected as admin check
                        if (!connectedComp.termIsAdmin)
                        {
                            write("\nfnet: must be admin of remote system!");
                            break;
                        }

                    if(cmdArgs.Length < 2)
                    {
                        write("\n--Please specify a folder!--\n");
                        break;
                    }

                    if (cmdArgs[1] == "..")
                        if (currentDir.parentFolder.fname != ".")       //check if root directory
                        {
                            currentDir = currentDir.parentFolder;
                            write("\n" + currentDir.getContents());
                        }
                        else
                            write("\n\n--Cannot exit root dir!--\n\n"); //cannot exit root dir
                    else
                    {
                        if (currentDir.searchForFolder(cmdArgs[1]) != null)
                            currentDir = currentDir.searchForFolder(cmdArgs[1]);
                        else
                        {
                            write("\n--Invalid directory!--\n");
                            break;
                        }
                        write("\n" + currentDir.getContents());
                    }
                    break;
                #endregion

                case "hello":
                    write("\nfnet: hello!");
                    break;

                case "clear": //clear the terminal
                    termOutput.text = "";
                    break;

                case "help": //get help dialog
                    if (cmdArgs.ElementAtOrDefault(1) == null)
                        write(Help.getHelp(cmdArgs[0]));
                    else
                        write(Help.getHelp(cmdArgs[1]));
                    break;

                case "connect":
                    if(cmdArgs.Length > 1)
                        StartCoroutine(connectToComputer(cmdArgs[1]));  //start a coroutine to connect to specified IP
                    break;

                case "dc":
                case "disconnect":
                    compDisconnect();
                    break;

                case "nmap":
                    if (connected)
                        StartCoroutine(portScan());
                    break;

                case "lanScan":
                    if (connected)
                        StartCoroutine(getLANScan());
                    break;

                case "lanUnlockv2":
                    if (connected)
                        StartCoroutine(unlockLockV2());
                    break;

                #region passhack
                case "passhack":
                    if (cmdArgs.Length > 1)
                        break;
                    if (connected)
                        if (connectedComp.hasBeenScanned)
                            if (!connectedComp.termIsAdmin)
                                StartCoroutine(passHack());
                            else
                                write("\nfnet: already admin of remote system!");
                        else
                            write("\nfnet: target not scanned! (use \"nmap\")");
                    else
                        write("\nfnet: not connected to a remote system!");
                    break;
                #endregion

                #region sshcrack
                case "sshcrack":
                    if (hasSSHExe)  //if player has the hacking exe
                        if (connected)  //if player is connected to a remote system
                            if (cmdArgs.Length > 1)
                                if (cmdArgs[1] == "22") //check that specified port is SSH port
                                    if (connectedComp.ports[22] == false)    //if remote system's respective port is closed
                                        StartCoroutine(portCrackRoutine(22));
                                    else
                                        write("\nfnet: port already open!");
                                else
                                    write("\nfnet: invalid SSH port!");
                            else
                                write("\nfnet: port not specified!");
                        else
                            write("\nfnet: not connected to a remote system!");
                    break;
                #endregion

                #region ftpbounce
                case "ftpbounce":
                    if (hasFTPExe)
                        if (connected)
                            if (cmdArgs.Length > 1)
                                if (cmdArgs[1] == "21")
                                    if (connectedComp.ports[21] == false)
                                        StartCoroutine(portCrackRoutine(21));
                                    else
                                        write("\nfnet: port already open!");
                                else
                                    write("\nfnet: invalid FTP port!");
                            else
                                write("\nfnet: port not specified!");
                        else
                            write("\nfnet: not connected to a remote system!");
                    break;
                #endregion

                #region smtpbreak
                case "smtpbreak":
                    if (hasSMTPExe)
                        if (connected)
                            if (cmdArgs.Length > 1)
                                if (cmdArgs[1] == "25")
                                    if (connectedComp.ports[25] == false)
                                        StartCoroutine(portCrackRoutine(25));
                                    else
                                        write("\nfnet: port already open!");
                                else
                                    write("\nfnet: invalid SMTP port!");
                            else
                                write("\nfnet: port not specified!");
                        else
                            write("\nfnet: not connected to a remote system!");
                    break;
                #endregion

                #region httpworm
                case "httpworm":
                    if (hasHTTPExe)
                        if (connected)
                            if (cmdArgs.Length > 1)
                                if (cmdArgs[1] == "80")
                                    if (connectedComp.ports[80] == false)
                                        StartCoroutine(portCrackRoutine(80));
                                    else
                                        write("\nfnet: port already open!");
                                else
                                    write("\nfnet: invalid HTTP port!");
                            else
                                write("\nfnet: port not specified!");
                        else
                            write("\nfnet: not connected to a remote system!");
                    break;
                #endregion

                #region sslcorrupt
                case "sslcorrupt":
                    if (hasSSLExe)
                        if (connected)
                            if (cmdArgs.Length > 1)
                                if (cmdArgs[1] == "443")
                                    if (connectedComp.ports[443] == false)
                                        StartCoroutine(portCrackRoutine(443));
                                    else
                                        write("\nfnet: port already open!");
                                else
                                    write("\nfnet: invalid SSL/HTTPS port!");
                            else
                                write("\nfnet: port not specified!");
                        else
                            write("\nfnet: not connected to a remote system!");
                    break;
                #endregion

                #region imapscan
                case "imapscan":
                    if (hasIMAPExe)
                        if (connected)
                            if (cmdArgs.Length > 1)
                                if (cmdArgs[1] == "220")
                                    if (connectedComp.ports[220] == false)
                                        StartCoroutine(portCrackRoutine(220));
                                    else
                                        write("\nfnet: port already open!");
                                else
                                    write("\nfnet: invalid IMAP port!");
                            else
                                write("\nfnet: port not specified!");
                        else
                            write("\nfnet: not connected to a remote system!");
                    break;
                #endregion

                #region upload
                //upload function for the three factions (story progression)
                case "upload":
                    if (cmdArgs.Length > 2)
                    {
                        for (int i = 0; i < currentDir.files.Count; i++)
                        { //search for file
                            if (cmdArgs[2].Equals(currentDir.files[i].fname))
                            {   //if specified file is found
                                switch (cmdArgs[1]) //upload to respective server
                                {
                                    case "-p":
                                        FileDropServer.fileDropUpload(currentDir.files[i], 0);
                                        break;
                                    case "-s":
                                        FileDropServer.fileDropUpload(currentDir.files[i], 1);
                                        break;
                                    case "-d":
                                        FileDropServer.fileDropUpload(currentDir.files[i], 2);
                                        break;
                                    default:
                                        write("\nfnet: incorrect drop server specified!");
                                        break;
                                }
                            }
                        }
                    }
                    else write("\nincorrect parameters specified for command \"upload\"!");

                    break;
                    #endregion
            }

            timePassed = 0f; //prevents update method from executing checkInput multiple times on one key-press
            termInput.text = "";
            cmdArgs = null;
        }
    }

    private void logEvent(string fName, int ev)    //logs a file event
    {
        Folder logDir = termFSys.root.searchForFolder("log");

        switch (ev)
        {
            case 0: //delete a file
                if(extConIP != null)
                    logDir.files.Add(new FileEntry("" + extConIP + " deleted file \"" + fName + "\"", null));
                else
                    logDir.files.Add(new FileEntry("" + selfIP + " deleted file \"" + fName + "\"", null));
                break;

            case 1: //read a file
                logDir.files.Add(new FileEntry("" + selfIP + " read file \"" + fName + "\"", null));
                break;

            case 2: //remote IP connects
                logDir.files.Add(new FileEntry("" + extConIP + " established connection", null));
                break;
        }
    }
}