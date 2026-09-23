using Nakama.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NinjaBattle.Game;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.U2D;
using Unity.VisualScripting;
using RTLTMPro;
using DG.Tweening;

public class UiManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Button dicRollButton;
    [SerializeField] private Transform transformOpp;

    [SerializeField] private List<TileDataOpp> tileDataOpps = new List<TileDataOpp>();
    [SerializeField] private List<ClickInCell> tileDataMe = new List<ClickInCell>();

    [SerializeField] private RTLTextMeshPro ScoreTextOpp;
    [SerializeField] private RTLTextMeshPro ScoreTextMe;

    [SerializeField] private Animator TextTurnYou;
    [SerializeField] private Animator TextTurnOpp;
    [SerializeField] private Animator TextValueMines;
    [SerializeField] private Sprite[] DiceRollsSprite;
    [SerializeField] private GameObject rematchPanle;
    [SerializeField] private RTLTextMeshPro messageLeftPalyerInRematch;
    [SerializeField] private Button rematchButton;

    [SerializeField] private Button acceptRematchButton;
    [SerializeField] private Button exitRematchButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private Image loading;
    public RTLTextMeshPro[] arryRowSumMe;
    public RTLTextMeshPro[] arryRowSumMeCal;
    public RTLTextMeshPro[] arryRowSumOpp;
    public RTLTextMeshPro[] arryRowSumOppCal;
    public RTLTextMeshPro NameOpp;
    public ParticleSystem WowPar;
    public static UiManager instance;


    public RTLTextMeshPro TasiWin;
    public GameObject PanelLeftPalyer;
    public TextMeshProUGUI NamePalyerLeft;

    public Animator StickerShow;
    public Image StickerOpp;
    public SpriteAtlas AllAssets;
    public Color colroParticlewhite;

    private DG.Tweening.Sequence rollAttentionSequence;
    private RectTransform rollAttentionTarget;
    private Vector3 rollAttentionBaseScale;
    private Vector3 rollAttentionBaseEuler;

    private void Start()
    {
        instance = this;
        NameOpp.text = PlayerPrefs.GetString("Opp", "Opponent");

        bool isTutorial = PlayerPrefs.GetInt(WelcomePopup.TutorialModeKey, 0) == 1;

        if (MultiplayerManager.Instance.isTurn)
        {
            GameManager.Instance.diceRoller.PrepareForTurn();
            dicRollButton.interactable = true;
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];
            TextTurnYou.Play("YouTurn", 0, 0);
            // Tutorial: keep timer paused until overlay guides the player
            TimerTurn.instance.TimerRunning = !isTutorial;
            if (isTutorial) TimerTurn.instance.TimerPause = true;
        }
        else
        {
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];
            TextTurnOpp.Play("OppTurn", 0, 0);
            TimerTurn.instance.TimerRunning = false;
            GameManager.Instance.diceRoller.Rotation(true);
        }

        SetInitialCellParticles();
        if (MultiplayerManager.Instance.isTurn)
            StartRollAttention();
    }

    /// در شروع بازی: particle زمین خودم روشن، particle زمین حریف خاموش
    /// تا کاربر بفهمه کدوم زمین خودشه
    private void SetInitialCellParticles()
    {
        if (tileDataMe != null)
        {
            foreach (var cell in tileDataMe)
            {
                if (cell == null) continue;
                var ps = cell.GetComponentInChildren<ParticleSystem>();
                if (ps != null) ps.Play();
            }
        }
        if (tileDataOpps != null)
        {
            foreach (var cell in tileDataOpps)
            {
                if (cell == null) continue;
                var ps = cell.GetComponentInChildren<ParticleSystem>();
                if (ps != null) ps.Stop();
            }
        }
    }
    void OnEnable()
    {
        GameManager.Instance.diceRoller = GameObject.Find("DieImage").GetComponent<DiceRoller>();
        PlayersManager.Instance.onSetDataInTurn += Instance_SetDataInTurn;
        PlayersManager.Instance.onSetScoreMe += Instance_onSetScoreMe;
        PlayersManager.Instance.onSetScoreOpp += Instance_onSetScoreOpp;
        MultiplayerManager.Instance.onTurnMe += Instance_onTurnMe;
        PlayersManager.Instance.onSetDataInRowMe += Instance_onSetDataInRowMe;
        PlayersManager.Instance.onSetDataInRowOpp += Instance_onSetDataInRowOpp;
        PlayersManager.Instance.onRematch += Instance_onRematch;
        PlayersManager.Instance.IsTurn += Instance_IsTurn;
        GameManager.Instance.diceRoller.RollUp += ShowHighLight;
        GameManager.Instance.diceRoller.RollStarted += StopRollAttention;
        TimerTurn.instance.TimerStop += Instance_TimerStop;
        PlayersManager.Instance.LeftPlayer += Instance_LeftPlayer;
    }

    private void Instance_LeftPlayer(string obj)
    {
        PanelLeftPalyer.SetActive(true);
        NamePalyerLeft.text = obj + " از بازی خارج شد";
    }

    private void Instance_TimerStop()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsActive)
            return;  // tutorial manages its own flow; never force-place tiles
        GameManager.Instance.diceRoller.currrentDie = Random.Range(0, 6);
        var cell = tileDataMe.First(e => e.isLock == false);
        cell.SetDataInCell();
    }

    void OnDestroy()
    {
        PlayersManager.Instance.onSetDataInTurn -= Instance_SetDataInTurn;
        PlayersManager.Instance.onSetScoreOpp -= Instance_onSetScoreOpp;
        PlayersManager.Instance.onSetScoreMe -= Instance_onSetScoreMe;
        MultiplayerManager.Instance.onTurnMe -= Instance_onTurnMe;
        PlayersManager.Instance.onSetDataInRowMe -= Instance_onSetDataInRowMe;
        PlayersManager.Instance.onSetDataInRowOpp -= Instance_onSetDataInRowOpp;
        PlayersManager.Instance.IsTurn -= Instance_IsTurn;
        PlayersManager.Instance.onRematch -= Instance_onRematch;
        if (GameManager.Instance != null && GameManager.Instance.diceRoller != null)
        {
            GameManager.Instance.diceRoller.RollUp -= ShowHighLight;
            GameManager.Instance.diceRoller.RollStarted -= StopRollAttention;
        }
        TimerTurn.instance.TimerStop -= Instance_TimerStop;
        PlayersManager.Instance.LeftPlayer -= Instance_LeftPlayer;
        StopRollAttention();
    }
    private void Instance_onRematch(RematchData obj)
    {
        //if (obj.UserId == MultiplayerManager.Instance.Self.UserId)
        //    return;

        if (obj.Answer == "req")
        {
            rematchPanle.SetActive(true);
            ActionEndGame.instance.ResultPanel.SetActive(false);
            AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
            AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);
        }
        if (obj.Answer == "yes")
        {
            rematchPanle.SetActive(false);
            ActionEndGame.instance.ResultPanel.SetActive(false);
            _ = Task.Delay(1000);
            ResetGame();
        }
        if (obj.Answer == "left")
        {
            messageLeftPalyerInRematch.text = "متاسفانه بازیکن از بازی خارج شد.";
            acceptRematchButton.gameObject.SetActive(true);
            acceptRematchButton.gameObject.SetActive(false);
            exitRematchButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            ActionEndGame.instance.ResultPanel.SetActive(false);
            loading.gameObject.SetActive(false);
            AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
            AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);
        }
        if (obj.Answer == "no")
        {
            messageLeftPalyerInRematch.text = "حریف قبول نکرد";
            loading.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
            AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);

        }
    }

    public void SendAcceptForRematch(string answerOpp)
    {
        rematchPanle.SetActive(true);
        AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
        AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);
        ActionEndGame.instance.ResultPanel.SetActive(false);
        var answer = new RematchData
        {
            Answer = answerOpp,
            UserId = MultiplayerManager.Instance.Self.UserId

        };
        if (answer.Answer == "send")
        {
            acceptRematchButton.gameObject.SetActive(false);
            exitRematchButton.gameObject.SetActive(false);
            loading.gameObject.SetActive(true);
        }
        else if (answer.Answer == "req")
        {
            loading.gameObject.SetActive(true);
            ActionEndGame.instance.ResultPanel.SetActive(false);

        }

        MultiplayerManager.Instance.Send(MultiplayerManager.Code.Rematch, answer);
    }

    private async void ResetGame()
    {
        foreach (var opp in tileDataOpps)
        {
            opp.SpriteDice.transform.parent.gameObject.SetActive(false);
            opp.ValueTile = 0;
            opp.GetComponentInChildren<ParticleSystem>().Stop();
            ParticleSystem.MainModule settings = opp.GetComponentInChildren<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(colroParticlewhite);
        }
        foreach (var me in tileDataMe)
        {
            me.isLock = false;
            me.ValueTile = 0;
            me.GetComponentInChildren<ParticleSystem>().Stop();
            ParticleSystem.MainModule settings = me.GetComponentInChildren<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(colroParticlewhite);
            me.SpriteDice.transform.parent.gameObject.SetActive(false);
        }
        ScoreTextOpp.text = "0";
        ScoreTextOpp.text = "0";
        RowSum();
        await Task.Delay(750);
        AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(true);
        AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(true);
        ActionEndGame.instance.ResultPanel.SetActive(false);
        AniamtionManager.instance.AnimGoToUpMe.Play("GotoUpPageMe", 0, 0);
        AniamtionManager.instance.AnimGoToUpOpp.Play("GoToUpOpp", 0, 0);
        await Task.Delay(1000);
        AniamtionManager.instance.AnimGoToUpMe.enabled = false;
        AniamtionManager.instance.AnimGoToUpOpp.enabled = false;
        AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconMe;
        AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconOpp;
        AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

    }
    private void ShowHighLight(bool obj)
    {
        var list = tileDataMe.FindAll(e => e.isLock == false);
        if (obj)
        {
            foreach (var item in list)
                item.GetComponentInChildren<ParticleSystem>().Play();

            TutorialManager.Instance?.OnDiceRolled();

            // Fly bubble: player's dice value travels to the local grid.
            int diceVal = GameManager.Instance.diceRoller.currrentDie + 1;
            TutorialManager.Instance?.ShowDiceFlyBubble(diceVal, isLocalMove: true);
        }
        else
        {
            foreach (var item in list)
                item.GetComponentInChildren<ParticleSystem>().Stop();
        }
        // RowSum();
    }

    private void Instance_onSetScoreMe(int obj, int mines, DataPlayer data)
    {
        if (mines > 0)
        {
            var tmp = TextValueMines.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = "-" + mines;
            TextValueMines.Play("Mines", 0, 0);
            TextValueMines.transform.DOKill();
            TextValueMines.transform.localScale = Vector3.one;
            TextValueMines.transform.DOPunchScale(Vector3.one * 0.35f, 0.3f, 5, 0.5f).SetUpdate(true);
            if (WowPar != null) WowPar.Play();
            mines = 0;
        }
        if (ScoreTextOpp != null)
        {
            ScoreTextOpp.text = PersianTextUtils.FormatNumberStandalone(obj);
            ScoreTextOpp.rectTransform.DOKill();
            ScoreTextOpp.rectTransform.localScale = Vector3.one;
            ScoreTextOpp.rectTransform.DOPunchScale(Vector3.one * 0.28f, 0.25f, 4, 0.5f).SetUpdate(true);
        }
    }

    private void Instance_onSetScoreOpp(int obj, int mines, DataPlayer data)
    {
        if (mines > 0)
        {
            var tmp = TextValueMines.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = "-" + PersianTextUtils.ToPersianDigits(mines.ToString());
            TextValueMines.Play("Mines", 0, 0);
            TextValueMines.transform.DOKill();
            TextValueMines.transform.localScale = Vector3.one;
            TextValueMines.transform.DOPunchScale(Vector3.one * 0.35f, 0.3f, 5, 0.5f).SetUpdate(true);
            if (WowPar != null) WowPar.Play();
            mines = 0;
        }
        if (ScoreTextMe != null)
        {
            ScoreTextMe.text = PersianTextUtils.FormatNumberStandalone(obj);
            ScoreTextMe.rectTransform.DOKill();
            ScoreTextMe.rectTransform.localScale = Vector3.one;
            ScoreTextMe.rectTransform.DOPunchScale(Vector3.one * 0.28f, 0.25f, 4, 0.5f).SetUpdate(true);
        }
    }

    private void SetRowSumText(TextMeshProUGUI label, int newScore)
    {
        if (label == null) return;
        string newText = PersianTextUtils.FormatNumberStandalone(newScore);
        if (label.text != newText)
        {
            label.text = newText;
            label.rectTransform.DOKill();
            label.rectTransform.localScale = Vector3.one;
            label.rectTransform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 4, 0.5f).SetUpdate(true);
        }
    }

    int Count;
    [UnityEngine.ContextMenu("sum")]
    public void RowSum()
    {
        var calc = CalculterRowScore.instance;

        if (GameManager.Instance.modeGame == ModeGame.VerticalAndHorizontal)
        {
            // VerticalAndHorizontal: each cell belongs to BOTH a row list and a column list.
            // Stop all particles upfront (using row lists, which cover every cell),
            // then run all calculations with skipClear=true so row matches don't
            // get erased when column processing runs.

            calc.StopParticlesOpp(calc.tileDataOpps);
            calc.StopParticlesOpp(calc.tileDataOpps2);
            calc.StopParticlesOpp(calc.tileDataOpps3);
            calc.StopParticlesMe(calc.clickInCells);
            calc.StopParticlesMe(calc.clickInCells1);
            calc.StopParticlesMe(calc.clickInCells2);

            // Rows (activate row-match particles)
            if (arryRowSumOpp != null && arryRowSumOpp.Length >= 3)
            {
                SetRowSumText(arryRowSumOpp[0], calc.TilesOpp(calc.tileDataOpps, out Count, skipClear: true));
                SetRowSumText(arryRowSumOpp[1], calc.TilesOpp(calc.tileDataOpps2, out Count, skipClear: true));
                SetRowSumText(arryRowSumOpp[2], calc.TilesOpp(calc.tileDataOpps3, out Count, skipClear: true));
            }
            if (arryRowSumMe != null && arryRowSumMe.Length >= 3)
            {
                SetRowSumText(arryRowSumMe[0], calc.TileMe(calc.clickInCells, out Count, skipClear: true));
                SetRowSumText(arryRowSumMe[1], calc.TileMe(calc.clickInCells1, out Count, skipClear: true));
                SetRowSumText(arryRowSumMe[2], calc.TileMe(calc.clickInCells2, out Count, skipClear: true));
            }

            // Columns (activate column-match particles — without stopping row matches)
            if (arryRowSumOppCal != null && arryRowSumOppCal.Length >= 3)
            {
                SetRowSumText(arryRowSumOppCal[0], calc.TilesOpp(calc.tileDataOppsCal, out Count, skipClear: true));
                SetRowSumText(arryRowSumOppCal[1], calc.TilesOpp(calc.tileDataOpps2Cal, out Count, skipClear: true));
                SetRowSumText(arryRowSumOppCal[2], calc.TilesOpp(calc.tileDataOpps3Cal, out Count, skipClear: true));
            }
            if (arryRowSumMeCal != null && arryRowSumMeCal.Length >= 3)
            {
                SetRowSumText(arryRowSumMeCal[0], calc.TileMe(calc.clickInCellsCal, out Count, skipClear: true));
                SetRowSumText(arryRowSumMeCal[1], calc.TileMe(calc.clickInCells1Cal, out Count, skipClear: true));
                SetRowSumText(arryRowSumMeCal[2], calc.TileMe(calc.clickInCells2Cal, out Count, skipClear: true));
            }
        }
        else
        {
            // Normal modes: each row owns its cells exclusively — no overlap, no issue.
            if (arryRowSumOpp != null && arryRowSumOpp.Length >= 4)
            {
                SetRowSumText(arryRowSumOpp[0], calc.TilesOpp(calc.tileDataOpps, out Count));
                SetRowSumText(arryRowSumOpp[1], calc.TilesOpp(calc.tileDataOpps2, out Count));
                SetRowSumText(arryRowSumOpp[2], calc.TilesOpp(calc.tileDataOpps3, out Count));
                SetRowSumText(arryRowSumOpp[3], calc.TilesOpp(calc.tileDataOpps4, out Count));
            }
            if (arryRowSumMe != null && arryRowSumMe.Length >= 4)
            {
                SetRowSumText(arryRowSumMe[0], calc.TileMe(calc.clickInCells, out Count));
                SetRowSumText(arryRowSumMe[1], calc.TileMe(calc.clickInCells1, out Count));
                SetRowSumText(arryRowSumMe[2], calc.TileMe(calc.clickInCells2, out Count));
                SetRowSumText(arryRowSumMe[3], calc.TileMe(calc.clickInCells3, out Count));
            }
        }

        CheckShowLight();
    }

    private void Instance_onSetDataInRowOpp(int arg1, int arg2)
    {
        var clone = tileDataOpps.Find(e => e.line == arg1 && e.row == arg2 && e.IsLock);
        if (clone != null)
        {
            clone.ValueTile = 0;
            clone.IsLock = false;
            var ps = clone.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Stop();
            ParticleSystem.MainModule settings = ps != null ? ps.main : default;
            if (ps != null) settings.startColor = new ParticleSystem.MinMaxGradient(colroParticlewhite);

            Transform tileParent = clone.SpriteDice != null ? clone.SpriteDice.transform.parent : null;
            if (tileParent != null)
            {
                tileParent.DOKill();
                tileParent.DOShakePosition(0.2f, 12f, 22, 90f, false, true).SetUpdate(true)
                    .OnComplete(() =>
                    {
                        tileParent.DOScale(Vector3.zero, 0.18f).SetEase(Ease.InBack).SetUpdate(true)
                            .OnComplete(() =>
                            {
                                if (clone.SpriteDice != null) clone.SpriteDice.sprite = null;
                                tileParent.gameObject.SetActive(false);
                                tileParent.localScale = Vector3.one;
                                tileParent.localPosition = Vector3.zero;
                            });
                    });
            }
            else
            {
                if (clone.SpriteDice != null) clone.SpriteDice.sprite = null;
            }
        }
        RowSum();
    }

    /// <summary>
    /// Set data in row 
    /// </summary>
    private void Instance_onSetDataInRowMe(int arg1, int arg2)
    {
        var meCell = tileDataMe.Find(r => r.numberLine == arg1 && r.numberRow == arg2 && r.isLock);
        if (meCell != null)
        {
            TutorialManager.Instance?.OnEliminationOccurred(arg1, arg2);

            meCell.ValueTile = 0;
            meCell.isLock = false;
            var ps = meCell.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Stop();
            ParticleSystem.MainModule settings = ps != null ? ps.main : default;
            if (ps != null) settings.startColor = new ParticleSystem.MinMaxGradient(colroParticlewhite);

            Transform tileParent = meCell.SpriteDice != null ? meCell.SpriteDice.transform.parent : null;
            if (tileParent != null)
            {
                tileParent.DOKill();
                tileParent.DOShakePosition(0.2f, 12f, 22, 90f, false, true).SetUpdate(true)
                    .OnComplete(() =>
                    {
                        tileParent.DOScale(Vector3.zero, 0.18f).SetEase(Ease.InBack).SetUpdate(true)
                            .OnComplete(() =>
                            {
                                if (meCell.SpriteDice != null) meCell.SpriteDice.sprite = null;
                                tileParent.gameObject.SetActive(false);
                                tileParent.localScale = Vector3.one;
                                tileParent.localPosition = Vector3.zero;
                            });
                    });
            }
            else
            {
                if (meCell.SpriteDice != null) meCell.SpriteDice.sprite = null;
            }
        }
        RowSum();
    }
    /// <summary>
    /// check data in turn player
    /// </summary>
    /// <param name="obj"></param>
    private void Instance_SetDataInTurn(DataPlayer obj)
    {
        if (obj.UserId != MultiplayerManager.Instance.players.User.Id)
        {
            ApplyBotMove(obj);
        }
        else
        {
            if (!obj.EndGame)
                TextTurnOpp.Play("OppTurn", 0, 0);
            RowSum();
            TutorialManager.Instance?.OnCellPlaced(obj.NumberLine, obj.NumberRow);

            TimerTurn.instance.TimerRunning = false;
            TimerTurn.instance.TimerText.text = "-";
        }
    }

    private void ApplyBotMove(DataPlayer obj)
    {
        Debug.Log($"[Tutorial] ApplyBotMove: line={obj.NumberLine}, row={obj.NumberRow}, tile={obj.NumberTile}, name={obj.NameTile}, oppList={tileDataOpps?.Count}");
        GameManager.Instance.diceRoller.Rotation(false);

        _ = Task.Delay(200);
        GameManager.Instance.diceRoller.GetComponent<Image>().sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
        GameManager.Instance.diceRoller.currrentDie = -1;
        var tile = tileDataOpps.Find(t => t.line == obj.NumberLine && t.row == obj.NumberRow);
        if (tile == null) tile = transformOpp.Find(obj.NameTile)?.GetComponentInChildren<TileDataOpp>();
        if (tile == null)
        {
            Debug.LogWarning($"[Tutorial] Bot tile not found! line={obj.NumberLine} row={obj.NumberRow} name={obj.NameTile}. Available cells:");
            foreach (var t in tileDataOpps) Debug.LogWarning($"  cell line={t.line}, row={t.row}");
            // Tutorial must not stay stuck — release the wait flag and proceed
            TutorialManager.Instance?.OnBotMovePlayed();
            return;
        }
        tile.IsLock = true;
        Transform oppTileParent = tile.SpriteDice.transform.parent;
        oppTileParent.gameObject.SetActive(true);
        oppTileParent.DOKill();
        oppTileParent.localScale = Vector3.zero;
        oppTileParent.DOScale(Vector3.one, 0.28f).SetEase(Ease.OutBack).SetUpdate(true);

        tile.SpriteDice.GetComponent<Animator>().Play("DiceRoot", 0, 0);
        tile.ValueTile = obj.NumberTile + 1;
        tile.SpriteDice.sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
        if (!obj.EndGame)
            TextTurnYou.Play("YouTurn", 0, 0);
        TimerTurn.instance.TimerRunning = true;
        TimerTurn.instance.TimerPause = false;
        TimerTurn.instance.TimerText.text = "30";
        TimerTurn.instance.TimerCount = 30;
        TimerTurn.instance.TimerText.color = Color.white;

        // Fly bubble: bot's dice value travels to the opponent grid.
        TutorialManager.Instance?.ShowDiceFlyBubble(obj.NumberTile + 1, isLocalMove: false);

        RowSum();

        // Notify tutorial that bot move is now visible on screen
        TutorialManager.Instance?.OnBotMovePlayed();
    }

    private void CheckShowLight()
    {
        Debug.Log(CalculterRowScore.instance.DuobleScore1.Count + " Count");
        Debug.Log(CalculterRowScore.instance.DuobleScore2.Count + " Count 2");


        CalculterRowScore.instance.DuobleScore2.Clear();
        CalculterRowScore.instance.DuobleScore1.Clear();
    }

    private void Instance_IsTurn(bool obj)
    {
        if (obj)
        {
            GameManager.Instance.diceRoller.PrepareForTurn();
            PlayYourTurnEntrance();
            MultiplayerManager.Instance.isTurn = true;
            _ = Task.Delay(1000);
            TimerTurn.instance.TimerPause = false;
            TimerTurn.instance.TimerRunning = true;
        }
        else
        {
            PlayOpponentTurnEntrance();
            MultiplayerManager.Instance.isTurn = false;
            TimerTurn.instance.TimerPause = false;
            TimerTurn.instance.TimerRunning = false;
            TutorialManager.Instance?.OnOpponentTurnStarted();
        }
    }

    private void Instance_onTurnMe()
    {
        GameManager.Instance.diceRoller.PrepareForTurn();
        PlayYourTurnEntrance();
    }

    private void PlayYourTurnEntrance()
    {
        if (dicRollButton != null)
        {
            dicRollButton.interactable = true;
            if (DiceRollsSprite != null && DiceRollsSprite.Length > 0)
                dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];

            dicRollButton.transform.DOKill();
            dicRollButton.transform.localScale = Vector3.one * 0.72f;
            dicRollButton.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        if (TextTurnYou != null)
        {
            TextTurnYou.Play("YouTurn", 0, 0);
            TextTurnYou.transform.DOKill();
            TextTurnYou.transform.localScale = Vector3.one;
            TextTurnYou.transform.DOPunchScale(new Vector3(0.35f, 0.35f, 0f), 0.42f, 6, 0.5f)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        if (WowPar != null)
        {
            WowPar.Play();
        }

        if (GameManager.Instance != null && GameManager.Instance.diceRoller != null)
        {
            GameManager.Instance.diceRoller.Rotation(false);
        }

        StartRollAttention();
    }

    private void PlayOpponentTurnEntrance()
    {
        StopRollAttention();
        if (WowPar != null && WowPar.isPlaying)
            WowPar.Stop();

        if (TextTurnOpp != null)
        {
            TextTurnOpp.Play("OppTurn", 0, 0);
            TextTurnOpp.transform.DOKill();
            TextTurnOpp.transform.localScale = Vector3.one;
            TextTurnOpp.transform.DOPunchScale(new Vector3(0.20f, 0.20f, 0f), 0.35f, 5, 0.5f)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        if (dicRollButton != null)
        {
            if (DiceRollsSprite != null && DiceRollsSprite.Length > 1)
                dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];
            dicRollButton.interactable = false;
        }
    }

    private void StartRollAttention()
    {
        if (dicRollButton == null || !dicRollButton.interactable)
            return;

        if (rollAttentionTarget == null)
        {
            rollAttentionTarget = dicRollButton.transform.parent as RectTransform;
            if (rollAttentionTarget == null)
                return;

            rollAttentionBaseScale = rollAttentionTarget.localScale;
            rollAttentionBaseEuler = rollAttentionTarget.localEulerAngles;
        }

        StopRollAttention();
        rollAttentionSequence = DOTween.Sequence()
            .Append(rollAttentionTarget.DOScale(rollAttentionBaseScale * 1.09f, 0.38f).SetEase(Ease.OutBack))
            .Join(rollAttentionTarget.DOLocalRotate(rollAttentionBaseEuler + new Vector3(0f, 0f, -3.5f), 0.38f)
                .SetEase(Ease.OutSine))
            .Append(rollAttentionTarget.DOScale(rollAttentionBaseScale * 0.98f, 0.22f).SetEase(Ease.InOutSine))
            .Join(rollAttentionTarget.DOLocalRotate(rollAttentionBaseEuler + new Vector3(0f, 0f, 3.5f), 0.22f)
                .SetEase(Ease.InOutSine))
            .Append(rollAttentionTarget.DOScale(rollAttentionBaseScale, 0.25f).SetEase(Ease.OutQuad))
            .Join(rollAttentionTarget.DOLocalRotate(rollAttentionBaseEuler, 0.25f).SetEase(Ease.OutQuad))
            .AppendInterval(0.45f)
            .SetLoops(-1)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void StopRollAttention()
    {
        if (rollAttentionSequence != null)
        {
            rollAttentionSequence.Kill(false);
            rollAttentionSequence = null;
        }

        if (rollAttentionTarget != null)
        {
            rollAttentionTarget.localScale = rollAttentionBaseScale;
            rollAttentionTarget.localEulerAngles = rollAttentionBaseEuler;
        }
    }

    public void Leave()
    {
        MultiplayerManager.Instance.LeaveMatchAsync();
    }


}
