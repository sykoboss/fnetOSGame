using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;
using System;

[XmlRoot("root"), XmlType("Emails")]
public class EmailParser : MonoBehaviour
{
    static XDocument xmlDoc;
    static List<XElement> hg1Items;
    static List<XElement> hg2Items;
    static List<XElement> hg3Items;

    public static List<Email> ProtocolEmails = new List<Email>();
    
    public static List<Email> SPECEmails = new List<Email>();
    
    public static List<Email> DraCOEmails = new List<Email>();

    public static TextAsset xmlDef;

    public static void init()
    {
        TextAsset xmlText = Resources.Load("emails") as TextAsset;
        xmlDoc = XDocument.Parse(xmlText.text);
        XElement rootdir = xmlDoc.Root.Element("Emails");

        hg1Items = rootdir.Element("ProtocolEmails").Descendants("Email").ToList();
        hg2Items = rootdir.Element("SPECEmails").Descendants("Email").ToList();
        hg3Items = rootdir.Element("DraCOEmails").Descendants("Email").ToList();

        string tsubject, tsender, tip, tbody;
        foreach(XElement xe in hg1Items)
        {
            tsubject = xe.Element("subject").Value;
            tsender = xe.Element("sender").Value;
            tip = xe.Element("ip").Value;
            tbody = xe.Element("body").Value.Replace("\\", "\n");
            ProtocolEmails.Add(new Email(tsubject, tsender, tip, tbody));
        }
        foreach (XElement xe in hg2Items)
        {
            tsubject = xe.Element("subject").Value;
            tsender = xe.Element("sender").Value;
            tip = xe.Element("ip").Value;
            tbody = xe.Element("body").Value.Replace("\n", "\n");
            SPECEmails.Add(new Email(tsubject, tsender, tip, tbody));
        }
        foreach (XElement xe in hg3Items)
        {
            tsubject = xe.Element("subject").Value;
            tsender = xe.Element("sender").Value;
            tip = xe.Element("ip").Value;
            tbody = xe.Element("body").Value.Replace("\n", "\n");
            DraCOEmails.Add(new Email(tsubject, tsender, tip, tbody));
        }
    }
}