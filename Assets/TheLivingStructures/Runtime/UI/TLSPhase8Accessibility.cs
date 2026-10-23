using System;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>
    /// Accessibility options are presentation-only and stored under a separate key.
    /// They never change C source, lab state, evidence, or the original Academy saves.
    /// </summary>
    public static class TLSPhase8Accessibility
    {
        public const string PreferenceKey = "tls_phase8_presentation_v1";
        [Serializable] private sealed class Record
        {
            public int version = 1;
            public int textLevel;
            public bool highContrast;
            public bool reducedMotion;
        }
        private static Record settings;
        private static bool open;
        private static Vector2 V(float x,float y) { return new Vector2(x,y); }
        private static Record Settings
        {
            get
            {
                if (settings != null) return settings;
                settings = new Record();
                if (!PlayerPrefs.HasKey(PreferenceKey)) return settings;
                try
                {
                    Record stored = JsonUtility.FromJson<Record>(PlayerPrefs.GetString(PreferenceKey));
                    if (stored != null && stored.version == 1 && stored.textLevel >= 0 && stored.textLevel <= 2)
                        settings = stored;
                }
                catch (ArgumentException) { /* Ignore malformed preferences, not academic progress. */ }
                return settings;
            }
        }
        public static int TextLevel { get { return Settings.textLevel; } }
        public static bool HighContrast { get { return Settings.highContrast; } }
        public static bool ReducedMotion { get { return Settings.reducedMotion; } }
        public static void Configure(int textLevel, bool highContrast, bool reducedMotion)
        {
            if (textLevel < 0 || textLevel > 2) throw new ArgumentOutOfRangeException("textLevel");
            settings = new Record { version=1, textLevel=textLevel, highContrast=highContrast, reducedMotion=reducedMotion };
            PlayerPrefs.SetString(PreferenceKey, JsonUtility.ToJson(settings));
            PlayerPrefs.Save();
        }
        public static void Apply(Transform root)
        {
            // The UI factory already creates the standard accessible defaults.
            // Avoid two costly hierarchy walks on every screen redraw when
            // the user has not enabled any presentation overrides.
            Record current = Settings;
            if (current.textLevel == 0 && !current.highContrast && !current.reducedMotion)
                return;
            float multiplier = 1f + current.textLevel * .12f;
            Text[] labels = root.GetComponentsInChildren<Text>(true);
            foreach (Text t in labels)
            {
                int sourceSize = t.fontSize;
                t.fontSize = Mathf.Clamp(Mathf.RoundToInt(sourceSize * multiplier), sourceSize, 36);
                if (current.highContrast && t.color == TLSUiFactory.Muted) t.color = TLSUiFactory.Pale;
                // Do not change an input field's text or onValueChanged while restyling.
            }
            foreach (Selectable selectable in root.GetComponentsInChildren<Selectable>(true))
            {
                selectable.navigation = new Navigation { mode = Navigation.Mode.Automatic };
                ColorBlock colors = selectable.colors;
                colors.fadeDuration = current.reducedMotion ? 0f : .1f;
                selectable.colors = colors;
            }
        }
        public static void Draw(Transform root, Action redraw)
        {
            TLSUiFactory.MakeButton(root,"Aa OPTIONS",V(.671f,.928f),V(.785f,.978f),()=>{open=!open;redraw();},
                open?TLSUiFactory.Violet:TLSUiFactory.Deep,13);
            if (!open) return;
            RectTransform panel=TLSUiFactory.Panel(root,"Phase8 accessibility options",V(.684f,.474f),V(.986f,.903f),TLSUiFactory.Slate);
            TLSUiFactory.Label(panel,"Options heading","ACCESSIBILITY / DISPLAY",V(.055f,.850f),V(.95f,.98f),19,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(panel,"Font status","TEXT SIZE  "+(100 + Settings.textLevel*12)+"%",V(.052f,.731f),V(.95f,.829f),16);
            TLSUiFactory.MakeButton(panel,"TEXT -",V(.05f,.626f),V(.481f,.726f),()=>{Configure(Math.Max(0,TextLevel-1),HighContrast,ReducedMotion);redraw();},TLSUiFactory.Deep,14,TextLevel>0);
            TLSUiFactory.MakeButton(panel,"TEXT +",V(.522f,.626f),V(.95f,.726f),()=>{Configure(Math.Min(2,TextLevel+1),HighContrast,ReducedMotion);redraw();},TLSUiFactory.Deep,14,TextLevel<2);
            TLSUiFactory.MakeButton(panel,HighContrast?"CONTRAST: HIGH":"CONTRAST: STANDARD",V(.05f,.494f),V(.95f,.601f),()=>{Configure(TextLevel,!HighContrast,ReducedMotion);redraw();},TLSUiFactory.Deep,14);
            TLSUiFactory.MakeButton(panel,ReducedMotion?"REDUCED MOTION: ON":"REDUCED MOTION: OFF",V(.05f,.358f),V(.95f,.47f),()=>{Configure(TextLevel,HighContrast,!ReducedMotion);redraw();},TLSUiFactory.Deep,13);
            TLSUiFactory.Label(panel,"Scope note","Applies to this app's UI. Reduced motion disables uGUI color fades; earlier code traces already step instantly.",V(.05f,.17f),V(.95f,.341f),13,TLSUiFactory.Muted);
            TLSUiFactory.MakeButton(panel,"CLOSE",V(.05f,.04f),V(.95f,.143f),()=>{open=false;redraw();},TLSUiFactory.Violet,14);
        }
    }
}
