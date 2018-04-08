using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missions {

    public List<string> protocolMissionDetails;
    public List<string> specMissionDetails;
    public List<string> dracoMissionDetails;
	
    public void initMissions() //for use at start of game
    {
        protocolMissionDetails = protocolDescs();
        specMissionDetails = specDescs();
        dracoMissionDetails = dracoDescs();
    }

    public List<string> protocolDescs()
    {
        List<string> list = new List<string>();

        //mission 1
        list.Add("Protocol -- Uncover and provide the IP address that stole files from a local business, IP 704.22.288.10");
        //mission 2
        list.Add("Protocol -- Shut down the malicious hacker located at 122.93.29.304");
        //mission 3
        list.Add("Protocol -- Delete a stolen database from a hacker located at 93.992.84.7");
        //mission 4
        list.Add("Protocol -- Locate and shut down a malicious hacker that stole an important data file from 104.2.99.338 and other associated IPs");
        //final mission -- FDB code #1
        list.Add("Protocol -- Find out where an ex-SPEC member ran off to using the provided leads");

        return list;
    }

    public List<string> specDescs()
    {
        List<string> list = new List<string>();

        //mission 1
        list.Add("SPEC -- Modify the database record containing 'John Silva' to display 'Deceased' instead of 'Alive'");
        //mission 2
        list.Add("SPEC -- Trace and shut down the computer at the front-end of a botnet. IP of one botnet computer: 99.229.654.21");
        //mission 3 -- FDB code #2
        list.Add("SPEC -- Prevent a hacker from getting into the SPEC core database");
        //mission 4
        list.Add("SPEC -- Uncover and provide the IP address that stole bank account information from the bank located at 100.99.388.929");
        //final mission
        list.Add("SPEC -- Upload the bank account information to the SPEC File Drop Server");

        return list;
    }

    public List<string> dracoDescs()
    {
        List<string> list = new List<string>();

        //mission 1 -- pre-cursor to DraCO -- FDB code #3
        list.Add("Yaku -- Go through a network located at 88.938.27.71 and locate the database. Stay on the lookout for info of interest");
        //mission 2 -- indoctrination to DraCO
        list.Add("DraCO -- Locate and upload a file named '0x0C0.hex'");
		//mission 3 -- delete an entire database
		list.Add("DraCO -- Locate and delete the entire contents of a database on the local-area network at 86.222.363.43");
		//misison 4 -- shut down a business office
		list.Add("DraCO -- Shut down a power management server for SecRON Enterprises, network: 122.377.8.220");
		//mission 5 -- perform a digital heist of an exotic car
		list.Add("DraCO -- Perform a digital heist of a Lamborghini LP-770-4, from the DDS at 63.98.263.777");
		//mission 6 -- shut down a regional power grid
		list.Add("DraCO -- Shut down the power grid relay at 129.88.0.93");
		//final mission
		list.Add("DraCO -- Find the top-secret MoI IP address on one of their LANs: gateway 229.992.32.888");
		
		//post-final mission (to obtain code #6)
		list.Add("??? -- investigate the IP of the mysterious email sender");

        return list;
    }
}
