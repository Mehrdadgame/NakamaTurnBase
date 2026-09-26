var canvas = UnityEngine.Object.FindAnyObjectByType<UnityEngine.Canvas>();
var bg = canvas.transform.Find("MissionProgressionUI/Background") as UnityEngine.RectTransform;
if (bg != null)
{
    bg.pivot = new UnityEngine.Vector2(0.5f, 0.5f);
    bg.localScale = UnityEngine.Vector3.one;
}
return "Reset background";
