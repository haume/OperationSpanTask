using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;
using System;

public class ScoreTracker : MonoBehaviour
{
    Configuration config;
    Dictionary<string, string> operations;
    string[] letters;
    string[] instructions;

    List<int> practiceLevels1 = new List<int> { 2, 3 };
    List<int> practiceLevels2 = new List<int> { 2, 3 };
    int practiceItemsPerLevel = 3;

    List<int> testLevels = new List<int> { 3, 4, 5, 6, 7, 8, 9, 10 };
    int testItemsPerLevel = 3;

    int timeOutFactor = 2;
    int measureAfterTrials = 2;
    List<float> times;
    float timeout = 0;
    float currentTime = 0;

    int experimentPhase = 0;

    int totalItems = 0;
    int correctItems = 0;
    int correct = 10; // 0 if the current answer is correct
    List<string> currentLetters;
    bool receivedText = false;
    bool waiting = false; //waiting after hiding input field (user must tap ENTER to proceed)

    public GameObject text;
    public GameObject buttonEnter;
    public GameObject buttonYes;
    public GameObject buttonNo;
    public GameObject inputField;
    int tapped = 0;
    bool proceed = false;
    string info = "";

    DataLogger log;
    private string file;
    int sessionID = 0;

    void Start()
    {
        currentLetters = new List<string>();
        times = new List<float>();
        config = GameObject.FindGameObjectWithTag("Config").GetComponent<Configuration>();
        operations = config.operations;
        letters = config.letters;
        sessionID = config.id;
        instructions = new string[]
        {
            "Welcome to our task! \n Tap ENTER to continue.",
            "In the following task you will need to do two things: check the validity of math equations and memorize the letters that appear after each item. Tap the button labelled YES if the equation is correct, or the button labelled NO if the equation is incorrect. \n After each equation a letter that we want you to memorize will appear.  After a certain number of items, a question mark will appear on the screen. At that point you will need to type in the list of letters. The order is irrelevant. \n Before the experiment starts, you will be doing some practice problems. During the practice problems, you will be told if you answered accurately or not. In the real experiment, there will be no feedback. Please work as quickly and as accurately as possible. Please only take breaks when directed to do so. \n Please tap ENTER to continue to the math equation practice section.",
            "In the second part of this experiment you will now see a letter after each equation. Your task is to remember these letters until you are prompted with a question mark to enter them. \n Type in the letters and press OK when you're done. \n Please make sure your answer is correct before entering. \n Tap ENTER to continue to the final practice round.",
            "You should now understand the experiment. If you do not have any more questions, we will now start with the actual experiment. If are not sure what you are being asked to do, please stop now and ask the experimenter for help. \n Make sure to respond as quickly and accurately as possible! There is a time limit for every item, so you will need to work fast. \n Tap ENTER to continue to the final part of the experiment.",
            "All done! \n Please tap ENTER to continue."
        };
        file = (System.DateTime.Now.ToString("yyyyMMdd") + "_" + sessionID + ".txt");
        log = new DataLogger();
        log.Open(file);
        UpdateText();
    }

    void UpdateText()
    {
        switch (experimentPhase)
        {
            case 0: //welcome
                text.GetComponent<Text>().text = instructions[0];
                break;
            case 1: //traning equations only
                StartCoroutine(TrainingOne());
                break;
            case 2: //instructions
                text.GetComponent<Text>().text = instructions[2];
                break;
            case 3: //training equations + letters
                StartCoroutine(TrainingTwo());
                break;
            case 4: //instructions
                text.GetComponent<Text>().text = instructions[3];
                break;
            case 5: //main experiment
                timeout = CalculateTimeout();
                StartCoroutine(Experiment());
                break;
            case 6: //end
                Application.Quit();
                break;
            default:
                break;
        }
    }

    IEnumerator TrainingOne()
    {
        yield return StartCoroutine(RunExperiment(practiceLevels1, practiceItemsPerLevel, false, true));
        info = "Out of " + totalItems + " problems, you answered " + correctItems + " correctly. \n Please tap ENTER to continue.";
        totalItems = 0;
        correctItems = 0;
        ToggleButtons();
        text.GetComponent<Text>().text = info;
    }

    IEnumerator TrainingTwo()
    {
        yield return StartCoroutine(RunExperiment(practiceLevels2, practiceItemsPerLevel, true, false));
        ToggleButtons();
        text.GetComponent<Text>().text = instructions[4];
        totalItems = 0;
        correctItems = 0;
    }

    IEnumerator Experiment()
    {
        yield return StartCoroutine(RunExperiment(testLevels, testItemsPerLevel, true, false));
    }

    public void TapEnter()
    {
        if (experimentPhase == 0 && text.GetComponent<Text>().text == instructions[0])
        {
            text.GetComponent<Text>().text = instructions[1];
        }
        else if (waiting)
        {
            proceed = true;
        }
        else
        {
            if (text.GetComponent<Text>().text != instructions[4] && text.GetComponent<Text>().text != info) experimentPhase++;
            UpdateText();
        }
    }

    public void TapYes()
    {
        tapped = 1;
    }

    public void TapNo()
    {
        tapped = 2;
    }

    public void Input()
    {
        receivedText = true;
    }

    IEnumerator GetText(int trial)
    {
        while (!receivedText)
        {
            yield return null;
        }
        string input = inputField.GetComponent<TMP_InputField>().text;
        char[] letters = input.ToCharArray();
        letters = letters.Distinct().ToArray();
        string correctLetters = "";
        for (int i = 0; i < letters.Length; i++)
        {
            foreach (string str in currentLetters)
            {
                print(str);
                print(letters[i].ToString());
                if (string.Compare(letters[i].ToString(), str) == 0)
                {
                    correctItems++;
                }
            }
        }
        foreach (string str in currentLetters) correctLetters += str;
        log.writeLine(trial, correctLetters.Replace("\r", "").Replace("\n", ""), input, correctItems, totalItems);
        receivedText = false;
        currentLetters.RemoveRange(0, currentLetters.Count);
        inputField.GetComponent<TMP_InputField>().text = "";
        inputField.SetActive(false);
        buttonEnter.SetActive(true);
        yield return StartCoroutine(AfterText());
    }

    IEnumerator AfterText()
    {
        text.GetComponent<Text>().text = "Please tap ENTER to continue.";
        while (!proceed)
        {
            waiting = true;
            yield return null;
        }
        text.GetComponent<Text>().text = "";
        yield return new WaitForSeconds(0.1f);
        ToggleButtons();
        waiting = false;
    }

    IEnumerator RunExperiment(List<int> levels, int items, bool showLetters, bool showFeedback)
    {
        ToggleButtons();
        int currentLevel = 0;
        for (; levels.Count != 0;)
        {
            int index = UnityEngine.Random.Range(0, levels.Count - 1);
            currentLevel = levels[index];
            int trial = 0;
            for (int i = 0; i < currentLevel; i++)
            {
                for (int j = 0; j < items; j++)
                {
                    int opIndex = UnityEngine.Random.Range(0, operations.Count);
                    string equation = operations.ElementAt(opIndex).Key;
                    string answer = operations.ElementAt(opIndex).Value;
                    trial = (i + 1) * (j + 1);
                    yield return StartCoroutine(ShowEquation(equation, answer, trial));
                    if (showFeedback)
                    {
                        yield return StartCoroutine(ShowFeedback());
                    }
                    if (timeout == 0)
                    {
                        if (measureAfterTrials > 0) measureAfterTrials--;
                    }
                    if (showLetters)
                    {
                        int letterIndex;
                        do
                        {
                            letterIndex = UnityEngine.Random.Range(0, letters.Length);
                        }
                        while (currentLetters.Contains(letters[letterIndex]));
                        text.GetComponent<Text>().text = letters[letterIndex];
                        currentLetters.Add(letters[letterIndex]);
                        yield return new WaitForSeconds(1f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                if (showLetters)
                {
                    text.GetComponent<Text>().text = "?";
                    buttonNo.SetActive(false);
                    buttonYes.SetActive(false);
                    inputField.SetActive(true);
                    yield return StartCoroutine(GetText(trial));
                }
            }
            levels.RemoveAt(index);
        }
        experimentPhase++;
        UpdateText();
    }

    IEnumerator ShowEquation(string op, string rightAnswer, int trial)
    {
        text.GetComponent<Text>().text = "";
        yield return new WaitForSeconds(0.1f);
        totalItems++;
        text.GetComponent<Text>().text = op;
        float timeSinceStart = Time.timeSinceLevelLoad;
        yield return StartCoroutine(WaitForAnswer());
        currentTime = Time.timeSinceLevelLoad - timeSinceStart;
        if (measureAfterTrials == 0 && timeout == 0)
        {
            times.Add(currentTime);
            currentTime = 0;
        }
        string currentAnswer = "";
        if (tapped == 1)
        {
            currentAnswer = "y";
        }
        else if (tapped == 2)
        {
            currentAnswer = "n";
        }
        //print(tapped);
        //print(currentAnswer.Length);
        //print(rightAnswer.Length);
        correct = string.Compare(rightAnswer[0].ToString(), currentAnswer);
        print(correct);
        if (correct == 0)
        {
            correctItems++;
        }
        log.writeLine(trial, rightAnswer.Replace("\r", "").Replace("\n", ""), currentAnswer, correctItems, totalItems);
        log.Flush();
        tapped = 0;
    }

    IEnumerator WaitForAnswer()
    {
        float start = Time.timeSinceLevelLoad;
        while (tapped == 0)
        {
            if (timeout != 0 && timeout <= Time.timeSinceLevelLoad - start)
            {
                text.GetComponent<Text>().text = "You are out of time!";
                tapped = -1;
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
        }
    }

    IEnumerator ShowFeedback()
    {
        if (correct == 0)
        {
            text.GetComponent<Text>().text = "Very Good!";
        }
        else
        {
            text.GetComponent<Text>().text = "Oops! Better luck next time.";
        }
        yield return new WaitForSeconds(0.5f);
    }

    void ToggleButtons()
    {
        buttonNo.SetActive(!buttonNo.activeInHierarchy);
        buttonYes.SetActive(!buttonYes.activeInHierarchy);
        buttonEnter.SetActive(!buttonEnter.activeInHierarchy);
    }

    float CalculateTimeout()
    {
        if (times.Count == 0) return 0;
        float timeout = 0;
        List<float> differences = new List<float>();
        foreach (float time in times)
        {
            foreach (float compare in times)
            {
                differences.Add(time - compare);
            }
        }
        float sum = 0;
        foreach (float diff in differences)
        {
            sum += diff;
        }
        float mean = sum / times.Count;
        float sd = 0;
        sum = 0;
        foreach (float time in times)
        {
            sum += (mean - time) * (mean - time);
        }
        sd = Mathf.Sqrt(sum / times.Count);
        timeout = mean + timeOutFactor * sd;
        print(timeout);
        return timeout;
    }
}
