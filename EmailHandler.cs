using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class EmailHandler {

    static Text subjectLine, senderLine, bodyText;

    public static void init()
    {
        subjectLine = GameObject.Find("subjectDisText").GetComponent<Text>();
        senderLine = GameObject.Find("senderDisText").GetComponent<Text>();
        bodyText = GameObject.Find("bodyDisText").GetComponent<Text>();
    }

    public static void displayEmail(Email e)
    {
        e.isRead = true;
        e.ctrl.SubjectText.fontStyle = FontStyle.Normal;
        subjectLine.text = e.Subject;
        senderLine.text = "" + e.Sender + " <" + e.IP + ">";
        bodyText.text = e.Body;
    }
}