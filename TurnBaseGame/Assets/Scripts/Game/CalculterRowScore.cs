
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Color = UnityEngine.Color;


/* A class declaration. */
public class CalculterRowScore : MonoBehaviour
{

    #region property
    public static CalculterRowScore instance;
    public List<TileDataOpp> tileDataOpps = new();
    public List<TileDataOpp> tileDataOpps2 = new();
    public List<TileDataOpp> tileDataOpps3 = new();
    public List<TileDataOpp> tileDataOpps4 = new();

    public List<ClickInCell> clickInCells = new();
    public List<ClickInCell> clickInCells1 = new();
    public List<ClickInCell> clickInCells2 = new();
    public List<ClickInCell> clickInCells3 = new();

    public List<TileDataOpp> tileDataOppsCal = new();
    public List<TileDataOpp> tileDataOpps2Cal = new();
    public List<TileDataOpp> tileDataOpps3Cal = new();

    public List<ClickInCell> clickInCellsCal = new();
    public List<ClickInCell> clickInCells1Cal = new();
    public List<ClickInCell> clickInCells2Cal = new();

    public List<SaveShowLight> DuobleScore1 = new();
    public List<SaveShowLight> DuobleScore2 = new();
    public Color colorParticle2Count;
    public Color colorParticle3Count;
    public Color colorParticle4Count;
    public Color whitecolor;

    public ParticleSystem Fire;
    private bool IsPluseScoreMe;
    private bool IsPluseScoreOpp;
    #endregion
    private void Start()
    {
        instance = this;
    }

    /// <summary>
    /// set color particel for double or triple dice
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    private Color SetColorParticle(int a)
    {
        switch (a)
        {
            case 2: return colorParticle2Count;
            case 3: return colorParticle3Count;
            case 4: return colorParticle4Count;
        }
        return whitecolor;
    }

/// <summary>
/// It returns the number of tiles in the list.
/// </summary>
/// <param name="cell">The list of tiles that are in the cell.</param>
    public int TilesOpp(List<TileDataOpp> cell)
    {
        var total = 0;
        var freqMap = cell.GroupBy(x => x.ValueTile)
                                                    .Where(g => g.Count() > 1).Where(r => r.Key > 0)
                                                     .ToDictionary(x => x.Key, x => x.Count());





        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());
        foreach (var item in placeCell)
        {
            for (int i = 0; i < item.Value.Length; i++)
            {
                if (item.Value[i].IsLock)
                {

                    item.Value[i].particleDouble.Play();
                    ParticleSystem.MainModule settings = item.Value[i].particleDouble.main;
                    if (item.Value.Length == 4)
                    {
                        if (item.Value[i].PluseScore == false)
                        {
                            item.Value[i].PluseScore = true;
                            IsPluseScoreOpp = true;
                        }
                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(4));
                        item.Value[i].PluseScore = true;
                    }
                    if (item.Value.Length == 3)
                    {
                        if (item.Value[i].PluseScore == false)
                        {
                            item.Value[i].PluseScore = true;
                            IsPluseScoreOpp = true;
                        }
                        item.Value[i].PluseScore = true;
                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(3));

                    }
                    if (item.Value.Length == 2)
                    {
                        if (item.Value[i].PluseScore == false)
                        {
                            item.Value[i].PluseScore = true;
                            IsPluseScoreOpp = true;

                        }
                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(2));

                    }
                    SaveShowLight show = new()
                    {
                        line = item.Value[i].line,
                        row = item.Value[i].row

                    };
                    if (!DuobleScore2.Contains(show))
                    {
                        DuobleScore2.Add(show);
                        show.countInLine++;
                    }

                }
            }

        }
        if (IsPluseScoreOpp)
        {
            Debug.Log("Opp Score");
            IsPluseScoreOpp = false;
        }
        foreach (var item in freqMap)
        {


            if (item.Value > 3)
            {

                return item.Key * 16;

            }
            else if (item.Value == 3)
            {

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

        return cell.Where(r => r.ValueTile > -1).Sum(c => c.ValueTile);

    }

/// <summary>
/// It takes a list of ClickInCell objects and returns an integer.
/// </summary>
/// <param name="cell">A list of ClickInCell objects. Each ClickInCell object represents a click in a
/// cell.</param>
    public int TileMe(List<ClickInCell> cell)
    {

        var total = 0;
        Dictionary<int, int> freqMap = cell.GroupBy(x => x.ValueTile)
                                           .Where(g => g.Count() > 1).Where(r => r.Key > 0)
                                            .ToDictionary(x => x.Key, x => x.Count());

        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());


        foreach (var item in placeCell)
        {

            for (int i = 0; i < item.Value.Length; i++)
            {
                if (item.Value[i].isLock)
                {

                    item.Value[i].particleDouble.Play();
                    ParticleSystem.MainModule settings = item.Value[i].particleDouble.main;
                    if (item.Value.Length == 4)
                    {
                        if (item.Value[i].PlusScore == false)
                        {
                            IsPluseScoreMe = true;
                            item.Value[i].PlusScore = true;
                            Fire.Play();
                        }
                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(4));

                    }
                    if (item.Value.Length == 3)
                    {
                        if (item.Value[i].PlusScore == false)
                        {
                            IsPluseScoreMe = true;
                            item.Value[i].PlusScore = true;
                            Fire.Play();

                        }
                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(3));

                    }
                    if (item.Value.Length == 2)
                    {
                        if (item.Value[i].PlusScore == false)
                        {
                            IsPluseScoreMe = true;
                            item.Value[i].PlusScore = true;
                        }
                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(2));

                    }
                    SaveShowLight show = new()
                    {
                        line = item.Value[i].numberLine,
                        row = item.Value[i].numberRow,

                    };
                    if (!DuobleScore1.Contains(show))
                    {
                        DuobleScore1.Add(show);
                        show.countInLine++;
                    }
                }

            }

        }
        if (IsPluseScoreMe)
        {
            Debug.Log("Score Me Plus");
            IsPluseScoreMe = false;
        }

        foreach (var item in freqMap)
        {


            if (item.Value > 3)
            {

                return item.Key * 16;
            }
            else if (item.Value == 3)
            {
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

        return cell.Where(r => r.ValueTile > -1).Sum(c => c.ValueTile);




    }
}
