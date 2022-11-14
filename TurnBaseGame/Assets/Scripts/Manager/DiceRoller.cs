using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Nakama.Helpers;
using System;

/* A class declaration. */
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
    private float intervalTimeOppTurn;
    public int currrentDie = -1;
    public bool dieRolled;
    public bool isRootDice;

    private Image dice;

    void Start()
    {
        rolls = 0;
        total = 0;
        // moved this here because there is no need  to continually get the same object
        dice = GameObject.Find("DieImage").GetComponent<Image>();

        Init();
    }

    public void Init()
    {
        totalTime = 0.0f;
        intervalTime = 0.0f;
        currrentDie = 0;
        dieRolled = false;
        dice.sprite = Dice[currrentDie];
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
                dice.transform.Rotate(0, 0, UnityEngine.Random.Range(0, 360));

                //set image to selected die
                dice.sprite = Dice[currrentDie];
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
            intervalTimeOppTurn += Time.deltaTime;
            if (intervalTimeOppTurn >= 0.2f)
            {
                dice.transform.Rotate(0, 0, UnityEngine.Random.Range(0, 360));
                currrentDie = UnityEngine.Random.Range(0, 6);
                dice.sprite = Dice[currrentDie];
                intervalTimeOppTurn -= 0.2f;
            }
        }
        else
        {
            intervalTimeOppTurn=0;
            dice.GetComponent<RectTransform>().eulerAngles = Vector3.zero;
        }
    }

    /// <summary>
    /// If the die hasn't been rolled, then set the die to rolling. If the die isn't rolling, then
    /// initialize the die and set it to rolling
    /// </summary>
    /// <param name="Button">The button that was clicked</param>
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
    public void AddTotal()
    {
        // value is the true amount of the die face (currrentDie is 0 base so add 1)
        int value = currrentDie + 1;
        //   totalValue[currrentDie] += value; // add value to totalValue of current die
        total += value;                   // add value to total
        dice.GetComponent<RectTransform>().eulerAngles = Vector3.zero;
        RollUp?.Invoke(true);



    }


}