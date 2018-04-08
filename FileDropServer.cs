using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileDropServer
{

    public string IP;               //server IP address
    public string connectedIP;
    public string name;             //server name
    public FileSystem files;        //the file-system
    public Vector2 location;        //location in the fnet
    private static System.Random random = new System.Random();
    public FileDropServer thisServer;   //reference current instance of server
    public bool isNodeVisible;
    public bool isNodeDrawn;
    public GameObject nodeObject;
    public bool fileDropActive;
    public static Terminal term;
    private static EmailListController ELC;

    public FileDropServer(string serverName, string setIP, Vector2 cLoc, Terminal getTerm)
    {
        location = cLoc;
        IP = setIP;
        connectedIP = null;
        name = serverName;
        
        files = new FileSystem();
        thisServer = this;
        isNodeVisible = false;
        isNodeDrawn = false;

        fileDropActive = false;
        nodeObject = null;
        term = getTerm;
        ELC = GameObject.Find("emailListPanel").GetComponent<EmailListController>();
    }

    public FileSystem getFileSys()
    {
        return files;
    }

    public string getIP()
    {
        return IP;
    }

    public static void fileDropUpload(FileEntry file, int faction)
    {
        switch (faction)
        {
            case 0: //Protocol

                //check file upload progressions per the story and where the user is (<faction>MissionProgress, each starting at 0)
                switch (Terminal.protocolMissionProgress)
                {
                    case 0:
                        //start of protocol missions -- with sccs-biosConfig.dll upload
                        if (file.fname.Equals("sccs-biosConfig.dll"))
                        {
                            Terminal.protocolMissionProgress = 1;
                            ELC.addNewEmail(EmailParser.ProtocolEmails[0]);
                        }
                        break;
                }

                break;

            case 1: //SPEC

                //check file upload progressions per the story
                switch (Terminal.specMissionProgress)
                {
                    case 4:
                        if (file.fname.Contains("HBRec_"))
                        {
                            Terminal.specMissionProgress++;
                            ELC.addNewEmail(EmailParser.SPECEmails[5]);
                            Terminal.dracoMissionProgress++;
                            ELC.addNewEmail(EmailParser.DraCOEmails[0]);
                        }
                        break;
                }

                break;

            case 2: //DraCO

                //check file upload progressions per the story
                switch (Terminal.dracoMissionProgress)
                {
                    case 1:
                        if (file.fname.Equals("0x0C0.hex"))
                        {
                            Terminal.dracoMissionProgress++;
                            ELC.addNewEmail(EmailParser.DraCOEmails[2]);
                        }
                        break;
                }

                break;
        }
    }

    public static void strSubmitUpload(int faction)
    {
        switch (faction)
        {
            case 0:   //Protocol
                switch (Terminal.protocolMissionProgress)
                {
                    case 1:	//mission 1 check complete
                        if (term.strSubmitInput.text.Equals("122.93.29.304"))
                        {
                            Terminal.protocolMissionProgress++;
                            ELC.addNewEmail(EmailParser.ProtocolEmails[1]);
                        }
                        break;

                    case 2: //mission 2 check complete
                        if (term.netmap.FindPC("122.93.29.304").getFolder("sys").searchForFile("kernel") == null)
                        {
                            Terminal.protocolMissionProgress++;
                            ELC.addNewEmail(EmailParser.ProtocolEmails[2]);
                        }
                        break;

                    case 3:	//mission 3 check complete
                        bool m3_f = false;
                        for (int i = 0; i < 30; i++)
                        {
                            if (term.netmap.FindPC("93.992.84.7").getFolder("home").searchForFolder("dat").searchForFile("dat" + i) != null)
                            {
                                m3_f = true;
                                break;
                            }
                        }
                        if (!m3_f)
                        {
                            Terminal.protocolMissionProgress++;
                            ELC.addNewEmail(EmailParser.ProtocolEmails[3]);
                        }
						break;

                    case 4: //mission 4 check complete
                        if(term.netmap.FindPC("272.299.0.39").getFolder("sys").searchForFile("kernel") == null)
                        {
                            Terminal.protocolMissionProgress++;
                            ELC.addNewEmail(EmailParser.ProtocolEmails[4]);
                        }
                        break;

                    case 5: //mission 5 check complete (final)
                        if (term.strSubmitInput.text.Equals("199.99.99.20"))
                        {
                            Terminal.protocolMissionProgress++; //no more missions
                            ELC.addNewEmail(EmailParser.ProtocolEmails[5]);
                            Terminal.specMissionProgress++;  //begin SPEC missions
                            ELC.addNewEmail(EmailParser.SPECEmails[0]);
                        }
                        break;
                }
                break;

            case 1:   //SPEC
                switch (Terminal.specMissionProgress)
                {
                    case 0:
                        if (term.netmap.FindPC("79.130.0.22").getFolder("db").doesFileExist("John Silva_Adult_Deceased"))
                        {
                            Terminal.specMissionProgress++;
                            ELC.addNewEmail(EmailParser.SPECEmails[1]);
                        }
                        break;
                    case 1:
                        if (term.netmap.FindPC("127.0.1.2").getFolder("sys").searchForFile("kernel") == null)
                        {
                            Terminal.specMissionProgress++;
                            ELC.addNewEmail(EmailParser.SPECEmails[2]);
                        }
                        break;
                    case 2:
                        if (term.netmap.FindPC("16.822.98.1").getFolder("sys").searchForFile("kernel") == null)
                        {
                            Terminal.specMissionProgress++;
                            ELC.addNewEmail(EmailParser.SPECEmails[3]);
                        }
                        break;
                    case 3:
                        if (term.strSubmitInput.text.Equals("49.992.0.2"))
                        {
                            Terminal.specMissionProgress++;
                            ELC.addNewEmail(EmailParser.SPECEmails[4]);
                        }
                        break;
                }
                break;

            case 2:   //DraCO
                switch (Terminal.dracoMissionProgress)
                {
                    case 0:
                        if (term.strSubmitInput.text.Equals("27.128.83.992"))
                        {
                            Terminal.dracoMissionProgress++;
                            ELC.addNewEmail(EmailParser.DraCOEmails[1]);
                        }
                        break;
                    case 2:
                        for (int d2i = 0; d2i < term.netmap.nodes.Count; d2i++)
                        {
                            if (term.netmap.nodes[d2i].name.Contains("Nyrex"))
                            {
                                if (term.netmap.nodes[d2i].lanType.Equals("database"))
                                {
                                    if(term.netmap.nodes[d2i].getFolder("db").searchForFile("dbEnt_1000") == null)
                                    {
                                        Terminal.dracoMissionProgress++;
                                        ELC.addNewEmail(EmailParser.DraCOEmails[3]);
                                    }
                                }
                            }
                        }
                        break;
                    case 3:
                        for (int d3i = 0; d3i < term.netmap.nodes.Count; d3i++)
                        {
                            if (term.netmap.nodes[d3i].name.Contains("SecRON"))
                            {
                                if (term.netmap.nodes[d3i].lanType.Equals("database"))
                                {
                                    if (term.netmap.nodes[d3i].getFolder("sys").searchForFile("kernel") == null)
                                    {
                                        Terminal.dracoMissionProgress++;
                                        ELC.addNewEmail(EmailParser.DraCOEmails[4]);
                                    }
                                }
                            }
                        }
                        break;
                    case 4:
                        for (int d4i = 0; d4i < term.netmap.nodes.Count; d4i++)
                        {
                            if (term.netmap.nodes[d4i].name.Contains("Exotica"))
                            {
                                if (term.netmap.nodes[d4i].lanType.Equals("database"))
                                {
                                    if (term.netmap.nodes[d4i].getFolder("dds").searchForFile("Lamborghini LP-770-4 // John Ross") != null)
                                    {
                                        Terminal.dracoMissionProgress++;
                                        ELC.addNewEmail(EmailParser.DraCOEmails[5]);
                                    }
                                }
                            }
                        }
                        break;
                    case 5:
                        for (int d5i = 0; d5i < term.netmap.nodes.Count; d5i++)
                        {
                            if (term.netmap.nodes[d5i].name.Contains("fnet/SEC-4"))
                            {
                                if (term.netmap.nodes[d5i].lanType.Equals("database"))
                                {
                                    if (term.netmap.nodes[d5i].getFolder("sys").searchForFile("kernel") == null)
                                    {
                                        Terminal.dracoMissionProgress++;
                                        ELC.addNewEmail(EmailParser.DraCOEmails[6]);
                                    }
                                }
                            }
                        }
                        break;
                    case 6:
                        string d6IP = null;
                        for (int d6i = 0; d6i < term.netmap.nodes.Count; d6i++)
                        {
                            if (term.netmap.nodes[d6i].name.Contains("MoI/OCT"))
                            {
                                if (term.netmap.nodes[d6i].lanType.Equals("database"))
                                {
                                    d6IP = term.netmap.nodes[d6i].IP;
                                }
                            }
                        }
                        if (term.strSubmitInput.text.Equals(d6IP))
                        {
                            Terminal.dracoMissionProgress++;
                            ELC.addNewEmail(EmailParser.DraCOEmails[7]);
                        }
                        break;
                }
                break;
        }
    }
}
