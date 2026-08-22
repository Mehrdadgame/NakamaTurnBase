// FigmaModels.cs
// مدل‌های داده برای پاسخ‌های Figma REST API
// نیازمند پکیج: com.unity.nuget.newtonsoft-json
// محل قرارگیری: Assets/Editor/Figma/

using System.Collections.Generic;
using UnityEngine;

namespace FigmaImport
{
    // ---------- پاسخ‌های سطح بالا ----------

    public class FigmaFileResponse
    {
        public string name;
        public string lastModified;
        public FigmaNode document;
    }

    public class FigmaNodesResponse
    {
        public string name;
        public Dictionary<string, FigmaNodeEntry> nodes;
    }

    public class FigmaNodeEntry
    {
        public FigmaNode document;
    }

    public class FigmaImagesResponse
    {
        public string err;
        // کلید = node id ، مقدار = URL موقت تصویر (حدود ۳۰ دقیقه معتبر است)
        public Dictionary<string, string> images;
    }

    // ---------- ساختارهای کمکی ----------

    public class FigmaRect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public FigmaRect Clone() => new FigmaRect { x = x, y = y, width = width, height = height };
    }

    public class FigmaColor
    {
        public float r;
        public float g;
        public float b;
        public float a = 1f;

        public Color ToUnity(float extraOpacity = 1f)
        {
            return new Color(r, g, b, a * extraOpacity);
        }
    }

    public class FigmaColorStop
    {
        public float position;
        public FigmaColor color;
    }

    public class FigmaPaint
    {
        public string type;          // SOLID | IMAGE | GRADIENT_LINEAR | GRADIENT_RADIAL | ...
        public bool visible = true;
        public float opacity = 1f;
        public FigmaColor color;     // فقط برای SOLID
        public string imageRef;      // فقط برای IMAGE
        public string scaleMode;     // FILL | FIT | TILE | STRETCH
        public List<FigmaColorStop> gradientStops;
    }

    public class FigmaTypeStyle
    {
        public string fontFamily;
        public string fontPostScriptName;
        public float fontSize = 16f;
        public int fontWeight = 400;
        public bool italic;
        public string textAlignHorizontal = "LEFT";   // LEFT | CENTER | RIGHT | JUSTIFIED
        public string textAlignVertical = "TOP";      // TOP | CENTER | BOTTOM
        public string textCase;                       // UPPER | LOWER | TITLE
        public float letterSpacing;
        public float lineHeightPx;
        public float lineHeightPercent = 100f;
    }

    public class FigmaEffect
    {
        public string type;          // DROP_SHADOW | INNER_SHADOW | LAYER_BLUR | ...
        public bool visible = true;
        public FigmaColor color;
        public float radius;
    }

    // ---------- نود ----------

    public class FigmaNode
    {
        public string id;
        public string name;
        public string type;          // DOCUMENT | CANVAS | FRAME | GROUP | COMPONENT | INSTANCE
                                     // | RECTANGLE | ELLIPSE | VECTOR | TEXT | LINE | ...
        public bool visible = true;
        public float opacity = 1f;

        public FigmaRect absoluteBoundingBox;

        public List<FigmaPaint> fills;
        public List<FigmaPaint> strokes;
        public float strokeWeight;

        public float cornerRadius;
        public List<float> rectangleCornerRadii;   // [TL, TR, BR, BL]

        public bool clipsContent;

        // فقط برای TEXT
        public string characters;
        public FigmaTypeStyle style;

        public List<FigmaEffect> effects;

        public List<FigmaNode> children;

        // ---------- کمکی‌ها ----------

        public FigmaPaint FirstVisibleFill()
        {
            if (fills == null) return null;
            foreach (var f in fills)
                if (f != null && f.visible && f.opacity > 0.001f) return f;
            return null;
        }

        public bool HasImageFill()
        {
            if (fills == null) return false;
            foreach (var f in fills)
                if (f != null && f.visible && f.type == "IMAGE") return true;
            return false;
        }

        public bool HasGradientFill()
        {
            if (fills == null) return false;
            foreach (var f in fills)
                if (f != null && f.visible && !string.IsNullOrEmpty(f.type) && f.type.StartsWith("GRADIENT"))
                    return true;
            return false;
        }

        public bool HasVisibleStroke()
        {
            if (strokes == null || strokeWeight <= 0f) return false;
            foreach (var s in strokes)
                if (s != null && s.visible && s.opacity > 0.001f) return true;
            return false;
        }

        public bool IsContainer()
        {
            return type == "FRAME" || type == "GROUP" || type == "COMPONENT"
                || type == "COMPONENT_SET" || type == "INSTANCE" || type == "SECTION"
                || type == "CANVAS";
        }
    }
}
