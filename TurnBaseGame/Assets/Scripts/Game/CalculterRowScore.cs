
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Nakama.Helpers;


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

    public int TilesOpp(List<TileDataOpp> cell)
    {
        //  DuobleScore2.Clear();
        var total = 0;
        var freqMap = cell.GroupBy(x => x.ValueTile)
                                                    .Where(g => g.Count() > 1).Where(r => r.Key > 0)
                                                    .ToDictionary(x => x.Key, x => x.Count());


        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());
        foreach (var item in placeCell)
        {
            foreach (var par in item.Value)
            {
                if (par.IsLock)
                {
                    SaveShowLight show = new()
                    {
                        line = par.line,
                        row = par.row

                    };
                    if (!DuobleScore2.Contains(show))
                    {
                        DuobleScore2.Add(show);
                        show.countInLine++;
                    }
                   


                    Debug.Log($"{show.line}{show.row}  DuobleScore2");
                    //par.GetComponentInChildren<ParticleSystem>().Play();
                    //ParticleSystem.MainModule settings = par.GetComponentInChildren<ParticleSystem>().main;
                    //settings.startColor = new ParticleSystem.MinMaxGradient(colorParticle);

                }


            }

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

    public int TileMe(List<ClickInCell> cell)
    {

        var total = 0;
        // DuobleScore1.Clear();
        Dictionary<int, int> freqMap = cell.GroupBy(x => x.ValueTile)
                                           .Where(g => g.Count() > 1).Where(r => r.Key > 0)
                                            .ToDictionary(x => x.Key, x => x.Count());


        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());


        foreach (var item in placeCell)
        {

            foreach (var par in item.Value)
            {
                if (par.isLock == true)
                {
                    SaveShowLight show = new()
                    {
                        line = par.numberLine,
                        row = par.numberRow,
                    };
                    if (!DuobleScore2.Contains(show))
                    {
                        DuobleScore2.Add(show);
                        show.countInLine++;
                    }
                  
                    Debug.Log($"{show.line}{show.row} DuobleScore1");
                    //par.GetComponentInChildren<ParticleSystem>().Play();
                    //ParticleSystem.MainModule settings = par.GetComponentInChildren<ParticleSystem>().main;
                    //settings.startColor = new ParticleSystem.MinMaxGradient(colorParticle);
                }

            }

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
