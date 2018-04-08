using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocalAreaNetwork {

    private static System.Random r = new System.Random();
    public List<Computer> nodes;
    public string accessIP;
    private string lanIPHeader;
    public int LANIndex;
    public string LANName;
    public NetworkMap netMap;
    private bool isDBGenerated;
    private bool isFirstAdminGen;
    private int numToGen;
    private int numGen;

    private int iterCheck; //for figuring out how many times the system tries to generate the LAN

    private int rCount;

    /* 
     * LAN Formulation -- num determines amount of computers (minimum of 2 besides the access point)
     * 
     * Types of Nodes:
     * --AP (where user comes in)
     * --Locks (switches ability to connect to computers)
     *      --v1 (brute-forceable by the user)
     *      --v2 (requires user to open via admin)
     * --Admin (user can open all lock-v2's from here, but lock-v1's reset)
     * --Regular (general clutter)
     * --Router (connected to two regulars and a lock (or DB) -- only the lock will have something after it)
     * 
     * From a lock v1:
     * --20% chance: router
     * --70% chance: regular
     * --10% chance: admin
     * 
     * From a lock v2:
     * --40% chance: router
     * --60% chance: regular
     * 
     * From a regular:
     * --10% chance: admin
     * --10% chance: lock v1
     * --10% chance: lock v2 (if an admin has been gen'd)
     * 
     */


    public LocalAreaNetwork(string name, string a_IP, NetworkMap nmap, int nIndex, int num)
    {
        iterCheck = 0;
        netMap = nmap;
        isDBGenerated = false;
        isFirstAdminGen = false;
        accessIP = a_IP;
        string[] lIPTemp = accessIP.Split('.');
        lanIPHeader = "" + lIPTemp[0] + "." + lIPTemp[1] + "." + lIPTemp[2] + "."; //first three numbers (e.g. 192.37.44.xxx)
        LANIndex = nIndex;
        LANName = name;
        rCount = 0;
        //nodes[0] is always the access point
        numToGen = num;
        numGen = 0;
        generateLAN();
    }

    public List<Computer> getLAN()
    {
        for(int i = 0; i < nodes.Count; i++)
        {
            nodes[i].cLan = this;
        }
        debugContents(); //debug list of all nodes in LAN
        if (isDBGenerated)
            return nodes;
        else
            return null;
    }

    public Computer getLockv2()
    {
        if (isDBGenerated)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].lanType.Equals("lock-v2"))
                    return nodes[i];
            }
        }
        return null;
    }

    private void generateLAN()
    {
        nodes = new List<Computer>();
        GC.Collect();
        nodes.Add(new Computer(true, "" + LANName + " Access Point", accessIP, netMap.randomNodePos(), r.Next(4), "ap"));
        if (numToGen < 2)
        {
            genDatabase();
            //add LAN links
            nodes[numGen - 1].addLANLinks(nodes[numGen].IP);
            nodes[numGen].addLANLinks(nodes[numGen - 1].IP);
            return;
        }

        nodes.Add(new Computer(true, "LANLock v1", lanIPHeader + r.Next(999), netMap.randomNodePos(), 1, "lock-v1"));
        numGen++;
        nodes[numGen].addLANLinks(nodes[0].IP);
        safeNextGen();
    }

    private void safeNextGen()
    {
        //this catches occasional exceptions and tries again to generate the LAN
        try
        {
            iterCheck++;
            nextGenerate();
        }
        catch (Exception e)
        {
            if(iterCheck > 1000000)
            {
                Debug.Log("LAN: iterCheck is greater than 1 million");
                return;
            }
            isDBGenerated = false;
            isFirstAdminGen = false;
            numGen = 0;
            generateLAN();
        }
    }

    private void nextGenerate() //main recursive function for generating the next computer
    {
        if (numGen != numToGen - 1)
        {
            string t = "" + nodes[numGen].lanType;
            if (t.Equals("lock-v1"))
            {
                Computer c = genAfterLockv1();
                if (c != null)
                {
                    nodes.Add(c);
                    numGen++;
                    //add LAN links
                    nodes[numGen - 1].addLANLinks(nodes[numGen].IP);
                    nodes[numGen].addLANLinks(nodes[numGen - 1].IP);
                }
            }
            else if (t.Equals("regular"))
            {
                Computer c = genAfterRegular();
                if (c != null)
                {
                    nodes.Add(c);
                    numGen++;
                    //add LAN links
                    nodes[numGen - 1].addLANLinks(nodes[numGen].IP);
                    nodes[numGen].addLANLinks(nodes[numGen - 1].IP);
                }
            }
            else if (t.Equals("admin"))
            {
                nodes.Add(genAfterAdmin());
                numGen++;
                //add LAN links
                nodes[numGen - 1].addLANLinks(nodes[numGen].IP);
                nodes[numGen].addLANLinks(nodes[numGen - 1].IP);
            }
            else if (t.Equals("router"))
            {
                nodes.AddRange(genAfterRouter());
            }
            else if (t.Equals("ap")) //in the event it doesn't properly initialize
            {
                nodes = new List<Computer>();
                numGen = 0;
                generateLAN();
            }
            safeNextGen();
        }
        else if (!isDBGenerated)
        {
            if (isFirstAdminGen)
            {
                nodes.Add(new Computer(true, "LANLock v2", lanIPHeader + r.Next(999), netMap.randomNodePos(), 1, "lock-v2"));
                numGen++;
                nodes[numGen - 1].addLANLinks(nodes[numGen].IP);
                nodes[numGen].addLANLinks(nodes[numGen - 1].IP);

                nodes.Add(genDatabase());
                numGen++;
                //add LAN links
                nodes[numGen - 1].addLANLinks(nodes[numGen].IP);
                nodes[numGen].addLANLinks(nodes[numGen - 1].IP);
            }
            else
            {
                nodes.Add(new Computer(
                true, "" + LANName + " Admin", lanIPHeader + numGen, netMap.randomNodePos(), r.Next(3, 5), "admin"));
                numGen++;
                nodes[numGen - 1].addLANLinks(nodes[numGen].IP);
                nodes[numGen].addLANLinks(nodes[numGen - 1].IP);

                nodes.Add(new Computer(true, "LANLock v2", lanIPHeader + r.Next(999), netMap.randomNodePos(), 1, "lock-v2"));
                numGen++;
                nodes[numGen - 1].addLANLinks(nodes[numGen].IP);
                nodes[numGen].addLANLinks(nodes[numGen - 1].IP);

                nodes.Add(genDatabase());
                numGen++;
                //add LAN links
                nodes[numGen - 1].addLANLinks(nodes[numGen].IP);
                nodes[numGen].addLANLinks(nodes[numGen - 1].IP);
            }
        }
        else return; //reached when LAN is completely generated
    }

    //the last node to be generated in the LAN
    private Computer genDatabase()
    {
        Computer c = new Computer(
            true, ""+LANName+" Database", lanIPHeader + numGen, netMap.randomNodePos(), r.Next(3,5), "database");
        isDBGenerated = true;
        return c;
    }

    private Computer genAfterLockv1()
    {
        Computer c = null;
        if(numGen == numToGen - 1) //one node left to generate
        {
            c = genDatabase();
            return c;
        }

        int rand = r.Next(0, 9);
        if(rand < 2)
        {
            if(numGen <= numToGen - 4) //need at least four nodes left to generate
            c = new Computer(
            true, "" + LANName + " Router", lanIPHeader + numGen, netMap.randomNodePos(), r.Next(1, 2), "router");
        }
        else if(rand < 9 && numGen <= numToGen - 2) //need at least two nodes left to generate
        {
            c = new Computer(
            true, "" + LANName + " PC-" + r.Next(100, 999), lanIPHeader + numGen, netMap.randomNodePos(), r.Next(1, 3), "regular");
        }
        else if(numGen <= numToGen - 2)
        {
            c = new Computer(
            true, "" + LANName + " Admin", lanIPHeader + numGen, netMap.randomNodePos(), r.Next(3, 5), "admin");
            if (!isFirstAdminGen) isFirstAdminGen = true;
        }

        return c;
    }

    private Computer genAfterRegular()
    {
        Computer c = null;

        int rand = r.Next(0, 9);
        if (rand > 4 && rand < 8 && numGen <= numToGen - 4)
        {
            c = new Computer(
            true, "" + LANName + " Router", lanIPHeader + numGen, netMap.randomNodePos(), r.Next(1, 2), "router");
        }
        else if (rand > 6 && rand < 8 && numGen <= numToGen - 2)
        {
            c = new Computer(
            true, "" + LANName + " PC-" + r.Next(100, 999), lanIPHeader + numGen, netMap.randomNodePos(), r.Next(1, 3), "regular");
        }
        else if (rand >= 8 && numGen <= numToGen - 2)
        {
            c = new Computer(
            true, "" + LANName + " Admin", lanIPHeader + numGen, netMap.randomNodePos(), r.Next(3, 5), "admin");
            if (!isFirstAdminGen) isFirstAdminGen = true;
        }

        return c;
    }

    private Computer genAfterAdmin()
    {
        Computer c = new Computer(
            true, "" + LANName + " Router", lanIPHeader + numGen, netMap.randomNodePos(), r.Next(1, 2), "router");
        return c;
    }

    private List<Computer> genAfterRouter()
    {
        int i = 0;
        List<Computer> cs = new List<Computer>(3);
        cs.Add(new Computer(
            true, "" + LANName + " PC-" + r.Next(100, 999), lanIPHeader + numGen, netMap.randomNodePos(), r.Next(3, 5), "regular"));
        numGen++;
        i++;
        cs[i - 1].addLANLinks(nodes[numGen - 1].IP);
        nodes[numGen - 1].addLANLinks(cs[i - 1].IP);

        cs.Add(new Computer(
            true, "" + LANName + " PC-" + r.Next(100, 999), lanIPHeader + numGen, netMap.randomNodePos(), r.Next(3, 5), "regular"));
        numGen++;
        i++;
        cs[i - 1].addLANLinks(nodes[numGen - 2].IP);
        nodes[numGen - 2].addLANLinks(cs[i - 1].IP);

        if (numGen <= numToGen - 1)
        {
            cs.Add(new Computer(true, "LANLock v1", lanIPHeader + r.Next(999), netMap.randomNodePos(), 1, "lock-v1"));
            numGen++;
            i++;
            cs[i - 1].addLANLinks(nodes[numGen - 3].IP);
            nodes[numGen - 3].addLANLinks(cs[i - 1].IP);
        } 
        else
        {
            if (isFirstAdminGen)
            {
                cs.Add(new Computer(true, "LANLock v2", lanIPHeader + r.Next(999), netMap.randomNodePos(), 1, "lock-v2"));
                numGen++;
                i++;
                cs[i - 1].addLANLinks(nodes[numGen - 3].IP);
                nodes[numGen - 3].addLANLinks(cs[i - 1].IP);

                cs.Add(genDatabase());
                numGen++;
                i++;
                //add LAN links
                cs[i - 1].addLANLinks(nodes[numGen - 4].IP);
                nodes[numGen - 4].addLANLinks(cs[i - 1].IP);
            }
            else
            {
                cs.Add(new Computer(
                true, "" + LANName + " Admin", lanIPHeader + numGen, netMap.randomNodePos(), r.Next(3, 5), "admin"));
                numGen++;
                i++;
                cs[i - 1].addLANLinks(nodes[numGen - 3].IP);
                nodes[numGen - 3].addLANLinks(cs[i - 1].IP);

                cs.Add(new Computer(true, "LANLock v2", lanIPHeader + r.Next(999), netMap.randomNodePos(), 1, "lock-v2"));
                numGen++;
                i++;
                cs[i - 1].addLANLinks(nodes[numGen - 4].IP);
                nodes[numGen - 4].addLANLinks(cs[i - 1].IP);

                cs.Add(genDatabase());
                numGen++;
                i++;
                //add LAN links
                cs[i - 1].addLANLinks(nodes[numGen - 5].IP);
                nodes[numGen - 5].addLANLinks(cs[i - 1].IP);
            }
        }
        return cs;
    }

    private void debugContents()
    {
        string cons = "[";
        foreach (Computer c in nodes)
        {
            cons += "" + c.lanType + ", ";
        }
        cons += "]";
        Debug.Log("" + LANName + " nodes: " + cons);
    }
}