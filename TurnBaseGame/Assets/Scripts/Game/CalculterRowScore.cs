
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Drawing.Imaging;

public class CalculterRowScore : MonoBehaviour
{
    public static CalculterRowScore instance;
    public List<TileDataOpp> tileDataOpps = new List<TileDataOpp>();
    public List<TileDataOpp> tileDataOpps2 = new List<TileDataOpp>();
    public List<TileDataOpp> tileDataOpps3 = new List<TileDataOpp>();
    public List<ClickInCell> clickInCells = new List<ClickInCell>();
    public List<ClickInCell> clickInCells1 = new List<ClickInCell>();
    public List<ClickInCell> clickInCells2 = new List<ClickInCell>();


    public Color colorParticle;
    private void Start()
    {
        instance = this;
    }

    public int TilesOpp(List<TileDataOpp> cell)
    {
        var total = 0;

        Dictionary<int, int> freqMap = cell.GroupBy(x => x.ValueTile)
                                            .Where(g => g.Count() > 1)
                                            .ToDictionary(x => x.Key, x => x.Count());


        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());
        foreach (var item in placeCell)
        {

            foreach (var par in item.Value)
            {
                if (par.ValueTile > 0)
                {
                    par.GetComponentInChildren<ParticleSystem>().Play();
                    ParticleSystem.MainModule settings = par.GetComponentInChildren<ParticleSystem>().main;
                    settings.startColor = new ParticleSystem.MinMaxGradient(colorParticle);

                }
            }

        }

        foreach (var item in freqMap)
        {


            if (item.Value > 2)
            {
                return item.Key * 9;
            }
            else if (item.Value == 2)
            {

                var dif = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() == 1).ToArray();

                total = item.Key * 4;


                return total + dif[0].Key;

            }


        }
        return cell.Sum(c => c.ValueTile);

    }

    public int TileMe(List<ClickInCell> cell)
    {
        var total = 0;

        Dictionary<int, int> freqMap = cell.GroupBy(x => x.ValueTile)
                                            .Where(g => g.Count() > 1)
                                            .ToDictionary(x => x.Key, x => x.Count());

        var placeCell = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() > 1).ToDictionary(x => x.Key, x => x.ToArray());
        foreach (var item in placeCell)
        {

            foreach (var par in item.Value)
            {
                if (par.isLock == true)
                {
                    par.GetComponentInChildren<ParticleSystem>().Play();
                    ParticleSystem.MainModule settings = par.GetComponentInChildren<ParticleSystem>().main;
                    settings.startColor = new ParticleSystem.MinMaxGradient(colorParticle);
                }
            }

        }


        foreach (var item in freqMap)
        {

            if (item.Value > 2)
            {
                return item.Key * 9;

            }
            else if (item.Value == 2)
            {
                var dif = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() == 1).ToArray();

                total = item.Key * 4;

                return total + dif[0].Key;

            }


        }

        return cell.Sum(c => c.ValueTile);




    }
}
