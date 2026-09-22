using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Nakama.Helpers;
using System;
using DG.Tweening;

/// <summary>
/// Controls dice rolling, animations, and turn interactions for all game modes.
/// </summary>
public class DiceRoller : MonoBehaviour
{
    public Action<bool> RollUp;
    public event Action RollStarted;
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

    private Image die;
    private RectTransform _dieRt;
    private Vector2 _dieBasePos;
    private Vector3 _dieBaseScale = Vector3.one;
    private Sequence _arrivalSequence;
    private Sequence _idleSequence;

    void Awake()
    {
        ResolveDie();
    }

    void Start()
    {
        rolls = 0;
        total = 0;
        ResolveDie();
        Init();
    }

    private void ResolveDie()
    {
        if (die == null)
        {
            GameObject dieObj = GameObject.Find("DieImage");
            if (dieObj != null)
                die = dieObj.GetComponent<Image>();
            else
                die = GetComponent<Image>();
        }

        if (die != null && _dieRt == null)
        {
            _dieRt = die.rectTransform;
            _dieBasePos = _dieRt.anchoredPosition;
            _dieBaseScale = _dieRt.localScale == Vector3.zero ? Vector3.one : _dieRt.localScale;
        }
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }

    private void KillAllTweens()
    {
        if (_arrivalSequence != null)
        {
            _arrivalSequence.Kill(false);
            _arrivalSequence = null;
        }
        if (_idleSequence != null)
        {
            _idleSequence.Kill(false);
            _idleSequence = null;
        }
        if (_dieRt != null)
        {
            _dieRt.DOKill();
        }
    }

    public void Init()
    {
        PrepareForTurn();
    }

    public void PrepareForTurn()
    {
        totalTime = 0.0f;
        intervalTime = 0.0f;
        currrentDie = -1;
        dieRolled = false;
        isRolling = false;

        ResolveDie();
        if (_dieRt != null)
        {
            KillAllTweens();
            _dieRt.anchoredPosition = _dieBasePos;
            _dieRt.eulerAngles = Vector3.zero;

            if (Dice != null && Dice.Length > 0)
                die.sprite = Dice[0];

            // ── Dynamic Die Arrival Animation ──
            // The die pops in with an elastic spin to signal the player's turn
            _dieRt.localScale = Vector3.zero;
            _dieRt.localEulerAngles = new Vector3(0f, 0f, 180f);

            _arrivalSequence = DOTween.Sequence()
                .Append(_dieRt.DOScale(_dieBaseScale * 1.15f, 0.32f).SetEase(Ease.OutBack))
                .Join(_dieRt.DOLocalRotate(Vector3.zero, 0.32f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
                .Append(_dieRt.DOScale(_dieBaseScale, 0.14f).SetEase(Ease.InOutSine))
                .AppendCallback(StartIdleAnimation)
                .SetUpdate(true)
                .SetLink(gameObject);
        }
    }

    private void StartIdleAnimation()
    {
        if (_dieRt == null || isRolling || dieRolled)
            return;

        if (_idleSequence != null)
        {
            _idleSequence.Kill(false);
            _idleSequence = null;
        }

        // Gentle inviting pulse while waiting for player to tap roll
        _idleSequence = DOTween.Sequence()
            .Append(_dieRt.DOScale(_dieBaseScale * 1.07f, 0.55f).SetEase(Ease.InOutSine))
            .Append(_dieRt.DOScale(_dieBaseScale, 0.55f).SetEase(Ease.InOutSine))
            .SetLoops(-1)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void StopIdleAnimation()
    {
        if (_idleSequence != null)
        {
            _idleSequence.Kill(false);
            _idleSequence = null;
        }
    }

    void Update()
    {
        if (isRolling)
        {
            intervalTime += Time.deltaTime;
            totalTime += Time.deltaTime;

            if (intervalTime >= 0.08f)
            {
                // Dynamic tumbling: change die face, rotate, and slight squash-stretch
                currrentDie = UnityEngine.Random.Range(0, 6);
                if (Dice != null && Dice.Length > currrentDie)
                    die.sprite = Dice[currrentDie];

                die.transform.Rotate(0, 0, UnityEngine.Random.Range(40f, 80f));

                // Micro tumbling oscillation
                if (_dieRt != null)
                {
                    float sx = UnityEngine.Random.Range(0.92f, 1.10f);
                    float sy = UnityEngine.Random.Range(0.92f, 1.10f);
                    _dieRt.localScale = new Vector3(_dieBaseScale.x * sx, _dieBaseScale.y * sy, 1f);
                }

                intervalTime -= 0.08f;
            }

            if (totalTime >= 1.9f)
            {
                isRolling = false;
                dieRolled = true;
                AddRolls(rollValue);
                AddTotal();
            }
        }

        if (isRootDice)
        {
            die.transform.Rotate(0, 0, UnityEngine.Random.Range(0, 360) * Time.deltaTime * 2);
            currrentDie = UnityEngine.Random.Range(0, 6);
            if (Dice != null && Dice.Length > currrentDie)
                die.sprite = Dice[currrentDie];
        }
        else if (!isRolling && !dieRolled && _arrivalSequence == null)
        {
            if (_dieRt != null && _idleSequence == null)
                _dieRt.eulerAngles = Vector3.zero;
        }
    }

    public void DieImage_Click(Button button)
    {
        if (isRolling || dieRolled)
            return;

        TutorialManager tutorial = TutorialManager.Instance;
        if (tutorial != null && !tutorial.CanRollDice())
            return;

        StopIdleAnimation();
        RollStarted?.Invoke();

        if (button != null)
            button.interactable = false;

        // Anticipation wind-up before rolling
        if (_dieRt != null)
        {
            _dieRt.DOKill();
            _dieRt.DOScale(_dieBaseScale * 0.82f, 0.12f).SetEase(Ease.InQuad).SetUpdate(true).OnComplete(() =>
            {
                isRolling = true;
                totalTime = 0f;
                intervalTime = 0f;
            });
        }
        else
        {
            isRolling = true;
            totalTime = 0f;
            intervalTime = 0f;
        }
    }

    public void PanelTestButton_Click()
    {
        GameObject panel = GameObject.Find("DiePanel");
        if (panel != null)
        {
            Animator animator = panel.GetComponent<Animator>();
            if (animator != null)
                animator.Play("DiePanelOpen");
        }
    }

    public void PanelOKButton_Click()
    {
        GameObject panel = GameObject.Find("DiePanel");
        if (panel != null)
        {
            Animator animator = panel.GetComponent<Animator>();
            if (animator != null)
                animator.Play("DiePanelClose");
        }
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

    public void AddTotal()
    {
        // Tutorial: if a forced value was set, use it instead of random
        if (_forcedValue > 0)
        {
            currrentDie = _forcedValue - 1;  // convert to 0-based index
            if (Dice != null && Dice.Length > currrentDie)
                die.sprite = Dice[currrentDie];
            _forcedValue = -1;
        }

        int value = currrentDie + 1;
        total += value;

        ResolveDie();
        if (_dieRt != null)
        {
            _dieRt.DOKill();
            _dieRt.eulerAngles = Vector3.zero;
            _dieRt.anchoredPosition = _dieBasePos;

            // ── Dramatic Slam Impact on final face ──
            _dieRt.localScale = _dieBaseScale * 1.36f;
            DOTween.Sequence()
                .Append(_dieRt.DOScale(_dieBaseScale, 0.28f).SetEase(Ease.OutBounce))
                .Join(_dieRt.DOPunchRotation(new Vector3(0f, 0f, 20f), 0.36f, 7, 0.5f))
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        RollUp?.Invoke(true);
    }
}