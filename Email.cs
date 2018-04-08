using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class Email {

    public string Subject, Sender, IP, Body;
    public bool isRead;
    public EmailListItemController ctrl;

    public Email(string subject, string sender, string ip, string body)
    {
        Subject = subject;
        Sender = sender;
        IP = ip;
        Body = body;
        isRead = false;
        ctrl = null;
    }
}