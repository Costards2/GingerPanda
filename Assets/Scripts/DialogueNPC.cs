using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Serialization;
using System.Security.Cryptography;

public class DialogueNPC : MonoBehaviour
{
    [Header("Dialogue Mission Imcomplete")]
    public string[] dialogueNpc;
    public int dialogueIndex;

    [Header("NPC Name")]

    public string nameNpcWrite;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameNpc;
    public Image imageNpc;
    public GameObject exclamationMark;

    [Header("Bools")]
    public bool readyToTalk;
    public bool startDialogue;
    public bool firstTalk = true;
    public bool dontTalkAgaing = false;

    void Update()
    {
        //Codigo para que o player tenha que apertar para interagir 
        //if (Input.GetKey(KeyCode.E))
        //{
        //    //if (!startDialogue && (FindObjectOfType<MoveFSM>().IsGrounded() == true) && readyToTalk)
        //    //{
        //    //    FindObjectOfType<MoveFSM>().doNothing = true;
        //    //    firstTalk = false;
        //    //    StartDialogue();
        //    //}
        //}

        if (startDialogue)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (dialogueText.text != dialogueNpc[dialogueIndex])
                {
                    StopAllCoroutines();
                    dialogueText.text = dialogueNpc[dialogueIndex];
                }
                else if (dialogueIndex < dialogueNpc.Length - 1)
                {
                    NextDialogue();
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    void StartDialogue()
    {
        startDialogue = true;
        nameNpc.text = "" + nameNpcWrite;
        dialogueIndex = 0;
        dialoguePanel.SetActive(true);
        exclamationMark.SetActive(false);
        StartCoroutine(ShowDialogue());
    }

    void NextDialogue()
    {
        dialogueIndex = dialogueIndex + 1;

        if (dialogueIndex < dialogueNpc.Length)
        {
            StartCoroutine(ShowDialogue());
        }
    }

    void EndDialogue()
    {
        readyToTalk = false;
        dialoguePanel.SetActive(false);
        startDialogue = false;
        dialogueIndex = 0;
        FindObjectOfType<MoveFSM>().doNothing = false;
    }


    IEnumerator ShowDialogue()
    {
        dialogueText.text = "";

        foreach (char letter in dialogueNpc[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            readyToTalk = true;
        }
        else
        {
            readyToTalk = false;
        }

        if (!startDialogue && (FindObjectOfType<MoveFSM>().IsGrounded() == true) && readyToTalk && !dontTalkAgaing)
        {
            FindObjectOfType<MoveFSM>().doNothing = true;
            firstTalk = false;
            StartDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dontTalkAgaing = true;
        }
    }
}
