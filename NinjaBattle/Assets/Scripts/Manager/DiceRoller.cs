using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Nakama.Helpers;

public class DiceRoller : MonoBehaviour
{
    public Sprite[] Dice;
    public int rolls;
    public int rollValue;
    public int total;
    public int[] totalValue;

    private bool isRolling;
    private float totalTime;
    private float intervalTime;
    public int currrentDie;
    private bool dieRolled;
    private int currentTotal;

    Image die;

    void Start()
    {
        rolls = 0;
        total = 0;

        // moved this here because there is no need  to continually get the same object
        die = GameObject.Find("DieImage").GetComponent<Image>();

        Init();
    }

    private void Init()
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
                currrentDie = Random.Range(0, 6);
                die.transform.Rotate(0, 0, Random.Range(0, 360));

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

    public void AddRolls(int newRollValue)
    {
        rolls += newRollValue;
  
    }

    // removed the parameter because AddTotal() is called from wone place with same parameter
    public async void AddTotal()
    {
        // value is the true amount of the die face (currrentDie is 0 base so add 1)
        int value = currrentDie + 1;
        totalValue[currrentDie] += value; // add value to totalValue of current die
        total += value;                   // add value to total
        die.GetComponent<RectTransform>().eulerAngles = Vector3.zero;

     
    }

   
}