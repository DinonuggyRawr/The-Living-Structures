using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>Separate explicit native-C workflow; does not replace the modeled 1.4 lab.</summary>
    public static class TLSNativeCompilerLabScreen
    {
        private static Vector2 V(float x,float y) {return new Vector2(x,y);}
        public static void Render(Transform root, StackFunctionLabSession practice,
            StackNativeLabSession native, Action back, Action refresh)
        {
            if(root==null||practice==null||native==null||back==null||refresh==null)
                throw new ArgumentNullException("Native Compiler Lab needs both sessions and navigation.");
            TLSUiFactory.Panel(root,"Native lab background",V(0,0),V(1,0.908f),TLSUiFactory.Navy);
            RectTransform tabs=TLSUiFactory.Panel(root,"Native function selector",V(0.019f,0.807f),V(0.981f,0.892f),TLSUiFactory.Slate);
            for(int i=0;i<3;i++)
            {
                StackFunctionEvaluator.Function f=(StackFunctionEvaluator.Function)i;
                float x=0.014f+i*0.332f;
                TLSUiFactory.MakeButton(tabs,"COMPILE "+f.ToString().ToUpperInvariant(),V(x,0.14f),V(x+0.319f,0.86f),
                    ()=>{practice.Select(f);refresh();},practice.Selected==f?TLSUiFactory.Deep:TLSUiFactory.Navy,17,!native.Busy);
            }
            RectTransform code=TLSUiFactory.Panel(root,"Native C editor",V(0.019f,0.156f),V(0.626f,0.796f),TLSUiFactory.Slate);
            TLSUiFactory.Label(code,"Title","COMPILER-BACKED C11 TESTS — "+practice.Selected.ToString().ToUpperInvariant(),
                V(0.032f,0.907f),V(0.971f,0.983f),21,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(code,"Objective",StackFunctionEvaluator.Objective(practice.Selected),
                V(0.034f,0.79f),V(0.968f,0.895f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(code,"Label","COMPLETE C FUNCTION  |  same draft as WRITE C FUNCTIONS (5000 characters max)",
                V(0.034f,0.736f),V(0.974f,0.789f),15,TLSUiFactory.Violet,TextAnchor.MiddleLeft,true);
            InputField editor=TLSUiFactory.Field(code,"Native C editor input",practice.Draft,
                "Type C11 function source",V(0.034f,0.162f),V(0.969f,0.733f),s=>practice.Draft=s);
            editor.lineType=InputField.LineType.MultiLineNewline;
            editor.characterLimit=StackNativeDockerRunner.SourceLimit;
            editor.textComponent.fontSize=18;
            editor.textComponent.alignment=TextAnchor.UpperLeft;
            editor.textComponent.horizontalOverflow=HorizontalWrapMode.Wrap;
            editor.textComponent.verticalOverflow=VerticalWrapMode.Truncate;
            editor.interactable=!native.Busy;
            TLSUiFactory.MakeButton(code,"RUN C11 TESTS",V(0.035f,0.038f),V(0.43f,0.145f),
                ()=>{native.Start(practice.Selected,practice.Draft);refresh();},TLSUiFactory.Violet,17,native.OptedIn&&!native.Busy);
            TLSUiFactory.MakeButton(code,"RESTORE STARTER",V(0.44f,0.038f),V(0.701f,0.145f),
                ()=>{practice.ResetDraft();refresh();},TLSUiFactory.Deep,14,!native.Busy);
            TLSUiFactory.MakeButton(code,native.Busy?"CANCEL RUN":"COPY GCC DIAGNOSTICS",
                V(0.711f,0.038f),V(0.97f,0.145f),
                ()=>{
                    if(native.Busy) native.Cancel();
                    else if(native.Latest!=null) GUIUtility.systemCopyBuffer=native.Latest.Diagnostic;
                    refresh();
                },TLSUiFactory.Deep,13,native.Busy || (native.Latest!=null && !string.IsNullOrEmpty(native.Latest.Diagnostic)));

            RectTransform side=TLSUiFactory.Panel(root,"Native C runner report",V(0.638f,0.156f),V(0.982f,0.796f),TLSUiFactory.Slate);
            TLSUiFactory.Label(side,"Runner title","OPTIONAL DOCKER COMPILER",V(0.044f,0.919f),V(0.969f,0.982f),20,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(side,"Safety","Requires local Docker Desktop + Linux containers. Docker is not a perfect sandbox.",
                V(0.044f,0.834f),V(0.963f,0.914f),14,TLSUiFactory.Amber);
            TLSUiFactory.MakeButton(side,native.OptedIn?"DISABLE NATIVE RUNNER":"ENABLE NATIVE RUNNER",
                V(0.044f,0.753f),V(0.964f,0.827f),()=>{if(native.OptedIn)native.Disable();else native.Enable();refresh();},
                native.OptedIn?TLSUiFactory.Deep:TLSUiFactory.Violet,15,!native.Busy);
            StackNativeDockerRunner.Result result=practice.Selected==native.LastFunction?native.Latest:null;
            string headline=native.Busy?"COMPILING / TESTING...":result==null?"NOT RUN FOR THIS FUNCTION":
                !result.CompilerRan?"DOCKER UNAVAILABLE":!result.Compiled?"C11 COMPILATION FAILED":
                result.Completed?"C11: "+result.Passed+" / 8 checks passed":"INCOMPLETE TEST RUN";
            TLSUiFactory.Label(side,"Result headline",headline,V(0.05f,0.686f),V(0.961f,0.746f),18,
                result!=null&&result.AllPassed?TLSUiFactory.Green:TLSUiFactory.Pale,TextAnchor.MiddleLeft,true);
            for(int i=0;i<StackNativeDockerRunner.CasesPerFunction;i++)
            {
                float top=0.674f-i*0.051f;
                string n=StackNativeDockerRunner.CaseName(practice.Selected,i);
                string suffix=result==null||!result.Completed||practice.Selected!=native.LastFunction?"—":result.Cases[i]?"PASS":"FAIL";
                Color c=result!=null&&result.Completed&&practice.Selected==native.LastFunction?
                    (result.Cases[i]?TLSUiFactory.Green:TLSUiFactory.Amber):TLSUiFactory.Muted;
                TLSUiFactory.Label(side,"Native case "+i,(i+1)+". "+n+"    "+suffix,
                    V(0.052f,top-0.050f),V(0.964f,top),14,c);
            }
            TLSUiFactory.Label(side,"Native status",native.Status,
                V(0.046f,0.138f),V(0.955f,0.27f),14,TLSUiFactory.Pale);
            string diagnostic=result==null?"Pull once in your terminal: docker pull "+StackNativeDockerRunner.Image:
                (string.IsNullOrEmpty(result.Diagnostic)?
                (result.Completed && !result.AllPassed ? "First failure: " + StackNativeDockerRunner.FirstFailureDescription(result) :
                "No compiler diagnostics to display."):result.Diagnostic);
            TLSUiFactory.Label(side,"Native diagnostics",diagnostic,
                V(0.048f,0.017f),V(0.951f,0.13f),13,TLSUiFactory.Muted);

            RectTransform footer=TLSUiFactory.Panel(root,"Native lab footer",V(0.019f,0.021f),V(0.981f,0.146f),TLSUiFactory.Deep);
            TLSUiFactory.Label(footer,"Native disclosure","Actual GCC/C11 compilation happens only inside the opt-in Docker runner; no host-native execution.",
                V(0.03f,0.58f),V(0.96f,0.96f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(footer,"Mastery scope","Finite 8-case checks are practice evidence only. Not exhaustive C correctness, memory-safety certification, or saved mastery.",
                V(0.03f,0.33f),V(0.96f,0.60f),14,TLSUiFactory.Amber);
            TLSUiFactory.MakeButton(footer,"RETURN TO MODELED LAB",V(0.704f,0.035f),V(0.973f,0.315f),back,TLSUiFactory.Slate,14);
        }
    }
}
