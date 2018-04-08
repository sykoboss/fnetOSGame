using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PassGen {

    public static List<string> passwords;
    private static TextAsset passFile;
    private static System.Random r;

    public static void init()
    {
        r = new System.Random();
        passwords = new List<string>();
        passFile = Resources.Load<TextAsset>("passwords");  //read passwords.txt

        var pSplit = passFile.text.Split('\t', '\n');   //split the file into tons of individual password strings
        for(int i = 0; i < pSplit.Length; ++i)
            passwords.Add(pSplit[i]);   //add them to the list
    }

    public static string getRandomPass()    //returns a random password
    {
        return passwords[r.Next(passwords.Count)];
    }
}
