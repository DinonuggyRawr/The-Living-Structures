using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>Separate explicit wasm-C workflow; does not replace the modeled 1.4 lab.</summary>
    public static class TLSWasmCompilerLabScreen
    {
        private static Vector2 V(float x,float y) {return new Vector2(x,y);}
        public static void Render(Transform root, StackFunctionLabSession practice,
            StackWasmLabSession wasm, Action back, Action refresh)
        {
            if(root==null||practice==null||wasm==null||back==null||refresh==null)
                throw new ArgumentNullException("WASM C Compiler Lab needs both sessions and navigation.");
            TLSUiFactory.Panel(root,"WASM lab background",V(0,0),V(1,0.908f),TLSUiFactory.Navy);
            RectTransform tabs=TLSUiFactory.Panel(root,"WASM function selector",V(0.019f,0.807f),V(0.981f,0.892f),TLSUiFactory.Slate);
            for(int i=0;i<3;i++)
            {
                StackFunctionEvaluator.Function f=(StackFunctionEvaluator.Function)i;
                float x=0.014f+i*0.332f;
                TLSUiFactory.MakeButton(tabs,"COMPILE "+f.ToString().ToUpperInvariant(),V(x,0.14f),V(x+0.319f,0.86f),
                    ()=>{practice.Select(f);refresh();},practice.Selected==f?TLSUiFactory.Deep:TLSUiFactory.Navy,17,!wasm.Busy);
            }
            RectTransform code=TLSUiFactory.Panel(root,"WASM C editor",V(0.019f,0.156f),V(0.626f,0.796f),TLSUiFactory.Slate);
            TLSUiFactory.Label(code,"Title","REAL C11-TO-WASM TESTS — "+practice.Selected.ToString().ToUpperInvariant(),
                V(0.032f,0.907f),V(0.971f,0.983f),21,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(code,"Objective",StackFunctionEvaluator.Objective(practice.Selected),
                V(0.034f,0.79f),V(0.968f,0.895f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(code,"Label","COMPLETE C FUNCTION  |  same draft as WRITE C FUNCTIONS (5000 characters max)",
                V(0.034f,0.736f),V(0.974f,0.789f),15,TLSUiFactory.Violet,TextAnchor.MiddleLeft,true);
            InputField editor=TLSUiFactory.Field(code,"WASM C editor input",practice.Draft,
                "Type C11 function source",V(0.034f,0.162f),V(0.969f,0.733f),s=>practice.Draft=s);
            editor.lineType=InputField.LineType.MultiLineNewline;
            editor.characterLimit=StackWasmRunner.SourceLimit;
            editor.textComponent.fontSize=18;
            editor.textComponent.alignment=TextAnchor.UpperLeft;
            editor.textComponent.horizontalOverflow=HorizontalWrapMode.Wrap;
            editor.textComponent.verticalOverflow=VerticalWrapMode.Truncate;
            editor.interactable=!wasm.Busy;
            TLSUiFactory.MakeButton(code,"RUN WASM C11 TESTS",V(0.035f,0.038f),V(0.43f,0.145f),
                ()=>{wasm.Start(practice.Selected,practice.Draft,UnityEngine.Application.dataPath);refresh();},TLSUiFactory.Violet,17,wasm.OptedIn&&!wasm.Busy);
            TLSUiFactory.MakeButton(code,"RESTORE STARTER",V(0.44f,0.038f),V(0.701f,0.145f),
                ()=>{practice.ResetDraft();refresh();},TLSUiFactory.Deep,14,!wasm.Busy);
            TLSUiFactory.MakeButton(code,wasm.Busy?"CANCEL RUN":"COPY C DIAGNOSTICS",
                V(0.711f,0.038f),V(0.97f,0.145f),
                ()=>{
                    if(wasm.Busy) wasm.Cancel();
                    else if(wasm.Latest!=null) GUIUtility.systemCopyBuffer=wasm.Latest.Diagnostic;
                    refresh();
                },TLSUiFactory.Deep,13,wasm.Busy || (wasm.Latest!=null && !string.IsNullOrEmpty(wasm.Latest.Diagnostic)));

            RectTransform side=TLSUiFactory.Panel(root,"WASM C runner report",V(0.638f,0.156f),V(0.982f,0.796f),TLSUiFactory.Slate);
            TLSUiFactory.Label(side,"Runner title","DOCKER-FREE C11 / WEBASSEMBLY",V(0.044f,0.919f),V(0.969f,0.982f),20,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(side,"Safety","Requires local Zig + Wasmtime. No Docker. Run only your own learning code.",
                V(0.044f,0.834f),V(0.963f,0.914f),14,TLSUiFactory.Amber);
            TLSUiFactory.MakeButton(side,wasm.OptedIn?"DISABLE WASM RUNNER":"ENABLE WASM RUNNER",
                V(0.044f,0.753f),V(0.964f,0.827f),()=>{if(wasm.OptedIn)wasm.Disable();else wasm.Enable();refresh();},
                wasm.OptedIn?TLSUiFactory.Deep:TLSUiFactory.Violet,15,!wasm.Busy);
            bool stale = practice.Selected==wasm.LastFunction && wasm.Latest!=null && practice.Draft!=wasm.LastSource;
            StackWasmRunner.Result result=practice.Selected==wasm.LastFunction&&!stale?wasm.Latest:null;
            string headline=wasm.Busy?"C11 TO WASM / TESTING...":stale?"CODE CHANGED - RE-RUN TESTS":result==null?"NOT RUN FOR THIS FUNCTION":
                !result.CompilerRan?"NO COMPILER RESULT":!result.Compiled?"C11 COMPILATION FAILED":
                result.Completed?"WASM C11: "+result.Passed+" / 8 passed":"INCOMPLETE WASM RUN";
            TLSUiFactory.Label(side,"Result headline",headline,V(0.05f,0.686f),V(0.961f,0.746f),18,
                result!=null&&result.AllPassed?TLSUiFactory.Green:TLSUiFactory.Pale,TextAnchor.MiddleLeft,true);
            for(int i=0;i<StackWasmRunner.CasesPerFunction;i++)
            {
                float top=0.674f-i*0.051f;
                string n=StackWasmRunner.CaseName(practice.Selected,i);
                string suffix=result==null||!result.Completed||practice.Selected!=wasm.LastFunction?"—":result.Cases[i]?"PASS":"FAIL";
                Color c=result!=null&&result.Completed&&practice.Selected==wasm.LastFunction?
                    (result.Cases[i]?TLSUiFactory.Green:TLSUiFactory.Amber):TLSUiFactory.Muted;
                TLSUiFactory.Label(side,"WASM case "+i,(i+1)+". "+n+"    "+suffix,
                    V(0.052f,top-0.050f),V(0.964f,top),14,c);
            }
            TLSUiFactory.Label(side,"WASM status",wasm.Status,
                V(0.046f,0.138f),V(0.955f,0.27f),14,TLSUiFactory.Pale);
            string diagnostic=stale?"Draft changed since the last run; previous results are hidden." : result==null?"Install Zig + Wasmtime via setup script in the patch ZIP":
                (string.IsNullOrEmpty(result.Diagnostic)?
                (result.Completed && !result.AllPassed ? "First failure: " + StackWasmRunner.FirstFailureDescription(result) :
                "No compiler diagnostics to display."):result.Diagnostic);
            TLSUiFactory.Label(side,"WASM diagnostics",diagnostic,
                V(0.048f,0.017f),V(0.951f,0.13f),13,TLSUiFactory.Muted);

            RectTransform footer=TLSUiFactory.Panel(root,"WASM lab footer",V(0.019f,0.021f),V(0.981f,0.146f),TLSUiFactory.Deep);
            TLSUiFactory.Label(footer,"WASM disclosure","C11 builds to WebAssembly locally; only WASM runs. Zig/Wasmtime are trusted native host tools.",
                V(0.03f,0.58f),V(0.96f,0.96f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(footer,"Mastery scope","Eight finite tests are learning evidence, not comprehensive C correctness, security or saved mastery.",
                V(0.03f,0.33f),V(0.96f,0.60f),14,TLSUiFactory.Amber);
            TLSUiFactory.MakeButton(footer,"RETURN TO MODELED LAB",V(0.704f,0.035f),V(0.973f,0.315f),back,TLSUiFactory.Slate,14);
        }
    }
}
