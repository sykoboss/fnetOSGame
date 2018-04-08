using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMap : MonoBehaviour {

    public int NODE_SIZE = 28;
    public float ADMIN_CIRCLE_SCALE = 0.62f;
    public Sprite adminCircle;
    public Sprite adminNodeCircle;
    public Sprite circle;
    public Vector2 circleOrigin;
    public Sprite circleOutline;
    public bool DimNonConnectedNodes;
    public Sprite homeNodeCircle;
    public Computer lastAddedNode;
    public Sprite nodeCircle;
    public Sprite targetNodeCircle;
    private float rotation;
    private static System.Random random = new System.Random();
    public static GameObject netmapPanel;
    public static RectTransform rt;
    public static GameObject lineRender;
    public static LineRenderer lr;
    public Vector3 pos;
    public Terminal term;
    public float rtMinX;
    public float rtMaxX;
    public float rtMinY;
    public float rtMaxY;
    public bool fnetInitialized;
    public NameGen nameGen;
    public List<Computer> nodes;    //computers on the network
    public List<int> visibleNodes;  //index of computers in 'nodes' visible
    public List<GameObject> nodeDraw = new List<GameObject>();
    public List<FileDropServer> fileDropNodes = new List<FileDropServer>();
	public int nmStoryIndex;
    public static string[] peopleNames = //50 names
    {
        "Faith Araiza",
        "Solange Puccio",
        "Carlita Tinney",
        "Kacy Wisener",
        "Concha Flinn",
        "Millie Horst",
        "Jody Largo",
        "Maryanna Gonce",
        "Marylynn Weidman",
        "Tena Middlebrooks",
        "Rosamaria Heaps",
        "Cammie Thibeault",
        "Evangelina Streets",
        "Laverna Lansing",
        "Broderick Vierra",
        "Brianne Futral",
        "Jonathan Wagnon",
        "Gwyneth Wolfson",
        "Claude Baldridge",
        "Shakita Riemann",
        "Collette Clabaugh",
        "Fletcher Vanhouten",
        "Noelle Thor",
        "Theressa Troy",
        "Son Paneto",
        "Stephnie Luken",
        "Joellen Mancil",
        "Aron Gawronski",
        "Larae Factor",
        "Tyra Pascual",
        "Archie Vinzant",
        "Margorie Mccaa",
        "Latia Golliday",
        "Shanae Penney",
        "Julianne Kale",
        "Opal Jock",
        "Trinity Pigman",
        "Lasandra Rider",
        "Allan Heimbach",
        "Vincenzo Vanhorn",
        "Foster Schippers",
        "Ed Hassan",
        "Dominic Bartholomew",
        "Jeffery Branch",
        "Krystina Fraire",
        "Tasha Benavidez",
        "Nadia Begin",
        "Jaimee Templeton",
        "Gerri Tarbell",
        "Loise Purkey"
    };
    public static string[] peopleStatus =
    {
        "Young_Alive",
        "Young_Injured",
        "Young_Critical",
        "Young_Deceased",
        "Adult_Alive",
        "Adult_Injured",
        "Adult_Critical",
        "Adult_Deceased",
        "Elder_Alive",
        "Elder_Injured",
        "Elder_Critical",
        "Elder_Deceased"
    };


    /*public NetworkMap(Terminal getTerm) //constructor -- initialize all of the variables
    {
        
    }*/

    public void initFNet(Terminal getTerm) //initialize the entire fnet
    {
        fnetInitialized = false;
        term = getTerm;

        netmapPanel = GameObject.Find("NetmapPanel");
        lineRender = GameObject.Find("lineRender");
        rt = netmapPanel.GetComponent<RectTransform>();
        lr = lineRender.GetComponent<LineRenderer>();

        pos = rt.position;
        rtMinX = -(rt.rect.width / 2) + NODE_SIZE;
        rtMaxX = rt.rect.width / 2 - NODE_SIZE;
        rtMinY = -(rt.rect.height / 2) + NODE_SIZE;
        rtMaxY = rt.rect.height / 2 - NODE_SIZE;

        nameGen = new NameGen();
        nameGen.initNGen();

        //-----------------------------------------------------

        visibleNodes = new List<int>();
        //nodes = generateNetwork();
        nodes = new List<Computer>();
        nodes.AddRange(generateGameNodes());
        for(int i = 0; i < nodes.Count; i++)
        {
            Console.WriteLine("Index "+ i +": "+ nodes[i].name +" // "+ nodes[i].IP);
        }

        nodeCircle = Resources.Load<Sprite>("nodeCircle");
        adminNodeCircle = Resources.Load<Sprite>("adminNodeCircle");
        homeNodeCircle = Resources.Load<Sprite>("playerNodeCircle");
        targetNodeCircle = Resources.Load<Sprite>("targetNodeCircle");

        drawNodes();
        fileDropNodes = generateFactionDrops();
        fnetInitialized = true;
    }

    public Computer FindPC(string IP)
    {
        for(int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].IP.Equals(IP))
                return nodes[i];
        }
        return null;
    }
     
    public void Update()
    {
        rotation += 0.01f / 2f;
    }

    /*public string getVisibleNodesString()
    {
        var str = "<visible>";
        for (var index = 0; index < visibleNodes.Count; ++index)
            str = str + visibleNodes[index] + (index != visibleNodes.Count - 1 ? " " : "");
        return str + "</visible>";
    }*/

    public List<FileDropServer> generateFactionDrops()
    {
        var list = new List<FileDropServer>();
            //list[0] = Protocol
            //list[1] = SPEC
            //list[2] = DraCO
        
        list.Add(new FileDropServer("Protocol Drop Server", generateRandomIP(), randomNodePos(), term));  //[0]
        list.Add(new FileDropServer("SPEC Drop Server", generateRandomIP(), randomNodePos(), term));      //[1]
        list.Add(new FileDropServer("DraCO Drop Server", generateRandomIP(), randomNodePos(), term));     //[2]

        return list;
    }

    public List<Computer> generateNetwork() //rewritten fnet generation code
    {
        var fnet = new List<Computer>();

        for (var i = 0; i < 10; ++i)        //10 starting computers
            fnet.Add(new Computer(nameGen.generateName(), generateRandomIP(), randomNodePos(), random.Next(0, 4), Utils.randomCompType()));
        
        return fnet;
    }

    public List<Computer> generateGameNodes()
    {
        var list = new List<Computer>();
        int id = 0;
        /*
         * list.Add(new Computer("Hacker Stash", "133.333.333.337 ", getRandomPosition(), 0, 2));
         */

        nmStoryIndex = nodes.Count;

        /* id list by faction:
         * -------------------
         * Protocol =  0 - 13
         * SPEC ===== 14 - 25
         * DraCO ==== 26 - ??
         */

        #region tutorial-pcs

        //index 0 -- Generate tutorial PC #1
        list.Add(new Computer("E's Test PC", "192.168.0.1", randomNodePos(), 0, 1));
        var stFold = list[id].files.root.searchForFolder("bin");    //a variable used to reference the file-systems of all story computers
        stFold.files.Add(new FileEntry(PortExploits.cracks[22], PortExploits.crackExeData[22]));
        stFold = list[id].files.root.searchForFolder("home");
        stFold.files.Add(new FileEntry("tut-2.txt",
            "You've completed the first step towards becoming a hacker!\n\n" +
            "Now, you need to break into another computer. It's owned by a friend of mine, but it " +
            "has been a while since he was on last. However, it won't be as easy as getting into this computer: " +
            "you will need to open a port.\n\nFor any port, you will need the cracker for that " +
            "port first. I've provided you with the SSH port cracker, which is found in the \"bin\" folder on this computer. + " +
            "\n To move files from a computer you're connected to, you can use the \"scp [file]\" command.\n\n" +
            "You can then use the cracker by typing the name of the exe file, \"sshcrack\", and then the port number.\n\n" +
            "Another thing you shouldn't forget to do is delete logs. They keep track of everything you do to a computer, and so you'll" +
            "want to clear only yours. BE CAREFUL: other logs might come in handy.\n\n" +
            "-E\n\n192.168.0.2"));
        id++;

        //index 1 -- generate second tut PC
        list.Add(new Computer("Michael PC", "192.168.0.2", randomNodePos(), 1, 1));
        stFold = list[id].files.root.searchForFolder("bin");
        stFold.files.Add(new FileEntry(PortExploits.cracks[21], PortExploits.crackExeData[21]));
        stFold = list[id].files.root.searchForFolder("home");
        stFold.files.Add(new FileEntry("games-list.txt", "Games to get:\n--R6 Siege\n--PUBG\n--DOOM\n--some other one"));
        stFold.folders.Add(new Folder("misc", stFold));
        stFold = stFold.searchForFolder("misc");
        stFold.files.Add(new FileEntry("hacker-orgs.txt",  //interesting info for people who dig lore
            "Been doing some digging on this 'fnet', and found a few hacker groups I should probably note down (like right now):" +
            "\n\n\"Protocol\"" +
            "\n---modestly-sized and hidden one, they do general security and counter-hacking\n---Good place for assets, so I hear" +
            "\n---(I haven't started the trial yet, but it's there in-case I want to)" +
            "\n\n\"SPEC\"" +
            "\n---another moderately-sized one, but has quite a bit more of a reputation (for some reason)" +
            "\n---an ex-SPEC hacker is hiding somewhere, according to a prominent source" +
            "\n\n\"DraCO\"" +
            "\n---a smaller and more elite group, hard to find details on (takes only the best)" +
            "\n---more of a grey-hat reputation, pretty untraceable" +
            "\n\n\"rS-Rus\"" +
            "\n---a small Russian group, the most malicious of all listed" +
            "\n---(I wouldn't want to mess with them)" +
            "\n\n-I think there's another one, but my digging reveals absolutely jack-squat nothing...damn!"));
        stFold.files.Add(new FileEntry("protocol-trial.txt",   //init text file for getting into Protocol -- Chapter 1
            "So, you think you have what it takes to become part of Protocol? Prove it." +
            "\n\nBreak into 176.67.19.422, and go from there.\n\n-JaZz"));
        id++;

        #endregion

        #region story

        #region protocol-introduction
        //index 2 -- Protocol intro server
        list.Add(new Computer("P-Init Server", "176.67.19.422", randomNodePos(), 2, 2));
        {
            stFold = list[id].files.root.searchForFolder("home");
            stFold.files.Add(new FileEntry("protocol-trial.txt",   //init text file for getting into Protocol -- Chapter 1
                "Well, you appear competent enough to at least get into a computer. Now, how about tracking down a file?" +
                "\n\nStart at 210.678.2.14, and find a file named \"sccs-biosConfig.dll\" somewhere on their network.\n\n-JaZz"));
            id++;
        }
        //index 3
        list.Add(new Computer("SCCS File Redirection Server", "210.678.2.14", randomNodePos(), 1, 2));
        {
            stFold = list[id].files.root.searchForFolder("home");
            stFold.files.Add(new FileEntry("readme.txt",
                "NOTE: Distribution and config files are not to be kept here! This server is used to direct those over the network." +
                "\n\nTo-do: configure the log files to not show the IP of the storage server"));
            stFold = list[id].files.root.searchForFolder("log");
            stFold.files.Add(new FileEntry("LOCALHOST_redirected_file_from_210.678.3.106", null)); //the server with the file on it
            id++;
        }
        //index 4
        list.Add(new Computer("SCCS File Storage Server", "210.678.3.106", randomNodePos(), 2, 2));
        {
            stFold = list[id].files.root;
            stFold.folders.Add(new Folder("dist", stFold));
            stFold = stFold.searchForFolder("dist");
            stFold.files.Add(new FileEntry("sccsInitConfig.scs", Computer.generateBinaryString(256)));
            stFold.files.Add(new FileEntry("sccsCSS.html", Computer.generateBinaryString(256)));
            stFold.files.Add(new FileEntry("sccs-run.bat", Computer.generateBinaryString(256)));
            stFold.files.Add(new FileEntry("sccs-biosConfig.dll", Computer.generateBinaryString(256)));
            stFold.files.Add(new FileEntry("sccsBootDiskMgr.scs", Computer.generateBinaryString(256)));
            id++;
        }
        #endregion

        #region protocol-missions
        //index 5
        list.Add(new Computer("0x00cf-c", "122.93.29.304", randomNodePos(), 1, 2));

        //index 6
        list.Add(new Computer("Mal-PC", "93.992.84.7", randomNodePos(), 2, 1));
        {
            stFold = list[id].files.root.searchForFolder("bin");
            stFold.files.Add(new FileEntry(PortExploits.cracks[25], PortExploits.crackExeData[25]));
            stFold = list[id].files.root.searchForFolder("home");
            stFold.folders.Add(new Folder("dat", stFold));
            stFold = stFold.searchForFolder("dat");
            for (int i = 0; i < 30; i++)
            {
                stFold.files.Add(new FileEntry("dat" + i, Computer.generateBinaryString(256)));
            }
            id++;
        }
        //index 7
        list.Add(new Computer("Sysco Public Interface", "104.2.99.338", randomNodePos(), 1, 1));
        {
            stFold = list[id].files.root.searchForFolder("log");
            stFold.files.Add(new FileEntry("Connection_routed_to_92.22.228.93", null));
            id++;
        }
        //index 8
        list.Add(new Computer("Sysco Intercom", "92.22.228.93", randomNodePos(), 2, 2));
        {
            list[id].connectedIP = "272.299.0.39";
            list[id].logEvent(null, 2);  //connects
            list[id].logEvent("220", 4); //opens port
            list[id].logEvent("imDat00", 3); //downloaded file
            list[id].logEvent("imDat00", 0); //deleted file
            list[id].connectedIP = null;
            id++;
        }
        //index 9
        list.Add(new Computer("7e899f9c0-FCX", "272.299.0.39", randomNodePos(), 3, 1));
        {
            stFold = list[id].files.root.searchForFolder("home");
            stFold.files.Add(new FileEntry("imDat00", Computer.generateBinaryString(64)));
            id++;
        }
        //index 10-13 -- SPEC member trace (this IP address provided through email)
        list.Add(new Computer("0e89730ffc-SSC", "182.25.964.56", randomNodePos(), 2, 1));
        {
            list[id].connectedIP = generateRandomIP();
            for (int i = 0; i < 3; i++) //generate multiple paths
            {
                list[id].logEvent(Computer.generateBinaryString(4), random.Next(3));
            }
            list[id].connectedIP = "182.25.782.20"; //the correct path
            list[id].logEvent(Computer.generateBinaryString(4), random.Next(3));
            id++;
        }
        list.Add(new Computer("0f97350ffc-SSD", "182.25.782.20", randomNodePos(), 2, 1));
        {
            list[id].connectedIP = generateRandomIP();
            for (int i = 0; i < 3; i++) //generate multiple paths
            {
                list[id].logEvent(Computer.generateBinaryString(4), random.Next(3));
            }
            list[id].connectedIP = "182.25.648.14"; //the correct path
            list[id].logEvent(Computer.generateBinaryString(4), random.Next(3));
            id++;
        }
        list.Add(new Computer("0c45236ffc-SSE", "182.25.648.14", randomNodePos(), 1, 1));
        {
            list[id].connectedIP = generateRandomIP();
            for (int i = 0; i < 3; i++) //generate multiple paths
            {
                list[id].logEvent(Computer.generateBinaryString(4), random.Next(3));
            }
            list[id].connectedIP = "182.25.782.20"; //the correct path
            list[id].logEvent(Computer.generateBinaryString(4), random.Next(3));
            id++;
        }
        list.Add(new Computer("0b32339ffc-SSF", "182.25.210.99", randomNodePos(), 3, 1));
        {
            stFold = list[id].files.root;
            stFold.files.Add(new FileEntry("spec-read.txt", "You found me. Just give them the IP address \"199.99.99.20\" and move along. My former boss will give you further instructions.\n\n-Agent S"));
            id++;
        }

        #endregion

        #region spec-missions
        //index 14-16 -- Synamed Hospital
        list.Add(new Computer("Synamed Hospital Public Access", "78.993.65.427", randomNodePos(), 1, 1));
        {
            stFold = list[id].files.root.searchForFolder("log");
            stFold.files.Add(new FileEntry("Connection_routed_to_78.737.8.54", null));
            stFold.files.Add(new FileEntry("Connection_routed_to_78.921.22.9", null));
            id++;
        }
        list.Add(new Computer("Synamed Hospital Internal Server", "78.921.22.9", randomNodePos(), 2, 1));
        {
            stFold = list[id].files.root.searchForFolder("log");
            stFold.files.Add(new FileEntry("Connection_routed_from_78.737.8.54", null));
            stFold.files.Add(new FileEntry("Connection_routed_to_78.130.0.22", null));
            stFold.files.Add(new FileEntry("Connection_routed_to_79.34.18.93", null));
            id++;
        }
        list.Add(new Computer("Synamed Hospital Database", "78.130.0.22", randomNodePos(), 3, 1));
        {
            stFold = list[id].files.root.searchForFolder("log");
            stFold.files.Add(new FileEntry("Connection_routed_from_78.737.8.54", null));
            stFold.files.Add(new FileEntry("Connection_routed_from_78.921.22.9", null));
            stFold = list[id].files.root;
            stFold.folders.Add(new Folder("db", null));
            stFold = list[id].files.root.searchForFolder("db");
            for (int m8_i = 0; m8_i < 26; m8_i++)
            {
                if (m8_i == 14)
                    stFold.files.Add(new FileEntry("John Silva_Adult_Alive", null));
                else
                    stFold.files.Add(new FileEntry("" + peopleNames[Utils.random.Next(0, peopleNames.Length - 1)] + "_" + peopleStatus[Utils.random.Next(0, peopleStatus.Length - 1)], null));
            }
            id++;
        }
        //index 17-19 -- botnet
		list.Add(new Computer("rsLof-PC","99.229.654.21", randomNodePos(), 2, 1));
        {
            stFold = list[id].files.root.searchForFolder("log");
            for (int m9_i1 = 0; m9_i1 < 3; m9_i1++)
            {
                stFold.files.Add(new FileEntry("bnman_route_to_" + generateRandomIP(), null));
            }
            stFold.files.Add(new FileEntry("bnman_route_to_99.247.20.337", null));
            id++;
        }
		list.Add(new Computer("bsmse-Rfserver","99.247.20.337",randomNodePos(),2,2));
        {
            stFold = list[id].files.root.searchForFolder("log");
            for (int m9_i2 = 0; m9_i2 < 8; m9_i2++)
            {
                stFold.files.Add(new FileEntry("bnman_route_to_" + generateRandomIP(), null));
            }
            stFold.files.Add(new FileEntry("bnman_route_to_127.0.1.2", null));
            id++;
        }
		list.Add(new Computer("bnman-Man","127.0.1.2",randomNodePos(),3,2));
        {
            stFold = list[id].files.root.searchForFolder("log");
            for (int m9_i3 = 0; m9_i3 < 24; m9_i3++)
            {
                stFold.files.Add(new FileEntry("bnman_rf_man_" + generateRandomIP(), null));
            }
            stFold = list[id].files.root.searchForFolder("bin");
            stFold.files.Add(new FileEntry(PortExploits.cracks[80], PortExploits.crackExeData[80]));
            id++;
        }
        //index 20-22 -- SPEC central database
        list.Add(new Computer("SPEC CD-0f3c", "77.777.233.530", randomNodePos(), 5, 2));
        {
            list[id].termIsAdmin = true;
            list[id].ports[443] = true; //port is open
            list[id].connectedIP = "218.0.22.3#";
            list[id].logEvent("443", 4);
            list[id].connectedIP = null;
            stFold = list[id].files.root.searchForFolder("log");
            stFold.files.Add(new FileEntry("remote-hashrem_ip&int=4", null));
            id++;
        }
        list.Add(new Computer("$%&*@#!))", "218.0.22.34", randomNodePos(), 3, 1));
        {
            stFold = list[id].files.root.searchForFolder("log");
            stFold.files.Add(new FileEntry("route_16.822.9#.1", null));
            stFold.files.Add(new FileEntry("remote-hashrem_ip&int=8", null));
            id++;
        }
        list.Add(new Computer("SCore-x31", "16.822.98.1", randomNodePos(), 4, 2));
        {
            stFold = list[id].files.root.searchForFolder("bin");
            stFold.files.Add(new FileEntry(PortExploits.cracks[220], PortExploits.crackExeData[220]));
            stFold = list[id].files.root.searchForFolder("log");
            stFold.files.Add(new FileEntry("localhost_deleted_file_tDrill.exe", null));
            stFold.files.Add(new FileEntry("localhost_deleted_file_hashrem.exe", null));
            stFold.files.Add(new FileEntry("tDrill_remote-->B1-->DB", null));
            stFold.files.Add(new FileEntry("hashrem_ip&int=8-->B1", null));
            stFold.files.Add(new FileEntry("hashrem_ip&int=4-->B1-->DB", null));
            id++;
        }
        //index 23-25 -- Hit Bank
        list.Add(new Computer("Hit Bank Public Access", "100.99.388.929", randomNodePos(), 4, 2));
        {
            stFold = list[id].files.root.searchForFolder("log");
            for (int m11_i1 = 0; m11_i1 < 4; m11_i1++)
            {
                stFold.files.Add(new FileEntry("bank_handle: " + random.Next(100000,999999), null));
            }
            stFold.files.Add(new FileEntry("bank_redir: 100.28.27.54", null));
            for (int m11_i2 = 0; m11_i2 < 4; m11_i2++)
            {
                stFold.files.Add(new FileEntry("bank_handle: " + random.Next(100000, 999999), null));
            }
            id++;
        }
        list.Add(new Computer("HitBankProcServer", "100.28.27.54", randomNodePos(), 5, 2));
        {
            list[id].files.root.folders.Add(new Folder("rec", list[id].files.root));
            stFold = list[id].files.root.searchForFolder("rec");
            for (int m11_i3 = 0; m11_i3 < 30; m11_i3++)
            {
                stFold.files.Add(new FileEntry("HBRec_" + random.Next(100000, 999999), Computer.generateBinaryString(256)));
            }
            stFold = list[id].files.root.searchForFolder("log");
            for (int m11_i1 = 0; m11_i1 < 8; m11_i1++)
            {
                stFold.files.Add(new FileEntry("bank_handle: " + random.Next(100000, 999999), null));
            }
            stFold.files.Add(new FileEntry("bank_rem_dl-->49.992.#.2", null));
            stFold.files.Add(new FileEntry("remote-hashrem_ip&int=0", null));
            id++;
        }
        list.Add(new Computer("ICed-PC", "49.992.0.2", randomNodePos(), 3, 1));
        {
            stFold = list[id].files.root.searchForFolder("home");
            stFold.files.Add(new FileEntry("HBRec_127432", Computer.generateBinaryString(256)));
            stFold = list[id].files.root.searchForFolder("log");
            stFold.files.Add(new FileEntry("localhost_deleted_file_tDrill.exe", null));
            stFold.files.Add(new FileEntry("localhost_deleted_file_hashrem.exe", null));
            stFold.files.Add(new FileEntry("tDrill_remote-->B1-->DB", null));
            stFold.files.Add(new FileEntry("hashrem_ip&int=0-->B1", null));
            id++;
        }
        #endregion

        #region draco-missions
        //index 26 -- trace for database
        StartCoroutine(genLAN0(this));

        //index ?? -- delete contents of a database
        StartCoroutine(genLAN1(this));

        //index ?? -- power management server
        StartCoroutine(genLAN2(this));

        //index ?? -- lambo heist DDS
        StartCoroutine(genLAN3(this));

        //index ?? -- fnet regional power denial
        StartCoroutine(genLAN4(this));

        #endregion

        #region final-mission
        //index ?? -- final mission!
        StartCoroutine(finalMissionGen(this));
        #endregion
        #endregion

        return list;
    }

    IEnumerator genLAN0(NetworkMap n)
    {
        int id = 0;
        List<Computer> list = new List<Computer>();
        LocalAreaNetwork drLAN0 = new LocalAreaNetwork(nameGen.generateCompanyName()[0], "49.992.0.2", n, id, 5);
        list.AddRange(drLAN0.getLAN());
        id = drLAN0.getLAN().Count - 1;
        Folder stFold = list[id].getFolder("home");
        stFold.files.Add(new FileEntry("0x0C0.hex", Computer.generateBinaryString(512)));
        nodes.AddRange(list);
        yield return 0;
    }

    IEnumerator genLAN1(NetworkMap n)
    {
        yield return new WaitForSeconds(1f);
        int id = 0;
        List<Computer> list = new List<Computer>();
        LocalAreaNetwork drLAN1 = new LocalAreaNetwork("Nyrex EC", "86.222.363.43", n, id, 5);
        list.AddRange(drLAN1.getLAN());
        id = drLAN1.getLAN().Count - 1;
        list[id].files.root.folders.Add(new Folder("db", list[id].files.root));
        Folder stFold = list[id].files.root.searchForFolder("db");
        int drTemp1 = 999;
        for (int dR_m3 = 0; dR_m3 < 30; dR_m3++)
            stFold.files.Add(new FileEntry("dbEnt_" + drTemp1++, null));
        nodes.AddRange(list);
        yield return 0;
    }

    IEnumerator genLAN2(NetworkMap n)
    {
        yield return new WaitForSeconds(2f);
        int id = 0;
        List<Computer> list = new List<Computer>();
        LocalAreaNetwork drLAN2 = new LocalAreaNetwork("SecRON", "122.377.8.220", n, id, 3);
        list.AddRange(drLAN2.getLAN());
        id = drLAN2.getLAN().Count - 1;
        list[id].name = "SecRON Power Management Server";
        nodes.AddRange(list);
        yield return null;
    }

    IEnumerator genLAN3(NetworkMap n)
    {
        yield return new WaitForSeconds(3f);
        int id = 0;
        List<Computer> list = new List<Computer>();
        LocalAreaNetwork drLAN3 = new LocalAreaNetwork("Exotica", "63.98.263.777", n, id, 8);
        list.AddRange(drLAN3.getLAN());
        id = drLAN3.getLAN().Count - 1;
        list[id].name = "Exotica Digital Distribution Service";
        list[id].files.root.folders.Add(new Folder("dds", list[id].files.root));
        Folder stFold = list[id].files.root.searchForFolder("dds");
        for (int dR_m5 = 0; dR_m5 < 20; dR_m5++)
        {
            if (dR_m5 == 8)
                stFold.files.Add(new FileEntry("Lamborghini LP-770-4 // John Ross", null));
            else
            {
                stFold.files.Add(new FileEntry(
                    "Lamborghini LP-" + random.Next(670, 770) + "-" + random.Next(1, 9)
                    + " // " + peopleNames[random.Next(peopleNames.Length - 1)], null));
            }
        }
        nodes.AddRange(list);
        yield return null;
    }

    IEnumerator genLAN4(NetworkMap n)
    {
        yield return new WaitForSeconds(4f);
        int id = 0;
        List<Computer> list = new List<Computer>();
        LocalAreaNetwork drLAN4 = new LocalAreaNetwork("fnet/SEC-4 PDS", "129.88.0.93", n, id, 7);
        list.AddRange(drLAN4.getLAN());
        id = drLAN4.getLAN().Count - 1;
        list[id].name = "fnet/SEC-4 PDS Relay";
        Folder stFold = list[id].getFolder("sys");
        stFold.files.Add(new FileEntry("important.txt", "WARNING:\nDo not delete any files in here!\n\nIf you do, then the entirety of Sector 4 will lose power...and you will lose your life."));
        nodes.AddRange(list);
        yield return null;
    }

    //thread for generating the final mission
    IEnumerator finalMissionGen(NetworkMap n)
    {
        yield return new WaitForSeconds(5f);
        int id = 0;
        LocalAreaNetwork finMis = new LocalAreaNetwork("MoI/OCT Restricted", "229.992.32.888", n, id, 8);
        List<Computer> list = new List<Computer>();
        list.AddRange(finMis.getLAN());
        id = finMis.getLAN().Count - 1;
        list[id].files.root.folders.Add(new Folder("classified", list[id].files.root));
        list[id].getFolder("classified").folders.Add(new Folder("level-1", list[id].getFolder("classified")));
        list[id].getFolder("classified").folders.Add(new Folder("level-2", list[id].getFolder("classified")));
        list[id].getFolder("classified").folders.Add(new Folder("level-3", list[id].getFolder("classified")));
        Folder stFold = list[id].getFolder("classified").searchForFolder("level-1");
        for (int f1 = 0; f1 < 15; f1++)
        {
            if (f1 == 5)
                stFold.files.Add(new FileEntry("dossier/Michael Hastings",
                    "--------------------------------\n" +
                    "CLASSIFIED / LEVEL 1 / EYES ONLY\n" +
                    "--------------------------------\n" +
                    "Name: Michael Hastings\n" +
                    "DoB : July 14, 1999\n" +
                    "Res : 27389462\n" +
                    "Data:\n" +
                    "--User is a gaming enthusiast. Unclear how target got onto fnet. User has poked " +
                    "around on the higher-sec sectors, glitched-up some interesting data on local groups.\n\n" +
                    "Threat: 14/100 -- Low\n" +
                    "Notes: Keep an eye on this user, as he is quite capable of breaking and glitching things."
                    ));
            else
            {
                string nTemp = peopleNames[random.Next(peopleNames.Length - 1)];
                int tTemp = random.Next(33);
                stFold.files.Add(new FileEntry("dossier/" + nTemp,
                    "--------------------------------\n" +
                    "CLASSIFIED / LEVEL 1 / EYES ONLY\n" +
                    "--------------------------------\n" +
                    "Name: " + nTemp + "\n" +
                    "DoB : " + Computer.generateBinaryString(16) + "\n" +
                    "Res : " + random.Next(10000000, 99999999) + "\n" +
                    "Data:\n" + Computer.generateBinaryString(random.Next(128, 512)) + "\n" +
                    "Threat: " + tTemp + "/100 -- Low" +
                    "Notes: " + Computer.generateBinaryString(random.Next(32, 128))
                    ));
            }
        }
        stFold = list[id].getFolder("classified").searchForFolder("level-2");
        for (int f2 = 0; f2 < 15; f2++)
        {
            if (f2 == 9)
                stFold.files.Add(new FileEntry("dossier/Ryan Hostetter",
                    "--------------------------------\n" +
                    "CLASSIFIED / LEVEL 2 / EYES ONLY\n" +
                    "--------------------------------\n" +
                    "Name: Ryan Hostetter\n" +
                    "DoB : March 9, 2000\n" +
                    "Res : 75302883\n" +
                    "Data:\n" +
                    "--User is a master slacker. Unclear how target got onto fnet. User tried " +
                    "to circumvent Sector 3 security, and evaded a monitor capture.\n\n" +
                    "Threat: 48/100 -- Medium\n" +
                    "Notes: Overwatch protocol applies to user, as he has demonstrated technological prowess."
                    ));
            else
            {
                string nTemp = peopleNames[random.Next(peopleNames.Length - 1)];
                int tTemp = random.Next(66);
                string tTemp2 = null;
                if (tTemp < 33) tTemp2 = "Low"; else tTemp2 = "Medium";
                stFold.files.Add(new FileEntry("dossier/" + nTemp,
                    "--------------------------------\n" +
                    "CLASSIFIED / LEVEL 2 / EYES ONLY\n" +
                    "--------------------------------\n" +
                    "Name: " + nTemp + "\n" +
                    "DoB : " + Computer.generateBinaryString(16) + "\n" +
                    "Res : " + random.Next(10000000, 99999999) + "\n" +
                    "Data:\n" + Computer.generateBinaryString(random.Next(128, 512)) + "\n" +
                    "Threat: " + tTemp + "/100 -- " + tTemp2 +
                    "Notes: " + Computer.generateBinaryString(random.Next(32, 128))
                    ));
            }
        }
        stFold = list[id].getFolder("classified").searchForFolder("level-3");
        for (int f3 = 0; f3 < 15; f3++)
        {
            if (f3 == 9)
                stFold.files.Add(new FileEntry("dossier/Mia Yakusaki",
                    "--------------------------------\n" +
                    "CLASSIFIED / LEVEL 3 / EYES ONLY\n" +
                    "--------------------------------\n" +
                    "Name: Mia Yakusaki\n" +
                    "DoB : April 8, 1997\n" +
                    "Res : 12049263\n" +
                    "Data:\n" +
                    "--User is extremely adept at security and the breaking of it. Unclear how target got onto fnet. User is " +
                    "leader of local underground group 'DraCO' -- few details exist on this group and user.\n\n" +
                    "Threat: 98/100 -- Extreme\n" +
                    "Notes: Whereabouts and current activity unknown -- keep an eye on all monitor triggers."
                    ));
            else
            {
                string nTemp = peopleNames[random.Next(peopleNames.Length - 1)];
                int tTemp = random.Next(34, 80);
                string tTemp2 = null;
                if (tTemp < 66) tTemp2 = "Medium"; else tTemp2 = "High";
                stFold.files.Add(new FileEntry("dossier/" + nTemp,
                    "--------------------------------\n" +
                    "CLASSIFIED / LEVEL 3 / EYES ONLY\n" +
                    "--------------------------------\n" +
                    "Name: " + nTemp + "\n" +
                    "DoB : " + Computer.generateBinaryString(16) + "\n" +
                    "Res : " + random.Next(10000000, 99999999) + "\n" +
                    "Data:\n" + Computer.generateBinaryString(random.Next(128, 512)) + "\n" +
                    "Threat: " + tTemp + "/100 -- " + tTemp2 +
                    "Notes: " + Computer.generateBinaryString(random.Next(32, 128))
                    ));
            }
        }
        nodes.AddRange(list);
        yield return null;
    }

    public void discoverNode(Computer c)
    {
        if (!visibleNodes.Contains(nodes.IndexOf(c)))
            visibleNodes.Add(nodes.IndexOf(c));
        lastAddedNode = c;
    }

    public void discoverNode(string cName)
    {
        for (var index = 0; index < nodes.Count; ++index)
        {
            if (nodes[index].name.Equals(cName))
            {
                discoverNode(nodes[index]);
                break;
            }
        }
    }

    public Vector2 getRandomPosition()
    {
        for (var index = 0; index < 50; ++index)
        {
            var location = generatePos();
            if (!collides(location, -1f))
                return location;
        }
        Console.WriteLine("TOO MANY COLLISIONS");
        return generatePos();
    }

    private Vector2 generatePos()
    {
        var num = NODE_SIZE;
        return new Vector2((float)random.NextDouble(), (float)random.NextDouble());
    }

    public bool collides(Vector2 location, float minSeperation = -1f)
    {
        if (nodes == null)
            return false;
        var num = 0.075f;
        if (minSeperation > 0.0)
            num = minSeperation;
        for (var index = 0; index < nodes.Count; ++index)
        {
            if (Vector2.Distance(location, nodes[index].location) <= (double)num)
                return true;
        }
        return false;
    }

    public void drawNodes() //draw all nodes --> buttons are spawned at each point within the netmapPanel bounds and assigned a nodeCircle texture
    {
        GameObject nodeTDFab = GameObject.Find("nodeBtn");
        GameObject nodeInit;
        RectTransform rectTransform;
        Image nodeImage;
        Button nodeButton;
        int nIndex = 0;
        for(int i = 0; i < nodes.Count; ++i) //draw each node
        {
            if (nodes[i].isNodeVisible && !nodes[i].isNodeDrawn)
            {
                nodeInit = Instantiate(nodeTDFab, Vector3.zero, Quaternion.identity);    //create a clone of the reference button for node

                rectTransform = nodeInit.GetComponent<RectTransform>();                  //get the transform component of the button we just instantiated
                rectTransform.SetParent(netmapPanel.transform);                             //set netmap panel as parent transform
                rectTransform.transform.localScale = new Vector3(1, 1, 1);
                rectTransform.transform.localPosition = nodes[i].location;                  //set location relative to netmap panel in UI

                nodeImage = nodeInit.GetComponent<Image>();                              //get the image component of button
                nodeImage.sprite = nodeCircle;

                nodeButton = nodeInit.GetComponent<Button>();
                nodeButton.GetComponent<Image>().sprite = nodeCircle;

                nodeButton.GetComponentInChildren<Text>().text = "";

                nodes[i].isNodeDrawn = true;
                nodes[i].nodeObject = nodeInit;
            }
        }

        //draw player node
            nIndex = nodeDraw.Count;
            nodeDraw.Add(Instantiate(nodeTDFab, Vector3.zero, Quaternion.identity));
            rectTransform = nodeDraw[nIndex].GetComponent<RectTransform>();
            rectTransform.SetParent(netmapPanel.transform);
            rectTransform.transform.localScale = new Vector3(1, 1, 1);
            rectTransform.transform.localPosition = randomNodePos();
            nodeImage = nodeDraw[nIndex].GetComponent<Image>();
            nodeImage.sprite = homeNodeCircle;
            nodeButton = nodeDraw[nIndex].GetComponent<Button>();
            nodeButton.GetComponent<Image>().sprite = homeNodeCircle;
            nodeButton.GetComponentInChildren<Text>().text = "";
    }

    /*public void drawNodeConnection() //draw node connection when player is connected to a node
    {
        //one day
    }*/

    public void changeNodeCircle(Computer comp, string chType)    //used to change the connected node to admin (white), target/non-admin (red), or disconnected (blue)
    {
        switch(chType)
        {
            case "admin": //change to admin
                comp.nodeObject.GetComponent<Image>().sprite = null;
                comp.nodeObject.GetComponent<Button>().GetComponent<Image>().sprite = null;
                comp.nodeObject.GetComponent<Image>().sprite = adminNodeCircle;
                comp.nodeObject.GetComponent<Button>().GetComponent<Image>().sprite = adminNodeCircle;
                break;

            case "target": //change to target/non-admin
                comp.nodeObject.GetComponent<Image>().sprite = null;
                comp.nodeObject.GetComponent<Button>().GetComponent<Image>().sprite = null;
                comp.nodeObject.GetComponent<Image>().sprite = targetNodeCircle;
                comp.nodeObject.GetComponent<Button>().GetComponent<Image>().sprite = targetNodeCircle;
                break;

            case "disconnect": //change to disconnected
                comp.nodeObject.GetComponent<Image>().sprite = null;
                comp.nodeObject.GetComponent<Button>().GetComponent<Image>().sprite = null;
                comp.nodeObject.GetComponent<Image>().sprite = nodeCircle;
                comp.nodeObject.GetComponent<Button>().GetComponent<Image>().sprite = nodeCircle;
                break;
        }
    }

    public void updateDrawNodes()
    {
        GameObject nodeTDFab = GameObject.Find("nodeBtn");
        GameObject nodeInit;
        RectTransform rectTransform;
        Image nodeImage;
        Button nodeButton;
        for (int i = 0; i < nodes.Count; ++i) //draw each node
        {
            if (nodes[i].isNodeVisible && !nodes[i].isNodeDrawn)
            {
                nodeInit = MonoBehaviour.Instantiate(nodeTDFab, Vector3.zero, Quaternion.identity);       //create a clone of the reference button for node

                rectTransform = nodeInit.GetComponent<RectTransform>();                  //get the transform component of the button we just instantiated
                rectTransform.SetParent(netmapPanel.transform);                             //set netmap panel as parent transform
                rectTransform.transform.localScale = new Vector3(1, 1, 1);
                rectTransform.transform.localPosition = nodes[i].location;                  //set location relative to netmap panel in UI

                nodeImage = nodeInit.GetComponent<Image>();                              //get the image component of button
                nodeImage.sprite = nodeCircle;

                nodeButton = nodeInit.GetComponent<Button>();
                nodeButton.GetComponent<Image>().sprite = nodeCircle;

                nodeButton.GetComponentInChildren<Text>().text = "";

                nodes[i].nodeObject = nodeInit;
            }
        }
    }

    public Vector2 randomNodePos() //get the node draw position
    {
        Vector2 nodeLocation;
        nodeLocation.x = UnityEngine.Random.Range(rtMinX, rtMaxX);
        nodeLocation.y = UnityEngine.Random.Range(rtMinY, rtMaxY);

        return nodeLocation;
    }

    public static string generateRandomIP() //generate a random IP value 'XXX.XXX.XXX.XXX'
    {
        return Utils.random.Next(byte.MaxValue) + "." + Utils.random.Next(byte.MaxValue) + "." +
               Utils.random.Next(byte.MaxValue) + "." + Utils.random.Next(byte.MaxValue);
    }

    public void drawLine(Vector2 origin, Vector2 dest) //draw a line between two points
    {
        GameObject lineObj = new GameObject();
        lineObj.transform.position = new Vector3(0, 0, 0);
        lineObj.name = "NodeLineRender";
        lineObj.AddComponent<LineRenderer>();
        LineRenderer lineObjRend = lineObj.GetComponent<LineRenderer>();
        lineObjRend.SetPosition(0, origin);
        lineObjRend.SetPosition(1, dest);
    }
}