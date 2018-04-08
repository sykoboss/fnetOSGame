using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileSystem {

    public Folder root;
    
    public FileSystem(bool empty) { }

    public FileSystem()
    {
        root = new Folder("root", new Folder(".", null));

        root.folders.Add(new Folder("home", root));
        root.folders.Add(new Folder("log", root));
        root.folders.Add(new Folder("bin", root));
        root.folders.Add(new Folder("sys", root));
        genSysFiles();
    }

    public void genSysFiles()
    {
        var folder = root.searchForFolder("sys");
        folder.files.Add(new FileEntry("os-config.sys", Computer.generateBinaryString(256)));
        folder.files.Add(new FileEntry("boot.dll", Computer.generateBinaryString(128)));
        folder.files.Add(new FileEntry("kernel", Computer.generateBinaryString(128)));
        folder.files.Add(new FileEntry("fnet-cfg.dll", Computer.generateBinaryString(256)));
    }
}
