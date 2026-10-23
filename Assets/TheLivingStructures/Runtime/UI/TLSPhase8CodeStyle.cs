using System;
using UnityEngine;

namespace TLS.UI
{
    /// <summary>Font fallback only. No downloaded font assets and no C source rewriting.</summary>
    public static class TLSPhase8CodeStyle
    {
        private static Font font;
        public static Font Font
        {
            get
            {
                if (font != null) return font;
                try { font = Font.CreateDynamicFontFromOSFont(
                    new[] { "Consolas", "Cascadia Mono", "Courier New", "Liberation Mono" }, 19); }
                catch (Exception) { font = null; }
                if (font == null) font = TLSUiFactory.Font;
                return font;
            }
        }
    }
}
