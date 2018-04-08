using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmailListController : MonoBehaviour {

    public GameObject EmailItemPanel;
    public GameObject EmailItemPrefab;

    public static Terminal term;

    private int eRCounter;

    List<Email> emails;

	void Start()
    {
        term = GameObject.Find("termHandler").GetComponent<Terminal>();
        eRCounter = 0;
        EmailHandler.init();
        emails = new List<Email>();
        emails.Add(
            new Email("Welcome to the fnet!", //subject
                "E", //sender
                null,//IP address (if applicable)
                //below is body of email
                "Hi there!\n\nThis is the fmail system used on the fnet, for all sorts of communication. If you haven't already, please see the readme I left on your computer!\n\n-E")
        );

        for(int i = 0; i < emails.Count; i++) //only one to init, but that's okay
        {
            //create temporary variable for use in the AddListener delegate (avoid pointer)
            Email e = emails[i];
            //create new email item
            GameObject listEmail = Instantiate(EmailItemPrefab) as GameObject;
            e.ctrl = listEmail.GetComponent<EmailListItemController>();
            //set the text of the item to the email's subject
            e.ctrl.SubjectText.text = e.Subject;
            if(!e.isRead) //set bold if unread
                e.ctrl.SubjectText.fontStyle = FontStyle.Bold;
            //add an event listener so that the email contents are displayed upon selection
            e.ctrl.eButton = listEmail.GetComponentInChildren<Button>();
            e.ctrl.eButton.onClick.AddListener(delegate { EmailHandler.displayEmail(e); });
            //properly place the item on the UI
            listEmail.transform.SetParent(EmailItemPanel.transform, false);
            listEmail.transform.localScale = Vector3.one;
        }
	}

    void Update()
    {
        //check if there are any unread emails
        foreach (Email e in emails)
        {
            if (!e.isRead)
                eRCounter++;
        }
        if (eRCounter > 0)
            term.updateEmailGraphic(true);
        else
            term.updateEmailGraphic(false);
        eRCounter = 0;
    }

    public void addNewEmail(Email em) //function for adding an email to be displayed
    {
        emails.Add(em);  //add email to the list
        int i = emails.Count - 1;
        Email e = emails[i];
        GameObject listEmail = Instantiate(EmailItemPrefab) as GameObject;
        e.ctrl = listEmail.GetComponent<EmailListItemController>();
        e.ctrl.SubjectText.text = e.Subject;
        if (!e.isRead) //set bold if unread
            e.ctrl.SubjectText.fontStyle = FontStyle.Bold;
        e.ctrl.eButton = listEmail.GetComponentInChildren<Button>();
        e.ctrl.eButton.onClick.AddListener(delegate { EmailHandler.displayEmail(e); });
        listEmail.transform.SetParent(EmailItemPanel.transform, false);
        listEmail.transform.localScale = Vector3.one;
    }
}