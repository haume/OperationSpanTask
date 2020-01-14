using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Configuration : MonoBehaviour
{
    public Dictionary<string, string> operations;
    public TextAsset opFile;
    public string[] letters;
    public TextAsset letterFile;
    public float width;
    public float height;
    public int id = -1;
    GameObject inputField;
    Text text;
    GameObject canvas;

    int port = 9900;
    UdpClient client;
    Thread appThread;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        width = Screen.width;
        height = Screen.height;
        inputField = GameObject.FindGameObjectWithTag("ID");
        text = GameObject.FindGameObjectWithTag("Text").GetComponent<Text>();
        appThread = new Thread(new ThreadStart(ReceiveData));
        appThread.IsBackground = true;
        appThread.Start();
    }

    void Start()
    {
        operations = new Dictionary<string, string>();
        string[] entries = opFile.text.Split(new char[] { '	', '\n' });  //"\n"[0]);
        for (int i = 0; i < entries.Length; i=i+2) 
        {
            operations.Add(entries[i], entries[i + 1]);
            print(entries[i]);
            print(entries[i + 1]);
        }
        letters = letterFile.text.Split("\n"[0]);
    }

    void Update() 
    {
        if (width != Screen.width || height != Screen.height) 
        {
            width = Screen.width;
            height = Screen.height;
            if (SceneManager.GetActiveScene().name == "MainScene") canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(width, height);
        }
    }

    public void SetID() 
    {
        if (int.TryParse(inputField.GetComponent<TMP_InputField>().text, out id))
        {
            SceneManager.LoadScene("MainScene");
            canvas = GameObject.FindGameObjectWithTag("MainCanvas");
        }
        else 
        {
            text.text = "This is not a valid ID. Your ID must not be empty and contain only digits. Please try again:";
            inputField.GetComponent<TMP_InputField>().text = "";
        }
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        client.Client.Blocking = false;
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                if (id == -1)
                {
                    int.TryParse(text, out id);
                    SceneManager.LoadScene("MainScene");
                    canvas = GameObject.FindGameObjectWithTag("MainCanvas");
                }
                if (text != id.ToString()) Application.Quit();
                print("text: " + text);
                print("id: " + id);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    void OnApplicationQuit()
    {
        stopThread();
    }

    private void stopThread()
    {
        if (appThread.IsAlive)
        {
            appThread.Abort();
        }
        client.Close();
    }
}
