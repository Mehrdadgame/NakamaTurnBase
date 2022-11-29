using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IMonobaihaverButtonGameMode
{
    public ModeGame ModeGame { get; set; }
    public Dictionary<Button, int> Buttons_THT_Coin { get; set; }
    public List<Button> ButtonsModeGame { get; set; }
    public GameObject BackgroundGameModePage { get; set; }
    public void ActionGameMode();


}
