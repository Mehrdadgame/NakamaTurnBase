using Nakama;
using Nakama.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickToStickers : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string nameSticker;

    public void OnPointerClick(PointerEventData eventData)
    {
        nameSticker = GetComponent<Image>().sprite.name;
        UiManager.instance.StickerShow.GetComponent<Image>().sprite = UiManager.instance.AllAssets.GetSprite(nameSticker);
       // UiManager.instance.StickerShow.GetComponent<Image>().SetNativeSize();
        UiManager.instance.StickerShow.Play("showSticker", 0, 0);
        StickerData stickerData = new() { StickerName = nameSticker, ID = MultiplayerManager.Instance.Self.UserId };
        MultiplayerManager.Instance.Send(MultiplayerManager.Code.SendSticker, stickerData);
    }

  


}

public class StickerData
{
    public string ID;
    public string StickerName;
}
