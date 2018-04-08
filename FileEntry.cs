using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileEntry {

    public static List<string> filenames;
    public static List<string> fileData;
    public string fname;
    public string fdata;
    public int secondCreatedAt;
    public int size;
    private System.Random random = new System.Random();

    public FileEntry()
    {
        var index = random.Next(0, filenames.Count - 1);
        fname = filenames[index];
        fdata = fileData[index];
        size = fdata.Length * 8;
        secondCreatedAt = (int)Time.time;
    }
    public FileEntry(string nameEntry, string dataEntry)
    {
        nameEntry = nameEntry.Replace(" ", "_");
        fname = nameEntry;
        fdata = dataEntry;
        if (fdata != null)
            size = fdata.Length * 8;
        else
            size = 0;
        secondCreatedAt = (int)Time.time;
    }

    public string getName()
    {
        return fname;
    }
}
