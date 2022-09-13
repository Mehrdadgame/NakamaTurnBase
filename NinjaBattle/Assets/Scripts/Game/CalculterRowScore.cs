using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class CalculterRowScore : MonoBehaviour
{

    [SerializeField] private List< TileDataOpp> tileDataOpps = new List<TileDataOpp>();
    [SerializeField]private List< ClickInCell> clickInCells = new List<ClickInCell>();
    public int sum;

    private int tilesOpp()
    {
        tileDataOpps.Sort();
        int count = 0;
        int key = 0;
        var e = tileDataOpps.GroupBy(x => x.ValueTile);
        foreach (var item in e)
        {
            foreach (var item1 in item)
            {
                key = item1.ValueTile;
                count++;
            }
        }

        if (count > 0)
        {
            return (key * count) * count;
        }
        return sum+= key;

    }
}
