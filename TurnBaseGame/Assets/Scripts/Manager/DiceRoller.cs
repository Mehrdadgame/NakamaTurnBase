using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Nakama.Helpers;
using System;
/// <summary>
/// This Calss for DiceRoller 
/// </summary>
public class DiceRoller : MonoBehaviour
{

    public Action<bool> RollUp;
    public Sprite[] Dice;
    public int rolls;
    public int rollValue;
    public int total;
    public int[] totalValue;

    public bool isRolling;
    private float totalTime;
    private float intervalTime;
    public int currrentDie;
    public bool dieRolled;
    private int currentTotal;
    public bool isRootDice;

    // Tutorial: when set (>0), overrides random roll with this value (1-based)
    private int _forcedValue = -1;
    public void ForceNextValue(int value) => _forcedValue = value;

    Image die;

    void Start()
    {
        rolls = 0;
        total = 0;

        // moved this here because there is no need  to continually get the same object
        die = GameObject.Find("DieImage").GetComponent<Image>();

        Init();
    }

    public void Init()
    {
        totalTime = 0.0f;
        intervalTime = 0.0f;
        currrentDie = 0;
        dieRolled = false;
        die.sprite = Dice[currrentDie];
    }

    void Update()
    {
        if (isRolling)
        {
            intervalTime += Time.deltaTime;
            totalTime += Time.deltaTime;

            if (intervalTime >= 0.1f)
            {
                //change die & rotation
                currrentDie = UnityEngine.Random.Range(0, 6);
                die.transform.Rotate(0, 0, UnityEngine.Random.Range(0, 360));

                //set image to selected die
                die.sprite = Dice[currrentDie];
                intervalTime -= 0.1f;
            }

            if (totalTime >= 2.00f)
            {
                isRolling = false;
                dieRolled = true;
                AddRolls(rollValue);
                // removed the parameter because AddTotal() is called from wone place with same parameter
                AddTotal();
            }
        }

        if (isRootDice)
        {
            die.transform.Rotate(0, 0, UnityEngine.Random.Range(0, 360) * Time.deltaTime * 2);
            currrentDie = UnityEngine.Random.Range(0, 6);
            die.sprite = Dice[currrentDie];
        }
        else
        {
            die.GetComponent<RectTransform>().eulerAngles = Vector3.zero;
        }
    }

    public void DieImage_Click(Button button)
    {
        if (!dieRolled)
        {
            isRolling = true;
        }

        if (!isRolling)
        {
            Init();
            isRolling = true;
        }
        button.interactable = false;
      
    }

    public void PanelTestButton_Click()
    {
        GameObject panel = GameObject.Find("DiePanel");
        Animator animator = panel.GetComponent<Animator>();
        animator.Play("DiePanelOpen");
    }

    public void PanelOKButton_Click()
    {
        GameObject panel = GameObject.Find("DiePanel");
        Animator animator = panel.GetComponent<Animator>();
        animator.Play("DiePanelClose");
        rolls = 0;
        total = 0;

    }
    public void Rotation(bool isRoot)
    {
        isRootDice = isRoot;
    }

    public void AddRolls(int newRollValue)
    {
        rolls += newRollValue;

    }

    // removed the parameter because AddTotal() is called from wone place with same parameter
    public async void AddTotal()
    {
        // Tutorial: if a forced value was set, use it instead of random
        if (_forcedValue > 0)
        {
            currrentDie  = _forcedValue - 1;  // convert to 0-based index
            die.sprite   = Dice[currrentDie];
            _forcedValue = -1;
        }

        int value = currrentDie + 1;
        total += value;
        die.GetComponent<RectTransform>().eulerAngles = Vector3.zero;
        RollUp?.Invoke(true);
    }


}