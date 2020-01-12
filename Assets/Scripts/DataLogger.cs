using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataLogger : MonoBehaviour
{
    private StreamWriter writer;

    public bool Open(string fileName)
    {
        string timeStamp = System.DateTime.Now.ToString("HH:mm:ss:fff");
        string filePath = Application.persistentDataPath + fileName;
        try
        {
            writer = new StreamWriter(filePath, true);
            writer.WriteLine("SessionStart: " + timeStamp);
            writer.WriteLine("HH:mm:ss:fff" + "  " + "correct answer" + "  " + "given answer" + "  " + "sum of correct items");
        }
        catch (System.Exception e)
        {
            print(e.ToString());
            return false;
        }
        return true;
    }

    public void Close()
    {
        if (writer != null)
        {
            string timeStamp = System.DateTime.Now.ToString("HH:mm:ss:fff");
            writer.WriteLine("SessionEnd: " + timeStamp);
            writer.Close();
            writer = null;
        }
    }

    public void Flush()
    {
        writer.Flush();
    }

    public void writeLine(int trial, string correctAnswer, string enteredAnswer, int correctNum)
    {
        string timeStamp = System.DateTime.Now.ToString("HH:mm:ss:fff");
        writer.WriteLine(timeStamp + "  " + correctAnswer + "  " + enteredAnswer + "  " + correctNum);
    }

    private void OnApplicationQuit()
    {
        Close();
    }
}
