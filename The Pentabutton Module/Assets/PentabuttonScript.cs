using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class PentabuttonScript : MonoBehaviour {

    static int _moduleIdCounter = 1;
    int _moduleID = 0;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMSelectable Button;
    public KMColorblindMode Colourblind;
    public GameObject Base;
    public Material[] Colours;
    public Material Off;
    public TextMesh ColourblindText;
    public TextMesh Label;
    private bool ColourblindEnabled;
    private bool CorrectSoFar = true;
    private bool Holding;
    private bool Solved;
    private int HoldCorrect = 1;
    private int ReleaseCorrect;
    private int RndColour;
    private int LabelPos;
    private string[] Labels = { "press", "detonate", "hold", "abort", "release", "poke", "punch", "depress", "push", "select", "explode", "boom", "ignite", "escape", "colour", "penta", "button", };

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        ColourblindEnabled = Colourblind.ColorblindModeActive;
        LabelPos = Rnd.Range(0,Labels.Length);
        Button.OnInteract += delegate () { StartCoroutine(PressButton()); Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Button.transform); return false; };
        Button.OnInteractEnded += delegate () { StartCoroutine(ReleaseButton()); Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Button.transform); };
        Label.text = Labels[LabelPos];
    }

    // That backflip though
    void Start () {
        HoldCheck();
    }

    private IEnumerator PressButton()
    {
        if (!Solved)
        {
            Holding = true;
            RndColour = Rnd.Range(0, Colours.Length);
            ColourblindText.text = Colours[RndColour].name.ToString();
            ColourblindCheck();
            Base.GetComponent<MeshRenderer>().material = Colours[RndColour];
            if (HoldCorrect != ((int)Bomb.GetTime() % 60) % 10)
            {
                CorrectSoFar = false;
            }
            ReleaseCheck();
            yield return null;
            Debug.LogFormat("[The Pentabutton #{0}] The button was held when the last digit of the bomb's timer was {1}.", _moduleID, (int)(Bomb.GetTime() % 10));
            yield return null;
            Debug.LogFormat("[The Pentabutton #{0}] The base has lighted up {1}, therefore the button should be released when the last digit of the timer is {2}.", _moduleID, Colours[RndColour].name, ReleaseCorrect);
            for (int i = 0; i < 2; i++)
            {
                Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y - 0.004f, Button.transform.localPosition.z);
                yield return new WaitForSeconds(0.02f);
            }
        }
        yield break;

    }

    private IEnumerator ReleaseButton()
    {
        if (!Solved)
        {
            Holding = false;
            Base.GetComponent<MeshRenderer>().material = Off;
            ColourblindText.text = "";
            Debug.LogFormat("[The Pentabutton #{0}] The button was released when the last digit of the bomb's timer was {1}.", _moduleID, (int)(Bomb.GetTime() % 10));
            if (!CorrectSoFar || ReleaseCorrect != ((int)Bomb.GetTime() % 60) % 10)
            {
                Module.HandleStrike();
                Debug.LogFormat("[Tell Me When #{0}] That was not correct. Strike!", _moduleID);
                CorrectSoFar = true;
            }
            else
            {
                Module.HandlePass();
                Audio.PlaySoundAtTransform("solve", Button.transform);
                Debug.LogFormat("[Tell Me When #{0}] That was indeed the correct answer. Poggers!", _moduleID);
                Solved = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y + 0.004f, Button.transform.localPosition.z);
                yield return new WaitForSeconds(0.02f);
            }
        }
        yield break;
    }

    void HoldCheck()
    {
        if (LabelPos == 0)
        {
            HoldCorrect = 1 + Bomb.GetSerialNumberNumbers().First();
            Debug.LogFormat("[The Pentabutton #{0}] The label is PRESS, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        if (LabelPos == 15)
        {
            HoldCorrect = 1 + Bomb.GetSerialNumberNumbers().Last();
            Debug.LogFormat("[The Pentabutton #{0}] The label is PENTA, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        if (LabelPos == 6)
        {
            HoldCorrect = 1 + Bomb.GetSerialNumberNumbers().Sum();
            Debug.LogFormat("[The Pentabutton #{0}] The label is PUNCH, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        if (Bomb.GetSerialNumberNumbers().Last() % 2 != 0)
        {
            HoldCorrect = HoldCorrect + 2;
            Debug.LogFormat("[The Pentabutton #{0}] The last digit of the serial number is odd, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        if (HoldCorrect != 0)
        {
            if (HoldCorrect % 2 == 0)
            {
                HoldCorrect = HoldCorrect / 2;
                Debug.LogFormat("[The Pentabutton #{0}] N is now even, which changes N to {1}.", _moduleID, HoldCorrect);
            }
            else
            {
                HoldCorrect = HoldCorrect + 5;
                Debug.LogFormat("[The Pentabutton #{0}] N is now odd, which changes N to {1}.", _moduleID, HoldCorrect);
            }
        }
        else
        {
            HoldCorrect = HoldCorrect + 5;
            Debug.LogFormat("[The Pentabutton #{0}] N is now odd, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        if (IsPrime(Bomb.GetBatteryCount()))
        {
            HoldCorrect = HoldCorrect + Bomb.GetBatteryCount();
            Debug.LogFormat("[The Pentabutton #{0}] The amount of batteries is prime, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        else
        {
            HoldCorrect = HoldCorrect - Bomb.GetBatteryCount();
            Debug.LogFormat("[The Pentabutton #{0}] The amount of batteries is not prime, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        if (Label.text.Length >= 6)
        {
            HoldCorrect = HoldCorrect * 2 - Label.text.Length;
            Debug.LogFormat("[The Pentabutton #{0}] The label is 6 characters or longer, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        if (HoldCorrect % 2 != 0)
        {
            HoldCorrect = HoldCorrect + 4;
            Debug.LogFormat("[The Pentabutton #{0}] N is now odd, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        if (HoldCorrect % 3 == 0)
        {
            HoldCorrect = HoldCorrect + 3;
            Debug.LogFormat("[The Pentabutton #{0}] N is now divisible by 3, which changes N to {1}.", _moduleID, HoldCorrect);
        }
        while (HoldCorrect < 0)
        {
            HoldCorrect = HoldCorrect + 10;
        }
        HoldCorrect = HoldCorrect % 10;
        Debug.LogFormat("[The Pentabutton #{0}] No more rules apply, therefore the button should be held when the last digit of the timer is {1}.", _moduleID, HoldCorrect);
    }
    public static bool IsPrime(int number)
    {
        if (number <= 1) return false;
        if (number == 2) return true;
        if (number % 2 == 0) return false;

        var boundary = (int)Math.Floor(Math.Sqrt(number));

        for (int i = 3; i <= boundary; i += 2)
            if (number % i == 0)
                return false;

        return true;
    }
    void ReleaseCheck()
    {
        if (Label.text.Length >= 6)
        {
            if (RndColour == 0 || RndColour == 1)
            {
                ReleaseCorrect = (Bomb.GetSerialNumberNumbers().Last() + 7) % 10;
            }
            if (RndColour == 2)
            {
                ReleaseCorrect = (Bomb.GetSerialNumberNumbers().Last() + 3) % 10;
            }
            if (RndColour == 3)
            {
                ReleaseCorrect = Bomb.GetSerialNumberNumbers().Last();
            }
            if (RndColour == 4 || RndColour == 5)
            {
                ReleaseCorrect = (Bomb.GetSerialNumberNumbers().Last() + 8) % 10;
            }
            if (RndColour == 6)
            {
                ReleaseCorrect = (Bomb.GetSerialNumberNumbers().Last() + 4) % 10;
            }
        }
        else
        {
            if (RndColour == 0 || RndColour == 1)
            {
                ReleaseCorrect = (Bomb.GetSerialNumberNumbers().Last() + 1) % 10;
            }
            if (RndColour == 2)
            {
                ReleaseCorrect = (Bomb.GetSerialNumberNumbers().Last() + 5) % 10;
            }
            if (RndColour == 3)
            {
                ReleaseCorrect = (Bomb.GetSerialNumberNumbers().Last() + 2) % 10;
            }
            if (RndColour == 4 || RndColour == 5)
            {
                ReleaseCorrect = (Bomb.GetSerialNumberNumbers().Last() + 6) % 10;
            }
            if (RndColour == 6)
            {
                ReleaseCorrect = (Bomb.GetSerialNumberNumbers().Last() + 9) % 10;
            }
        }
    }
    void ColourblindCheck()
    {
        if (!ColourblindEnabled)
        {
            ColourblindText.text = "";
        }
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} hold on 5' to hold the button when the last digit of the bomb's timer is 5 and use '!{0} release on 5' to release the button when the last digit of the timer is 5.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] validcmdshold = { "hold on 0", "hold on 1", "hold on 2", "hold on 3", "hold on 4", "hold on 5", "hold on 6", "hold on 7", "hold on 8", "hold on 9" };
        string[] validcmdsrelease = { "release on 0", "release on 1", "release on 2", "release on 3", "release on 4", "release on 5", "release on 6", "release on 7", "release on 8", "release on 9" };
        int Time = 0;

        if ((!validcmdshold.Contains(command) && !Holding) || (!validcmdsrelease.Contains(command) && Holding))
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        for (int i = 0; i < 10; i++)
        {
            if ((validcmdshold[i] == command) || (validcmdsrelease[i] == command))
            {
                Time = i;
            }
        }
        while (Time != (int)Bomb.GetTime() % 10)
        {
            yield return null;
        }
        yield return null;
        if (validcmdshold.Contains(command))
        {
            Button.OnInteract();
        }
        else if (validcmdsrelease.Contains(command))
        {
            Button.OnInteractEnded();
        }
        else
        {
            yield return "sendtochaterror Oh shit something broke Kappa";
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        while ((int)Bomb.GetTime() % 10 != HoldCorrect)
            yield return true;
        Button.OnInteract();
        yield return true;
        while ((int)Bomb.GetTime() % 10 != ReleaseCorrect)
            yield return true;
        Button.OnInteractEnded();
    }
    // 5 glorious angles!!! O-O
}
