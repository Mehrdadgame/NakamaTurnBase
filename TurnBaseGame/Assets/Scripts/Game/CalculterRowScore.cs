
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
    public int TilesOpp(List<TileDataOpp> cell , out int count)
    {
        //  DuobleScore2.Clear();
        var total = 0;
        var freqMap = cell.GroupBy(x => x.ValueTile)
                                                    .Where(g => g.Count() > 1).Where(r => r.Key > 0)
                                                    .ToDictionary(x => x.Key, x => x.Count());



        foreach (var item in cell)
        {
            item.GetComponentInChildren<ParticleSystem>().Stop();
            ParticleSystem.MainModule settings = item.GetComponentInChildren<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(CalculterRowScore.instance.whitecolor);
        }
       
        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());
        foreach (var item in placeCell)
        {
            for (int i = 0; i < item.Value.Length; i++)
            {
                if (item.Value[i].IsLock)
                {

                    item.Value[i].GetComponentInChildren<ParticleSystem>().Play();
                    ParticleSystem.MainModule settings = item.Value[i].GetComponentInChildren<ParticleSystem>().main;
                    if (item.Value.Length==4)
                    {
                       
                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(4));
                    
                    }
                    if (item.Value.Length == 3)
                    {
                        
                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(3));

                    }
                    if (item.Value.Length == 2)
                    {
                    
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

        foreach (var item in freqMap)
        {


            if (item.Value > 3)
            {
                count = 4;
                return item.Key * 16 ;

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

    public int TileMe(List<ClickInCell> cell , out int count)
    {

        var total = 0;
        // DuobleScore1.Clear();
        Dictionary<int, int> freqMap = cell.GroupBy(x => x.ValueTile)
                                           .Where(g => g.Count() > 1).Where(r => r.Key > 0)
                                            .ToDictionary(x => x.Key, x => x.Count());
        foreach (var itemm in cell)
        {
            itemm.GetComponentInChildren<ParticleSystem>().Stop();
            ParticleSystem.MainModule settings = itemm.GetComponentInChildren<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(whitecolor);
        }


        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());


        foreach (var item in placeCell)
        {

            for (int i = 0; i < item.Value.Length; i++)
            {
                if (item.Value[i].isLock)
                {

                    item.Value[i].GetComponentInChildren<ParticleSystem>().Play();
                    ParticleSystem.MainModule settings = item.Value[i].GetComponentInChildren<ParticleSystem>().main;
                    if (item.Value.Length == 4)
                    {

                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(4));

                    }
                    if (item.Value.Length == 3)
                    {

                        settings.startColor = new ParticleSystem.MinMaxGradient(SetColorParticle(3));

                    }
                    if (item.Value.Length == 2)
                    {

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
}
