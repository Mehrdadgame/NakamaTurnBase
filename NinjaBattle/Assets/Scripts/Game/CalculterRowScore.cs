using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CalculterRowScore : MonoBehaviour
{
    public static CalculterRowScore instance;
    public List<TileDataOpp> tileDataOpps = new List<TileDataOpp>();
    public List<TileDataOpp> tileDataOpps2 = new List<TileDataOpp>();
    public List<TileDataOpp> tileDataOpps3 = new List<TileDataOpp>();
    public List<ClickInCell> clickInCells = new List<ClickInCell>();
    public List<ClickInCell> clickInCells1 = new List<ClickInCell>();
    public List<ClickInCell> clickInCells2 = new List<ClickInCell>();

    private void Start()
    {
        instance = this;
    }

    public int TilesOpp(List<TileDataOpp> ts)
    {
        var total = 0;

        Dictionary<int, int> freqMap = ts.GroupBy(x => x.ValueTile).ToDictionary(x => x.Key, x => x.Count());
        for (int i = 0; i < ts.Count; i++)
        {
            Debug.Log("[Value, Count]: " + String.Join(",", freqMap) + ts[i].line + " " + ts[i].row);
        }
    
        foreach (var item in freqMap)
        {

            if (item.Value > 2)
            {

                return freqMap.Sum(c => c.Key) * 9;

            }
            else if (item.Value == 2)
            {
                var dif = ts.GroupBy(x => x.ValueTile).Where(g => g.Count() == 1).ToArray();

                total = item.Key * 4;
                Debug.Log(dif[0].Key + " 1 dif Me " + total + " Key" + item.Key);

                return total + dif[0].Key;

            }
            else
            {
                return freqMap.Sum(c => c.Key);

            }

        }
        return 0;

    }

    public int TileMe(List<ClickInCell> cell)
    {
        var total = 0;

        Dictionary<int, int> freqMap = cell.GroupBy(x => x.ValueTile).ToDictionary(x => x.Key, x => x.Count());


        foreach (var item in freqMap)
        {

            if (item.Value > 2 )
            {
               
                return item.Key * 9;

            }
            else if (item.Value == 2)
            {
                var dif = cell.GroupBy(x => x.ValueTile).Where(g => g.Count() == 1).ToArray();

                total = item.Key * 4;
                Debug.Log(dif[0].Key + " 1 dif Me "+ total + " Key"+ item.Key);

                return total + dif[0].Key;

            }
            else
            {
                return freqMap.Sum(c => c.Key);

            }

        }
        return 0;


    }
}
