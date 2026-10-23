using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>
    /// Phase 2.3 real C11 Array/Queue whole-function practice. The editor controls one draft,
    /// while the guarded WebAssembly runner and independent first-attempt evidence stay separate.
    /// </summary>
    public static class TLSLinearFunctionScreen
    {
        private static Vector2 V(float x, float y) { return new Vector2(x, y); }
        public static void Render(Transform root, TLSLinearFunctionLabSession practice,
            TLSLinearWasmLabSession wasm, Action back, Action refresh)
        {
            if (root == null || practice == null || wasm == null || back == null || refresh == null)
                throw new ArgumentNullException("C11 function lab requires root, practice, WASM, and navigation.");
            TLSUiFactory.Panel(root, "Linear whole-function backdrop", V(0,0), V(1,0.908f), TLSUiFactory.Navy);
            int count = practice.ArrayDomain ? 4 : 3;
            int first = practice.ArrayDomain ? 0 : 4;
            RectTransform tabs = TLSUiFactory.Panel(root, "Linear function selector", V(0.019f,0.802f), V(0.982f,0.893f), TLSUiFactory.Slate);
            for (int i=0; i<count; i++)
            {
                TLSLinearFunction task = (TLSLinearFunction)(first+i);
                float left = 0.013f + i*(0.974f/count);
                float width = 0.974f/count - 0.012f;
                TLSUiFactory.MakeButton(tabs, TLSLinearFunctionCatalog.Name(task),
                    V(left,0.12f), V(left+width,0.88f),
                    () => {practice.Select(task);refresh();},
                    practice.Selected == task ? TLSUiFactory.Deep : TLSUiFactory.Navy, count==4 ? 15 : 17, !wasm.Busy);
            }
            RectTransform code = TLSUiFactory.Panel(root,"Linear C11 function editor",V(0.019f,0.161f),V(0.627f,0.790f),TLSUiFactory.Slate);
            TLSUiFactory.Label(code,"Function title", "WRITE REAL C11 — " + TLSLinearFunctionCatalog.Name(practice.Selected),
                V(0.037f,0.918f),V(0.97f,0.989f),21,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(code,"Coding objective",TLSLinearFunctionCatalog.Goal(practice.Selected),
                V(0.038f,0.815f),V(0.968f,0.910f),16,TLSUiFactory.Pale);
            string hint = practice.HintVisible ? "HINT: " + TLSLinearFunctionCatalog.Hint(practice.Selected) :
                practice.ReferenceVisible ? "Reference loaded. Its result counts as assisted practice, not unseen first-attempt evidence." :
                "Write the complete C11 function. Test cases check return values, output parameters, counts and raw cells.";
            TLSUiFactory.Label(code,"Coding help text",hint,V(0.038f,0.745f),V(0.968f,0.813f),14,
                practice.HintVisible || practice.ReferenceVisible ? TLSUiFactory.Amber : TLSUiFactory.Muted);
            Text liveHeadline = null;
            Text liveStatus = null;
            Text[] liveCases = new Text[TLSLinearWasmRunner.CasesPerFunction];
            InputField editor = TLSUiFactory.Field(code,"Linear student C editor",practice.Draft,
                "Write C11 code; no C++ syntax",V(0.037f,0.167f),V(0.970f,0.739f),s =>
                {
                    practice.Draft = s;
                    // Never leave a previously passing score visible while editing a different draft.
                    if (wasm.Latest != null && wasm.LastFunction == practice.Selected && wasm.LastSource != s)
                    {
                        if (liveHeadline != null)
                        {
                            liveHeadline.text = "DRAFT CHANGED — RETEST";
                            liveHeadline.color = TLSUiFactory.Pale;
                        }
                        if (liveStatus != null) liveStatus.text = "Edited since last run. Previous passing result hidden.";
                        for (int i = 0; i < liveCases.Length; i++)
                            if (liveCases[i] != null)
                                liveCases[i].text = (i+1) + ". " + TLSLinearWasmRunner.CaseName(practice.Selected,i) + "   —";
                    }
                });
            editor.lineType = InputField.LineType.MultiLineNewline;
            editor.characterLimit = TLSLinearWasmRunner.SourceLimit;
            editor.textComponent.fontSize = 18;
            editor.textComponent.alignment = TextAnchor.UpperLeft;
            editor.textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
            editor.textComponent.verticalOverflow = VerticalWrapMode.Truncate;
            editor.interactable = !wasm.Busy;
            TLSUiFactory.MakeButton(code,"RUN C11 WASM TESTS",V(0.038f,0.042f),V(0.445f,0.151f),
                () => {wasm.Start(practice,UnityEngine.Application.dataPath);refresh();},TLSUiFactory.Violet,16,wasm.OptedIn&&!wasm.Busy);
            TLSUiFactory.MakeButton(code,"RESTORE STARTER",V(0.455f,0.042f),V(0.713f,0.151f),
                () => {practice.RestoreStarter();refresh();},TLSUiFactory.Deep,14,!wasm.Busy);
            TLSUiFactory.MakeButton(code,wasm.Busy?"CANCEL RUN":"COPY DIAGNOSTICS",V(0.725f,0.042f),V(0.972f,0.151f),
                () => {if(wasm.Busy) wasm.Cancel(); else if(wasm.Latest!=null) GUIUtility.systemCopyBuffer = wasm.Latest.Diagnostic;refresh();},
                TLSUiFactory.Deep,13,wasm.Busy || (wasm.Latest!=null && !string.IsNullOrWhiteSpace(wasm.Latest.Diagnostic)));

            RectTransform info = TLSUiFactory.Panel(root,"Linear C11 results and assistance",V(0.64f,0.161f),V(0.982f,0.790f),TLSUiFactory.Slate);
            TLSUiFactory.Label(info,"C11 title","REAL C11 / WEBASSEMBLY",V(0.047f,0.928f),V(0.968f,0.989f),19,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(info,"C11 safety","Local Zig + Wasmtime; separate bounded WASM runs. Trusted practice code only.",
                V(0.047f,0.872f),V(0.968f,0.929f),13,TLSUiFactory.Amber);
            TLSUiFactory.MakeButton(info,wasm.OptedIn?"DISABLE WASM RUNNER":"ENABLE WASM RUNNER",V(0.046f,0.803f),V(0.957f,0.871f),
                () => {if(wasm.OptedIn)wasm.Disable();else wasm.Enable();refresh();},
                wasm.OptedIn?TLSUiFactory.Deep:TLSUiFactory.Violet,15,!wasm.Busy);
            TLSUiFactory.MakeButton(info,"GUIDED",V(0.045f,0.730f),V(0.495f,0.796f),
                () => {practice.SetHelp(TLSLinearCodingHelp.Guided);refresh();},
                practice.Help == TLSLinearCodingHelp.Guided?TLSUiFactory.Deep:TLSUiFactory.Navy,15,!wasm.Busy);
            TLSUiFactory.MakeButton(info,"INDEPENDENT",V(0.506f,0.730f),V(0.956f,0.796f),
                () => {practice.SetHelp(TLSLinearCodingHelp.Independent);refresh();},
                practice.Help == TLSLinearCodingHelp.Independent?TLSUiFactory.Deep:TLSUiFactory.Navy,15,!wasm.Busy);
            TLSUiFactory.MakeButton(info,"SHOW HINT",V(0.045f,0.654f),V(0.495f,0.723f),
                () => {practice.ShowHint();refresh();},TLSUiFactory.Deep,14,
                !wasm.Busy&&practice.Help==TLSLinearCodingHelp.Guided);
            TLSUiFactory.MakeButton(info,"SHOW REFERENCE",V(0.506f,0.654f),V(0.956f,0.723f),
                () => {practice.RevealReference();refresh();},TLSUiFactory.Deep,14,!wasm.Busy);
            bool currentResult = wasm.LastFunction == practice.Selected && wasm.Latest != null;
            bool stale = currentResult && practice.Draft != wasm.LastSource;
            TLSLinearWasmRunner.Result result = currentResult&&!stale?wasm.Latest:null;
            string headline = wasm.Busy?"COMPILING / TESTING...":stale?"DRAFT CHANGED — RETEST":result==null?"NO CURRENT C11 RESULT":
                !result.CompilerRan?"COMPILER NOT STARTED":!result.Compiled?"C11 COMPILATION FAILED":
                !result.Completed?"INCOMPLETE WASM TESTS":"WASM C11: "+result.Passed+"/10 passed";
            liveHeadline = TLSUiFactory.Label(info,"C11 result headline",headline,V(0.052f,0.598f),V(0.965f,0.645f),17,
                result!=null&&result.AllPassed?TLSUiFactory.Green:TLSUiFactory.Pale,TextAnchor.MiddleLeft,true);
            for (int i=0; i<TLSLinearWasmRunner.CasesPerFunction; i++)
            {
                int index = i;
                float top = 0.588f - index*0.042f;
                string state = result==null || !result.Completed ? "—" : result.Cases[i]?"PASS":"FAIL";
                Color color = result==null || !result.Completed?TLSUiFactory.Muted:result.Cases[i]?TLSUiFactory.Green:TLSUiFactory.Amber;
                liveCases[i] = TLSUiFactory.Label(info,"Linear WASM case "+i,(i+1)+". "+TLSLinearWasmRunner.CaseName(practice.Selected,i)+"   "+state,
                    V(0.051f,top-0.041f),V(0.965f,top),13,color);
            }
            string message = stale?"Source changed since testing; previous result intentionally hidden.":
                result==null?wasm.Status:result.Status;
            liveStatus = TLSUiFactory.Label(info,"C11 status",message,V(0.052f,0.066f),V(0.952f,0.155f),13,TLSUiFactory.Pale);
            string diag = stale?"Re-run to check your current C function.":result==null?"Compiler diagnostics appear here after RUN.":
                string.IsNullOrEmpty(result.Diagnostic)?TLSLinearWasmRunner.FirstFailureDescription(result):result.Diagnostic;
            TLSUiFactory.Label(info,"C11 diagnostics",diag,V(0.052f,0.006f),V(0.952f,0.064f),12,TLSUiFactory.Muted);

            RectTransform footer = TLSUiFactory.Panel(root,"Linear C11 footer",V(0.019f,0.023f),V(0.982f,0.150f),TLSUiFactory.Deep);
            TLSUiFactory.Label(footer,"C11 session evidence","SESSION ONLY: "+practice.CompiledPassCount+"/"+practice.DomainTotal+
                " functions previously passed 10 tests  |  "+practice.FirstIndependentPassCount+" unseen independent first passes",
                V(0.03f,0.61f),V(0.972f,0.972f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(footer,"C11 scope","Hints, answer reveals or earlier attempts bar unseen first-pass credit. Ten finite tests are not C mastery or memory-safety proof.",
                V(0.03f,0.315f),V(0.973f,0.612f),14,TLSUiFactory.Amber);
            TLSUiFactory.MakeButton(footer,"RETURN TO "+(practice.ArrayDomain?"ARRAY":"QUEUE")+" LAB",
                V(0.703f,0.027f),V(0.973f,0.298f),back,TLSUiFactory.Slate,14,!wasm.Busy);
        }
    }
}
