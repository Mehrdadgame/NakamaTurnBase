
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Nakama.Helpers;
using System.Drawing;
using Color = UnityEngine.Color;
using UnityEngine.Tilemaps;

public class CalculterRowScore : MonoBehaviour
{
    public static CalculterRowScore instance;
    public List<TileDataOpp> tileDataOpps = new List<TileDataOpp>();
    public List<TileDataOpp> tileDataOpps2 = new List<TileDataOpp>();
    public List<TileDataOpp> tileDataOpps3 = new List<TileDataOpp>();
    public List<TileDataOpp> tileDataOpps4 = new List<TileDataOpp>();

    public List<ClickInCell> clickInCells = new List<ClickInCell>();
    public List<ClickInCell> clickInCells1 = new List<ClickInCell>();
    public List<ClickInCell> clickInCells2 = new List<ClickInCell>();
    public List<ClickInCell> clickInCells3 = new List<ClickInCell>();

    public List<TileDataOpp> tileDataOppsCal = new List<TileDataOpp>();
    public List<TileDataOpp> tileDataOpps2Cal = new List<TileDataOpp>();
    public List<TileDataOpp> tileDataOpps3Cal = new List<TileDataOpp>();

    public List<ClickInCell> clickInCellsCal = new List<ClickInCell>();
    public List<ClickInCell> clickInCells1Cal = new List<ClickInCell>();
    public List<ClickInCell> clickInCells2Cal = new List<ClickInCell>();

    public List<SaveShowLight> DuobleScore1 = new();
    public List<SaveShowLight> DuobleScore2 = new();
    public Color colorParticle2Count;
    public Color colorParticle3Count;
    public Color colorParticle4Count;
    public Color whitecolor;

    private void Start()
    {
        instance = this;
    }

    private Color SetColorParticle(int a)
    {
        switch (a)
        {
            case 2: return colorParticle2Count;
            case 3: return colorParticle3Count;
            case 4: return colorParticle4Count;
            default:
                break;
        }
        return whitecolor;
    }
    // Call this before RowSum in VerticalAndHorizontal mode to stop all particles at once
    public void StopParticlesOpp(List<TileDataOpp> cell)
    {
        foreach (var item in cell)
            StopMatchParticle(item.GetComponentInChildren<ParticleSystem>());
    }

    public void StopParticlesMe(List<ClickInCell> cell)
    {
        foreach (var item in cell)
            StopMatchParticle(item.GetComponentInChildren<ParticleSystem>());
    }

    // skipClear: true = don't stop particles first (used when caller already cleared them)
    public int TilesOpp(List<TileDataOpp> cell, out int count, bool skipClear = false)
    {
        //  DuobleScore2.Clear();
        var total = 0;
        var freqMap = cell.GroupBy(x => x.ValueTile)
                                                    .Where(g => g.Count() > 1).Where(r => r.Key > 0)
                                                    .ToDictionary(x => x.Key, x => x.Count());

        if (!skipClear)
        {
            foreach (var item in cell)
                StopMatchParticle(item.GetComponentInChildren<ParticleSystem>());
        }

        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());
        foreach (var item in placeCell)
        {
            var lockedCells = item.Value.Where(c => c.IsLock).ToArray();
            for (int i = 0; i < lockedCells.Length; i++)
            {
                PlayMatchParticle(lockedCells[i].GetComponentInChildren<ParticleSystem>(), lockedCells.Length);

                SaveShowLight show = new()
                {
                    line = lockedCells[i].line,
                    row = lockedCells[i].row
                };
                if (!DuobleScore2.Contains(show))
                {
                    DuobleScore2.Add(show);
                    show.countInLine++;
                }
            }

        }

        foreach (var item in freqMap)
        {


            if (item.Value > 3)
            {
                count = 4;
                return item.Key * 16;

            }
            else if (item.Value == 3)
            {
                count = 3;
                var dif = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() == 1).ToArray();

                total = item.Key * 9;
                var totalDif = 0;
                if (dif.Length > 0)
                {
                    totalDif = dif[0].Key;

                }

                return total + totalDif;

            }
            else if (item.Value == 2)
            {
                var dif = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() == 1).ToArray();
                var same = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() == 2).Where(t => t.Key != item.Key).ToArray();

                count = 2;
                total += item.Key * 4;

                if (dif.Length > 0)
                {
                    var totalDif = 0;
                    for (int i = 0; i < dif.Length; i++)
                    {
                        totalDif += dif[i].Key;
                    }
                    return total + totalDif;
                }
                else if (same.Length > 0)
                {

                    total += same[0].Key * 4;

                }

                return total;
            }


        }
        count = 1;
        return cell.Where(r => r.ValueTile > -1).Sum(c => c.ValueTile);

    }

    public int TileMe(List<ClickInCell> cell, out int count, bool skipClear = false)
    {
        var total = 0;
        // DuobleScore1.Clear();
        Dictionary<int, int> freqMap = cell.GroupBy(x => x.ValueTile)
                                           .Where(g => g.Count() > 1).Where(r => r.Key > 0)
                                            .ToDictionary(x => x.Key, x => x.Count());

        if (!skipClear)
        {
            foreach (var itemm in cell)
                StopMatchParticle(itemm.GetComponentInChildren<ParticleSystem>());
        }


        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());

        foreach (var item in placeCell)
        {
            var lockedCells = item.Value.Where(c => c.isLock).ToArray();
            for (int i = 0; i < lockedCells.Length; i++)
            {
                PlayMatchParticle(lockedCells[i].GetComponentInChildren<ParticleSystem>(), lockedCells.Length);
                SaveShowLight show = new()
                {
                    line = lockedCells[i].numberLine,
                    row = lockedCells[i].numberRow,
                };
                if (!DuobleScore1.Contains(show))
                {
                    DuobleScore1.Add(show);
                    show.countInLine++;
                }
            }

        }


        foreach (var item in freqMap)
        {


            if (item.Value > 3)
            {
                count = 4;
                return item.Key * 16;
            }
            else if (item.Value == 3)
            {
                var dif = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() == 1).ToArray();
                count = 3;
                total = item.Key * 9;
                var totalDif = 0;
                if (dif.Length > 0)
                {
                    totalDif = dif[0].Key;

                }

                return total + totalDif;

            }
            else if (item.Value == 2)
            {
                var dif = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() == 1).ToArray();
                var same = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() == 2).Where(t => t.Key != item.Key).ToArray();


                total += item.Key * 4;
                count = 2;
                if (dif.Length > 0)
                {
                    var totalDif = 0;
                    for (int i = 0; i < dif.Length; i++)
                    {
                        totalDif += dif[i].Key;
                    }
                    return total + totalDif;
                }
                else if (same.Length > 0)
                {

                    total += same[0].Key * 4;

                }

                return total;
            }


        }
        count = 1;
        return cell.Where(r => r.ValueTile > -1).Sum(c => c.ValueTile);
    }

    private void StopMatchParticle(ParticleSystem particle)
    {
        if (particle == null)
            return;

        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule settings = particle.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(whitecolor);
    }

    private void PlayMatchParticle(ParticleSystem particle, int matchCount)
    {
        if (particle == null || matchCount < 2)
            return;

        // Existing availability particles are white. Clear them before changing
        // color so a double/triple starts immediately with its match color.
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule settings = particle.main;
        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(matchCount));
        particle.Play(true);

        RegisterMatchForAudio(particle.GetEntityId().ToString(), matchCount);
    }

    // ── Match audio (new matches only) ─────────────────────────────────────────
    // RowSum replays every existing match particle on every recalculation, so the
    // sound must fire only for cells that were NOT matched in the previous pass.

    private readonly HashSet<string> _matchedNow = new();
    private HashSet<string> _matchedPrev = new();
    private int _newMatchBest;

    private void RegisterMatchForAudio(string key, int matchCount)
    {
        _matchedNow.Add(key);
        if (!_matchedPrev.Contains(key))
            _newMatchBest = Mathf.Max(_newMatchBest, matchCount);
    }

    /// <summary>Called by UiManager once per full RowSum pass.</summary>
    public void CommitMatchAudio()
    {
        if (_newMatchBest >= 2)
            NinjaBattle.General.GameSfx.PlayMatch(_newMatchBest);

        _matchedPrev.Clear();
        _matchedPrev.UnionWith(_matchedNow);
        _matchedNow.Clear();
        _newMatchBest = 0;
    }
}
