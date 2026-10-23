using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>Whole-function C drafting with an isolated bounded-subset evaluator.</summary>
    public static class TLSFunctionLabScreen
    {
        private static Vector2 V(float x,float y) {return new Vector2(x,y);}
        public static void Render(Transform root,StackFunctionLabSession state,Action back,Action refresh, Action openWasm)
        {
            if(root==null||state==null||back==null||refresh==null||openWasm==null) throw new ArgumentNullException("Function Lab needs navigation and session state.");
            TLSUiFactory.Panel(root,"Function Lab background",V(0,0),V(1,0.908f),TLSUiFactory.Navy);
            RectTransform tabs=TLSUiFactory.Panel(root,"Functions",V(0.018f,0.807f),V(0.982f,0.892f),TLSUiFactory.Slate);
            for(int i=0;i<3;i++)
            {
                StackFunctionEvaluator.Function f=(StackFunctionEvaluator.Function)i;
                float x=0.014f+i*0.332f;
                TLSUiFactory.MakeButton(tabs,"WRITE "+f.ToString().ToUpperInvariant(),V(x,0.14f),V(x+0.319f,0.86f),
                    ()=>{state.Select(f);refresh();},state.Selected==f?TLSUiFactory.Deep:TLSUiFactory.Navy,19);
            }
            RectTransform code=TLSUiFactory.Panel(root,"Complete C function",V(0.02f,0.155f),V(0.626f,0.797f),TLSUiFactory.Slate);
            TLSUiFactory.Label(code,"Editor title","COMPLETE C FUNCTION  |  "+state.Selected.ToString().ToUpperInvariant(),
                V(0.032f,0.90f),V(0.96f,0.986f),23,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(code,"Objective",StackFunctionEvaluator.Objective(state.Selected),
                V(0.033f,0.795f),V(0.962f,0.898f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(code,"Editor label","EDITABLE C SOURCE  |  supported instructional subset (up to 5000 characters)",
                V(0.034f,0.736f),V(0.967f,0.79f),15,TLSUiFactory.Violet,TextAnchor.MiddleLeft,true);
            InputField editor=TLSUiFactory.Field(code,"Multiline C function editor",state.Draft,
                "Enter complete C function",V(0.034f,0.168f),V(0.968f,0.735f),s=>state.Draft=s);
            editor.lineType=InputField.LineType.MultiLineNewline;
            editor.characterLimit=5000;
            editor.textComponent.fontSize=18;
            editor.textComponent.alignment=TextAnchor.UpperLeft;
            editor.textComponent.horizontalOverflow=HorizontalWrapMode.Wrap;
            editor.textComponent.verticalOverflow=VerticalWrapMode.Truncate;
            Text placeholder=editor.placeholder as Text;
            if(placeholder!=null)placeholder.alignment=TextAnchor.UpperLeft;
            TLSUiFactory.MakeButton(code,"RUN MODELED TESTS",V(0.033f,0.045f),V(0.386f,0.147f),
                ()=>{state.Run();refresh();},TLSUiFactory.Violet,17);
            TLSUiFactory.MakeButton(code,"RESTORE STARTER",V(0.401f,0.045f),V(0.663f,0.147f),
                ()=>{state.ResetDraft();refresh();},TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(code,"SHOW REFERENCE",V(0.675f,0.045f),V(0.969f,0.147f),
                ()=>{state.Reveal();refresh();},TLSUiFactory.Deep,15);

            RectTransform side=TLSUiFactory.Panel(root,"Function checker and support",V(0.638f,0.155f),V(0.981f,0.797f),TLSUiFactory.Slate);
            TLSUiFactory.Label(side,"Checker title","TEST RESULTS / REASONING",V(0.042f,0.909f),V(0.96f,0.982f),20,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(side,"Evaluation disclosure","BOUNDED C11 SUBSET  |  NOT A COMPILER",V(0.044f,0.856f),V(0.969f,0.913f),15,TLSUiFactory.Amber,TextAnchor.MiddleLeft,true);
            TLSUiFactory.MakeButton(side,"GUIDED",V(0.042f,0.756f),V(0.486f,0.847f),
                ()=>{state.SetGuided(true);refresh();},state.Guided?TLSUiFactory.Deep:TLSUiFactory.Navy,16);
            TLSUiFactory.MakeButton(side,"INDEPENDENT",V(0.512f,0.756f),V(0.963f,0.847f),
                ()=>{state.SetGuided(false);refresh();},state.Guided?TLSUiFactory.Navy:TLSUiFactory.Deep,16);
            TLSUiFactory.MakeButton(side,"SHOW HINT",V(0.042f,0.66f),V(0.963f,0.744f),
                ()=>{state.ShowHint();refresh();},TLSUiFactory.Deep,16,state.Guided);
            TLSUiFactory.Label(side,"Hint",state.HintShowing?StackFunctionEvaluator.Hint(state.Selected):
                (state.Guided?"Use SHOW HINT for algorithm guidance. Hints remove first-attempt credit.":
                "No guided hints in Independent mode. Reveal is still available for review."),
                V(0.048f,0.52f),V(0.958f,0.653f),16,TLSUiFactory.Muted);
            StackFunctionEvaluator.Result result=state.Latest;
            string score=result==null?"NOT TESTED":result.Passed+" / "+result.Total+" modeled cases passed";
            TLSUiFactory.Label(side,"Score",score,V(0.05f,0.433f),V(0.965f,0.507f),19,
                result!=null&&result.Accepted?TLSUiFactory.Green:TLSUiFactory.Violet,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(side,"Feedback",state.Status,V(0.05f,0.167f),V(0.952f,0.421f),17,
                result!=null&&!result.Accepted?TLSUiFactory.Amber:TLSUiFactory.Pale);
            TLSUiFactory.Label(side,"Credit",state.FirstAttempt?"Unseen first-attempt modeled pass: YES":
                "Unseen first-attempt modeled pass: NOT RECORDED",V(0.05f,0.081f),V(0.959f,0.165f),14,TLSUiFactory.Muted);
            TLSUiFactory.Label(side,"No mastery","Compiler-independent C mastery/memory safety: NOT CERTIFIED",
                V(0.05f,0.016f),V(0.959f,0.084f),14,TLSUiFactory.Amber);

            RectTransform footer=TLSUiFactory.Panel(root,"Function lab footer",V(0.019f,0.022f),V(0.981f,0.145f),TLSUiFactory.Deep);
            TLSUiFactory.Label(footer,"Progress","SESSION: "+state.PassedCount+"/3 functions pass modeled cases  |  "+
                state.IndependentFirstAttemptCount+"/3 independent first-attempt modeled passes",
                V(0.028f,0.58f),V(0.97f,0.96f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(footer,"Scope","Only limited if/else, return, arithmetic, pointer checks, locals, and stack array operations; no saves, native execution, or mastery.",
                V(0.028f,0.34f),V(0.97f,0.59f),14,TLSUiFactory.Muted);
            TLSUiFactory.MakeButton(footer,"OPEN WASM C11 LAB",V(0.418f,0.035f),V(0.694f,0.33f),openWasm,TLSUiFactory.Violet,14);
            TLSUiFactory.MakeButton(footer,"RETURN TO STACK LAB",V(0.708f,0.035f),V(0.975f,0.33f),back,TLSUiFactory.Slate,14);
        }
    }
}
