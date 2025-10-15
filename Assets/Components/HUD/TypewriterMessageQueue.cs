using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum ConsoleMessageType { SYSTEM, WARNING, RADAR }

public class TypewriterMessageQueue : MonoBehaviour
{
    [Header("References")]
    public GameObject messagePrefab;
    public Transform messageParent;

    [Header("Timing")]
    public float charDelay = 0.09f;
    public float messageLifetime = 15f;
    public float timeBetweenMessages = 1f;
    
    [Header("Variables")]
    private bool isShuttingDown = false;

    private struct QueuedMessage
    {
        public string text;
        public Color color;
        public QueuedMessage(string t, Color c)
        {
            text = t;
            color = c;
        }
    }

    private Queue<QueuedMessage> messageQueue = new Queue<QueuedMessage>();
    private bool isPrinting = false;

    private Color defaultColor;

    void Awake()
    {
        defaultColor = new Color(0.24f, 1f, 0f);
    }

    public void EnqueueMessage(string text, ConsoleMessageType type = ConsoleMessageType.SYSTEM)
    {
        if (isShuttingDown) return;
        Color messageColor = defaultColor;
        switch (type)
        {
            case ConsoleMessageType.SYSTEM: messageColor = defaultColor; break;
            case ConsoleMessageType.WARNING: messageColor = new Color(0.78f, 0f, 0.11f); break;
            case ConsoleMessageType.RADAR: messageColor = new Color(0.66f, 0.66f, 0f); break;
        }

        messageQueue.Enqueue(new QueuedMessage(text, messageColor));

        if (!isPrinting)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isPrinting = true;

        while (messageQueue.Count > 0)
        {
            QueuedMessage next = messageQueue.Dequeue();
            yield return StartCoroutine(PrintMessage(next.text, next.color));
        }

        isPrinting = false;
    }

    private IEnumerator PrintMessage(string text, Color color)
    {
        GameObject msgObj = Instantiate(messagePrefab, messageParent);
        TextMeshProUGUI tmp = msgObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp == null)
        {
            Debug.LogError("Message prefab must contain a TextMeshProUGUI component!");
            yield break;
        }

        tmp.text = "";
        tmp.color = color;

        foreach (char c in text)
        {
            tmp.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        yield return new WaitForSeconds(timeBetweenMessages);
        StartCoroutine(AutoDestroy(msgObj));
    }

    private IEnumerator AutoDestroy(GameObject obj)
    {
        yield return new WaitForSeconds(messageLifetime);
        if (obj != null)
            Destroy(obj);
    }
    private void OnDestroy()
    {
        isShuttingDown = true;
        StopAllCoroutines();
    }
}
