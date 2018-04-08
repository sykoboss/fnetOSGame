using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help : MonoBehaviour {

    private static string helpText = "";

    public static string getHelp(string cmd)
    {
        switch (cmd)
        {
            case "help":
                helpText =
                    "\nHelp Dialog:\n" +
                    "--mv <file> <folder> | moves file to specified folder\n\n" +
                    "--cp <file> <folder> | copies file to specified folder\n\n" +
                    "--rm <file|folder>   | deletes specified file or folder\n\n" +
                    "--cd <folder|..>     | move into folder, or out of current folder\n\n" +
                    "--ls <folder|file>   | list files in current or specified folder\n" +
                    "                     | or view contents of file\n\n" + 
                    "--cat <file>         | displays contents of file in catView window\n";
                break;
            case "mv":
                helpText =
                    "\n--mv <file> <folder>\n\n" +
                    "--Moves specified file into the specified folder.\n" +
                    "If .. specified as folder, file will be moved out of current folder.\n\n";
                break;
            case "cp":
                helpText =
                    "\n--cp <file> <folder>\n\n" +
                    "--Copies specified file to the specified folder.\n" +
                    "If .. specified as folder, file will be copied out of current folder.\n\n";
                break;
            case "rm":
                helpText =
                    "\n--rm <file|folder>\n\n" +
                    "--Deletes the specified file or folder.\n" +
                    "If * specified as arg, contents of current folder will be deleted.\n\n";
                break;
            case "cd":
                helpText =
                    "\n--cd <folder|..>\n\n" +
                    "--Use to navigate into specified folder or out of current folder.\n" +
                    "Specify .. as arg to navigate out of current folder.\n\n";
                break;
            case "ls":
                helpText =
                    "\n--ls <folder|file>\n\n" +
                    "--Lists files in current folder if used alone, in specified" +
                    "folder, or the contents of specified file.\n\n";
                break;
            case "cat":
                helpText =
                    "\n--cat <file>\n\n" +
                    "--Displays the contents of file in current folder to the catView window.\n\n";
                break;
            case "upload":
                helpText =
                    "\n--upload <-p|-s|-d> <file>\n\n" +
                    "--Uploads the file to the respective fnet organization.\n\n";
                break;
        }

        return helpText;
    }
}
