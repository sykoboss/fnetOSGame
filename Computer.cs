using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Computer {

    public string IP;                       //computer IP address
    public string connectedIP;
    public string name;                     //computer name
    public byte type;                       //computer type -- 1 for PC, 2 for SERVER
    public byte PC = 1;
    public byte SERVER = 2;
    public FileSystem files;                //the file-system
    public List<string> links;              //connections to other IPs
    public int seclevel;                    //security level
    public Dictionary<int, bool> ports;     //ports on the computer, and whether it is open or closed (true/false)
    public Vector2 location;                //location in the fnet
    public Computer thisComputer;           //reference current instance of computer
    public bool isNodeVisible;
    public bool isNodeDrawn;
    private bool isHackable;
    public int numPortsOpen;
    public int portsNeededToHack;
	public bool termIsAdmin;
    public bool hasBeenScanned;
    public GameObject nodeObject;
    public string adminPass;
    private static System.Random random = new System.Random();
    public bool isLAN;
    public string lanType;
    public List<string> lanLinks;
    public bool lv2Locked;
    public LocalAreaNetwork cLan;

    //regular computer constructor
    public Computer(string pcName, string setIP, Vector2 cLoc, int security, byte cType)
    {
        isLAN = false;
        lanType = null;
        lanLinks = null;
        location = cLoc;
        IP = setIP;
        connectedIP = null;
        name = pcName;
        type = cType;
        seclevel = security;

		termIsAdmin = false;
        files = new FileSystem();
        thisComputer = this;
        isNodeVisible = false;
        isNodeDrawn = false;
        hasBeenScanned = false;
        adminPass = PassGen.getRandomPass();

        nodeObject = null;

        ports = new Dictionary<int, bool>();
        genSecPorts();
    }

    //LAN computer/node constructor
    public Computer(bool lanIs, string nName, string nIP, Vector2 cLoc, int security, string lTy)
    {
        cLan = null;
        isLAN = true;
        lanType = lTy;
        if (lTy.Equals("lock-v2")) lv2Locked = true;
        lanLinks = new List<string>();
        location = cLoc;
        IP = nIP;
        connectedIP = null;
        name = nName;
        type = 1;
        seclevel = security;

        termIsAdmin = false;
        files = new FileSystem();
        thisComputer = this;
        isNodeVisible = false;
        isNodeDrawn = false;
        hasBeenScanned = false;
        adminPass = PassGen.getRandomPass();

        nodeObject = null;

        ports = new Dictionary<int, bool>();
        genSecPorts();
    }

    public Folder getFileSys()
    {
        return files.root;
    }

    public Folder getFolder(string fname)
    {
        for(int i = 0; i < files.root.folders.Count; i++)
        {
            if (files.root.folders[i].fname.Equals(fname))
            {
                return files.root.folders[i];
            }
        }
        return null;
    }

    public string getIP()
    {
        return IP;
    }

    public static string generateBinaryString(int length)
    {
        var buffer = new byte[length / 8];
        random.NextBytes(buffer);
        var str = "";
        for (var index = 0; index < buffer.Length; ++index)
            str += Convert.ToString(buffer[index], 2);
        return str;
    }

    public void genSecPorts()
    {
        if (type == PC)             //add non-server ports (set closed)
        {
            ports.Add(PortExploits.portNums[0], false); //SSH
            ports.Add(PortExploits.portNums[1], false); //FTP
            ports.Add(PortExploits.portNums[2], false); //SMTP
            ports.Add(PortExploits.portNums[3], false); //HTTP
        }
        else if (type == SERVER)    //add all ports (set closed)
        {
            ports.Add(PortExploits.portNums[0], false);
            ports.Add(PortExploits.portNums[1], false);
            ports.Add(PortExploits.portNums[2], false);
            ports.Add(PortExploits.portNums[3], false);
            ports.Add(PortExploits.portNums[4], false); //HTTPS/SSL
            ports.Add(PortExploits.portNums[5], false); //IMAP3
        }
        isHackable = false;
        numPortsOpen = 0;
        portsNeededToHack = seclevel;
        if (type == PC && portsNeededToHack > 4) //normalize the PC number of ports needed if it is greater than 4
            portsNeededToHack = 4;
    }

    public bool checkOpen()
    {
        isHackable = (numPortsOpen == portsNeededToHack);
        return isHackable;    
    }

    public void logEvent(string fName, int ev)    //logs a file event
    {
        Folder logDir = files.root.searchForFolder("log");

        switch (ev)
        {
            case 0: //delete a file
                logDir.files.Add(new FileEntry("" + connectedIP + " deleted file \"" + fName + "\"", null));
                break;

            case 1: //read a file
                logDir.files.Add(new FileEntry("" + connectedIP + " read file \"" + fName + "\"", null));
                break;

            case 2: //remote IP connects
                logDir.files.Add(new FileEntry("" + connectedIP + " established connection", null));
                break;

            case 3: //remote IP downloads a file
                logDir.files.Add(new FileEntry("" + connectedIP + " copied file \"" + fName + "\" from local host", null));
                break;

            case 4: //remote IP opens a port
                logDir.files.Add(new FileEntry("" + connectedIP + " opened port " + fName, null));
                break;
        }
    }

    //--LAN computer methods below this point--//

    public string getLANType() //will return null if not a LAN computer
    {
        return lanType;
    }

    public void addLANLinks(string c1) //add single link
    {
        if (c1 != null)
            lanLinks.Add("" + c1);
    }

    public void addLANLinks(string c1, string c2) //add two links
    {
        if(c1 != null)
            lanLinks.Add("" + c1);
        if(c2 != null)
            lanLinks.Add("" + c2);
    }

    public void addLANLinks(string c1, string c2, string c3, string c4) //add four links (for routers)
    {
        if (c1 != null)
            lanLinks.Add("" + c1);
        if (c2 != null)
            lanLinks.Add("" + c2);
        if (c3 != null)
            lanLinks.Add("" + c3);
        if (c4 != null)
            lanLinks.Add("" + c4);
    }
}
