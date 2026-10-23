using System;
using System.Collections.Generic;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    public static class TLSPhase4VisualScreens
    {
        private static Vector2 V(float x,float y){return new Vector2(x,y);}
        public static void Recursion(Transform root,TLSRecursionLab session,Func<string> read,Action<string> save,Func<string> readPrediction,Action<string> savePrediction,Action redraw,Action practice,Action functions)
        {
            TLSUiFactory.Panel(root,"Recursion background",V(0,0),V(1,0.907f),TLSUiFactory.Navy);
            RectTransform tabs=TLSUiFactory.Panel(root,"Recursion example tabs",V(.019f,.813f),V(.982f,.898f),TLSUiFactory.Slate);
            for(int i=0;i<3;i++)
            {
                TLSRecursionExample e=(TLSRecursionExample)i;float x=.012f+i*.195f;
                TLSUiFactory.MakeButton(tabs,e.ToString().ToUpperInvariant(),V(x,.14f),V(x+.181f,.89f),
                    ()=>{session.Begin(e,e==TLSRecursionExample.Fibonacci?4:4);redraw();},
                    session.Example==e?TLSUiFactory.Violet:TLSUiFactory.Deep,15);
            }
            TLSUiFactory.MakeButton(tabs,"C PRACTICE",V(.617f,.14f),V(.795f,.89f),practice,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"WRITE C",V(.807f,.14f),V(.987f,.89f),functions,TLSUiFactory.Deep,15);
            RectTransform code=TLSUiFactory.Panel(root,"Recursion C code",V(.019f,.31f),V(.479f,.80f),TLSUiFactory.Slate);
            TLSUiFactory.Label(code,"Code title","C11 RECURSIVE FUNCTION",V(.035f,.908f),V(.974f,.986f),22,TLSUiFactory.Cyan);
            string[] lines=TLSRecursionLab.Code(session.Example);
            for(int i=0;i<lines.Length;i++)
            {
                float top=.88f-i*.137f;bool current=session.Current.Line==i;
                RectTransform row=TLSUiFactory.Panel(code,"C statement "+i,V(.024f,top-.124f),V(.978f,top),
                    current?new Color(.14f,.35f,.43f,1):TLSUiFactory.Deep);
                TLSUiFactory.Label(row,"C",(i+1).ToString("00")+"  "+lines[i],V(.025f,.06f),V(.98f,.965f),
                    17,current?TLSUiFactory.Pale:TLSUiFactory.Muted,TextAnchor.MiddleLeft,current);
            }
            RectTransform stack=TLSUiFactory.Panel(root,"Recursion call stack",V(.489f,.31f),V(.982f,.80f),TLSUiFactory.Deep);
            TLSUiFactory.Label(stack,"Stack title","ACTIVE C CALL FRAMES — LATEST AT BOTTOM",V(.035f,.909f),V(.98f,.987f),19,TLSUiFactory.Cyan);
            TLSUiFactory.Label(stack,"Stack content",session.Current.Calls,V(.048f,.125f),V(.971f,.89f),
                17,TLSUiFactory.Pale,TextAnchor.UpperLeft);
            TLSUiFactory.Label(stack,"Depth","DEPTH: "+session.Current.Depth+"  |  STEP "+session.Step+"/"+(session.Frames.Count-1),
                V(.048f,.02f),V(.97f,.115f),17,TLSUiFactory.Violet);
            RectTransform explanation=TLSUiFactory.Panel(root,"Recursion explanation",V(.019f,.196f),V(.982f,.296f),TLSUiFactory.Slate);
            TLSUiFactory.Label(explanation,"Explanation",session.Current.Explanation,V(.02f,.32f),V(.98f,.97f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(explanation,"Return",string.IsNullOrEmpty(session.Current.Result)?
                "Return is not yet visible at this step.":"CURRENT RETURN VALUE: "+session.Current.Result,
                V(.02f,.023f),V(.98f,.30f),15,TLSUiFactory.Green);
            RectTransform ctrl=TLSUiFactory.Panel(root,"Recursion controls",V(.019f,.016f),V(.982f,.179f),TLSUiFactory.Slate);
            TLSUiFactory.Label(ctrl,"Parameter","n (sum/factorial 0..10; fib 0..6)",V(.018f,.74f),V(.32f,.985f),14,TLSUiFactory.Pale);
            TLSUiFactory.Field(ctrl,"Recursion input",read(),"n",V(.019f,.405f),V(.137f,.729f),save);
            TLSUiFactory.MakeButton(ctrl,"START n",V(.147f,.415f),V(.270f,.73f),()=>{
                int n;
                if(!int.TryParse(read(),out n))session.RejectInput("Enter an integer n.");
                else try{session.Begin(session.Example,n);}catch(ArgumentOutOfRangeException e){session.RejectInput(e.Message);}
                redraw();},TLSUiFactory.Deep,14);
            TLSUiFactory.MakeButton(ctrl,"< BACK",V(.278f,.415f),V(.413f,.73f),()=>{session.Previous();redraw();},TLSUiFactory.Deep,14,session.Step>0);
            TLSUiFactory.MakeButton(ctrl,"NEXT >",V(.421f,.415f),V(.557f,.73f),()=>{session.Next();redraw();},TLSUiFactory.Deep,14,session.Step<session.Frames.Count-1);
            TLSUiFactory.MakeButton(ctrl,"FINISH",V(.565f,.415f),V(.684f,.73f),()=>{session.Finish();redraw();},TLSUiFactory.Deep,14);
            TLSUiFactory.Label(ctrl,"Prediction label",session.PredictionFeedback,
                V(.018f,.075f),V(.538f,.372f),14,TLSUiFactory.Amber);
            TLSUiFactory.Field(ctrl,"Recursive predicted return input",readPrediction(),"return?",V(.548f,.075f),V(.679f,.365f),savePrediction);
            TLSUiFactory.MakeButton(ctrl,"PREDICT n",V(.690f,.075f),V(.835f,.365f),()=>{session.Predict(readPrediction());redraw();},TLSUiFactory.Violet,14,!session.PredictionAnswered&&session.Step==0);
            TLSUiFactory.Label(ctrl,"Scope",string.IsNullOrEmpty(session.InputError)?"Modeled C11 calls — no code execution or progress writes.":session.InputError,
                V(.84f,.07f),V(.979f,.382f),12,TLSUiFactory.Muted);
        }
        public static void Tree(Transform root,TLSTreeSession session,Func<string> value,Func<string> parent,
            Action<string> setValue,Action<string> setParent,Action redraw,Action practice,Action functions)
        {
            TLSUiFactory.Panel(root,"Tree background",V(0,0),V(1,.907f),TLSUiFactory.Navy);
            RectTransform kinds=TLSUiFactory.Panel(root,"Tree mode tabs",V(.019f,.822f),V(.982f,.896f),TLSUiFactory.Slate);
            for(int i=0;i<3;i++)
            {
                TLSTreeKind k=(TLSTreeKind)i;float x=.01f+i*.164f;
                TLSUiFactory.MakeButton(kinds,k==TLSTreeKind.General?"GENERAL TREE":k==TLSTreeKind.Binary?"BINARY TREE":"BST",V(x,.12f),V(x+.156f,.9f),
                    ()=>{session.Select(k);redraw();},session.Kind==k?TLSUiFactory.Violet:TLSUiFactory.Deep,15);
            }
            TLSUiFactory.MakeButton(kinds,"C PRACTICE",V(.524f,.12f),V(.746f,.9f),practice,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(kinds,"WRITE C BST FUNCTIONS",V(.756f,.12f),V(.99f,.9f),functions,TLSUiFactory.Deep,14);
            RectTransform operations=TLSUiFactory.Panel(root,"Tree operation bar",V(.019f,.710f),V(.982f,.814f),TLSUiFactory.Slate);
            TLSTreeAction[] actions=session.Kind==TLSTreeKind.General?
                new[]{TLSTreeAction.AddRoot,TLSTreeAction.AddChild,TLSTreeAction.Preorder,TLSTreeAction.Postorder,TLSTreeAction.LevelOrder,TLSTreeAction.Reset}:
                session.Kind==TLSTreeKind.Binary?
                new[]{TLSTreeAction.AddRoot,TLSTreeAction.AddLeft,TLSTreeAction.AddRight,TLSTreeAction.Preorder,TLSTreeAction.Inorder,TLSTreeAction.Postorder,TLSTreeAction.LevelOrder,TLSTreeAction.Reset}:
                new[]{TLSTreeAction.Insert,TLSTreeAction.Search,TLSTreeAction.Delete,TLSTreeAction.Preorder,TLSTreeAction.Inorder,TLSTreeAction.Postorder,TLSTreeAction.LevelOrder,TLSTreeAction.Reset};
            for(int i=0;i<actions.Length;i++)
            {
                TLSTreeAction selected=actions[i];float x=.008f+i*.984f/actions.Length;
                TLSUiFactory.MakeButton(operations,selected==TLSTreeAction.LevelOrder?"BFS":selected.ToString().ToUpperInvariant(),
                    V(x,.12f),V(x+.970f/actions.Length,.90f),()=>{
                        int v=0,p=0;
                        bool needsValue=selected==TLSTreeAction.Insert||selected==TLSTreeAction.Search||selected==TLSTreeAction.Delete||
                            selected==TLSTreeAction.AddRoot||selected==TLSTreeAction.AddChild||selected==TLSTreeAction.AddLeft||selected==TLSTreeAction.AddRight;
                        bool needsParent=selected==TLSTreeAction.AddChild||selected==TLSTreeAction.AddLeft||selected==TLSTreeAction.AddRight;
                        if(needsValue&&!int.TryParse(value(),out v))session.RejectInput("Enter an integer VALUE / TARGET.");
                        else if(needsParent&&!int.TryParse(parent(),out p))session.RejectInput("Enter an integer PARENT VALUE.");
                        else session.Begin(selected,v,p);
                        redraw();},TLSUiFactory.Deep,actions.Length>7?12:14);
            }
            RectTransform code=TLSUiFactory.Panel(root,"Tree C modeled algorithm",V(.019f,.363f),V(.488f,.698f),TLSUiFactory.Slate);
            TLSUiFactory.Label(code,"Code header","C11 ALGORITHM SKETCH — NOT A CODE INTERPRETER",V(.025f,.889f),V(.978f,.988f),18,TLSUiFactory.Cyan);
            TLSTreeTrace trace=session.Trace;
            string[] source=trace==null?new[]{"// Select an operation above","// Invariants prevent cycles and dangling links."}:trace.Code;
            TLSTreeFrame frame=trace==null?null:session.Frame;
            for(int i=0;i<source.Length;i++)
            {
                float top=.872f-i*.120f;if(top<=.025f)break;
                RectTransform line=TLSUiFactory.Panel(code,"Line "+i,V(.022f,top-.116f),V(.975f,top),
                    frame!=null&&frame.Line==i?new Color(.15f,.34f,.43f,1):TLSUiFactory.Deep);
                TLSUiFactory.Label(line,"Text",(i+1).ToString("00")+" "+source[i],V(.016f,.02f),V(.984f,.978f),
                    13,frame!=null&&frame.Line==i?TLSUiFactory.Pale:TLSUiFactory.Muted);
            }
            RectTransform graph=TLSUiFactory.Panel(root,"Tree topology and memory",V(.498f,.363f),V(.982f,.698f),TLSUiFactory.Deep);
            TLSTreeState state=frame==null?session.Current:frame.State;
            TLSPhase8TreeDiagram.Draw(graph,state,frame==null?-1:frame.Focus,redraw);
            RectTransform explanation=TLSUiFactory.Panel(root,"Tree explanation",V(.019f,.229f),V(.982f,.35f),TLSUiFactory.Slate);
            TLSUiFactory.Label(explanation,"Why",frame==null?"Select an operation. For general/binary insertion set VALUE and PARENT VALUE.":frame.Explanation,
                V(.023f,.38f),V(.975f,.965f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(explanation,"Invariant",trace==null?(string.IsNullOrEmpty(session.InputError)?"General tree: ordered children; binary tree: left/right slots; BST: key ordering.":session.InputError):
                (string.IsNullOrEmpty(session.InputError)?trace.Diagnostic:session.InputError),V(.023f,.035f),V(.975f,.39f),14,trace!=null&&!trace.Success?TLSUiFactory.Amber:TLSUiFactory.Green);
            RectTransform ctrl=TLSUiFactory.Panel(root,"Tree timeline controls",V(.019f,.016f),V(.982f,.215f),TLSUiFactory.Slate);
            TLSUiFactory.Label(ctrl,"Inputs","VALUE / TARGET                                 PARENT VALUE (for child operations)",
                V(.017f,.78f),V(.553f,.98f),13,TLSUiFactory.Muted);
            TLSUiFactory.Field(ctrl,"Tree value input",value(),"value",V(.017f,.515f),V(.19f,.774f),setValue);
            TLSUiFactory.Field(ctrl,"Tree parent input",parent(),"parent",V(.204f,.515f),V(.379f,.774f),setParent);
            TLSUiFactory.MakeButton(ctrl,"< BACK",V(.39f,.518f),V(.527f,.778f),()=>{session.Previous();redraw();},TLSUiFactory.Deep,14,trace!=null&&session.Step>0);
            TLSUiFactory.MakeButton(ctrl,"NEXT >",V(.538f,.518f),V(.677f,.778f),()=>{session.Next();redraw();},TLSUiFactory.Deep,14,trace!=null&&session.Step<trace.Frames.Count-1);
            TLSUiFactory.MakeButton(ctrl,"FINISH",V(.688f,.518f),V(.815f,.778f),()=>{session.Finish();redraw();},TLSUiFactory.Deep,14,trace!=null&&session.Step<trace.Frames.Count-1);
            TLSUiFactory.MakeButton(ctrl,"RESET TRACE",V(.829f,.518f),V(.983f,.778f),()=>{session.ResetTrace();redraw();},TLSUiFactory.Deep,14,trace!=null);
            TLSUiFactory.Label(ctrl,"Step",trace==null?"Choose an operation to see its causal steps.":
                "STEP "+session.Step+"/"+(trace.Frames.Count-1)+"  |  Modeled pointer changes, commit on FINISH.",
                V(.016f,.302f),V(.718f,.492f),15,TLSUiFactory.Cyan);
            TLSUiFactory.Label(ctrl,"Prediction",session.Prediction,V(.017f,.012f),V(.670f,.292f),13,TLSUiFactory.Amber);
            bool canPredict=trace!=null&&session.Step==0&&!session.Predicted;
            TLSUiFactory.MakeButton(ctrl,"MORE",V(.682f,.046f),V(.775f,.285f),()=>{session.Predict(1);redraw();},TLSUiFactory.Deep,12,canPredict);
            TLSUiFactory.MakeButton(ctrl,"FEWER",V(.785f,.046f),V(.873f,.285f),()=>{session.Predict(-1);redraw();},TLSUiFactory.Deep,12,canPredict);
            TLSUiFactory.MakeButton(ctrl,"SAME",V(.883f,.046f),V(.98f,.285f),()=>{session.Predict(0);redraw();},TLSUiFactory.Deep,12,canPredict);
        }
    }
}
