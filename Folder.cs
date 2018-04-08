using System.Collections.Generic;

public class Folder {

    public string fname;
    public List<FileEntry> files;
    public List<Folder> folders;
    public Folder parentFolder;

    public Folder()
    {
        fname = null;
        files = new List<FileEntry>();
        folders = new List<Folder>();
        parentFolder = null;
    }

    public Folder(string folderName, Folder parFol)
    {
        fname = folderName;
        files = new List<FileEntry>();
        folders = new List<Folder>();
        parentFolder = parFol;
    }

    public string getContents()
    {
        string str = "";
        for (var index = 0; index < folders.Count; ++index)
        {
            str += ":" + folders[index].fname + "\n";
        }
        for (var index = 0; index < files.Count; ++index)
        {
            str += "" + files[index].fname + "\n";
        }
        return str;
    }

    public Folder searchForFolder(string folderName)
    {
        Folder fol = null;
        for (var index = 0; index < folders.Count; ++index)
        {
            if (folders[index].fname.Equals(folderName))
            {
                fol = folders[index];
                break;
            }
            else continue;
        }
        return fol;
    }

    public FileEntry searchForFile(string fileName)
    {
        FileEntry file = null;
        for (var index = 0; index < files.Count; ++index)
        {
            if (files[index].fname.Equals(fileName))
            {
                file = files[index];
                break;
            }
            else continue;
        }
        return file;
    }

    public bool doesFileExist(string fileName)
    {
        for (var index = 0; index < files.Count; ++index)
        {
            if (files[index].fname.Equals(fileName))
            {
                return true;
            }
        }
        return false;
    }
}
