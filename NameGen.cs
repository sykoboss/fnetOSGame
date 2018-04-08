using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameGen {

    public static List<string> main;
    public static List<string> postfix;
    public static List<string> nT;
    private static System.Random random = new System.Random();

	public void initNGen()
    {
        main = new List<string>();
        main.Add("Alutech");
        main.Add("RMS");
        main.Add("Shuangsen");
        main.Add("Popgard");
        main.Add("Panther");
        main.Add("Rygo");
        main.Add("Geotech");
        main.Add("Trigon");
        main.Add("Polyhedron");
        main.Add("ICS");
        main.Add("Seton");
        main.Add("Yorelon");
        main.Add("Chasm");
        main.Add("Phenom");
        main.Add("Kibur");
        main.Add("Reimann");
        main.Add("Monocube");
        main.Add("Audiale");
        main.Add("Exec");
        main.Add("Dyschip");

        postfix = new List<string>();
        postfix.Add(" Inc");
        postfix.Add(" Interactive");
        postfix.Add(" LLC");
        postfix.Add(" Internal");
        postfix.Add(" Software");
        postfix.Add(" Technologies");
        postfix.Add(" Connections");
        postfix.Add(" Solutions");
        postfix.Add(" Enterprises");
        postfix.Add(" Studios");
        postfix.Add(" Consortium");
        postfix.Add(" Communications");

        nT = new List<string>();
        nT.Add("Query Node");
        nT.Add("Uplink");
        nT.Add("Node Server");
        nT.Add("Data Server");
        nT.Add("Proxy");
        nT.Add("Domain Server");
        nT.Add("Comms Server");
        nT.Add("Quantum Link");
    }
	
	public string generateName()
    {
        return "" + main[random.Next(0, main.Count)] + postfix[random.Next(0, postfix.Count)];
    }



    public string[] generateCompanyName()
    {
        return new string[2]
        {
                main[random.Next(0, main.Count)],
                postfix[random.Next(0, postfix.Count)]
        };
    }

    public string generateType()
    {
        return "" + nT[random.Next(0, nT.Count)];
    }

    public string getRandomMain()
    {
        return main[random.Next(0, main.Count)];
    }
}
