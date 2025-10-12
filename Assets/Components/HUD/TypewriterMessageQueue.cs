using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypewriterMessageQueue : MonoBehaviour
{
    [Header("References")]
    public GameObject messagePrefab;
    public Transform messageParent;
    
    [Header("Timing")]
    public float charDelay = 0.09f;
    public float messageLifetime = 15f;

    private Queue<string> messageQueue = new Queue<string>();
    private bool isPrinting = false;
    public void EnqueueMessage(string text)
    {
        messageQueue.Enqueue(text);
        if (!isPrinting)
            StartCoroutine(ProcessQueue());
    }
    
    private IEnumerator ProcessQueue()
    {
        isPrinting = true;

        while (messageQueue.Count > 0)
        {
            string nextMessage = messageQueue.Dequeue();
            yield return StartCoroutine(PrintMessage(nextMessage));
        }

        isPrinting = false;
    }
    
    private IEnumerator PrintMessage(string text)
    {
        GameObject msgObj = Instantiate(messagePrefab, messageParent);
        TextMeshProUGUI tmp = msgObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp == null)
        {
            Debug.LogError("Message prefab must contain a TextMeshProUGUI component!");
            yield break;
        }

        tmp.text = "";
        foreach (char c in text)
        {
            tmp.text += c;
            yield return new WaitForSeconds(charDelay);
        }
        yield return new WaitForSeconds(3);
        StartCoroutine(AutoDestroy(msgObj));
        yield return null;
    }
    
    private IEnumerator AutoDestroy(GameObject obj)
    {
        yield return new WaitForSeconds(messageLifetime);
        if (obj != null)
            Destroy(obj);
    }
}
