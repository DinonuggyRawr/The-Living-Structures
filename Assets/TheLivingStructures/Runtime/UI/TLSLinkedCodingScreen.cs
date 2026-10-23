using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;
namespace TLS.UI
{
    public static class TLSLinkedCodingScreen
    {
        private static Vector2 V(float x,float y){return new Vector2(x,y);}
        public static void Render(Transform root,TLSLinkedCodingSession session,Action redraw,Action back)
        {
            TLSUiFactory.Panel(root,"Linked code practice background",V(0,0),V(1,0.907f),TLSUiFactory.Navy);
            RectTransform tabs=TLSUiFactory.Panel(root,"Linked practice categories",V(0.038f,0.799f),V(0.962f,0.885f),TLSUiFactory.Slate);
            for(int i=0;i<4;i++)
            {
                TLSLinkedTaskType type=(TLSLinkedTaskType)i;float x=0.01f+i*0.246f;
                TLSUiFactory.MakeButton(tabs,type.ToString().ToUpperInvariant()+" C",V(x,0.12f),V(x+0.231f,0.89f),
                    ()=>{session.Select(type,0);redraw();},session.Type==type?TLSUiFactory.Violet:TLSUiFactory.Deep,17);
            }
            RectTransform sheet=TLSUiFactory.Panel(root,"Linked C practice sheet",V(0.066f,0.175f),V(0.934f,0.788f),TLSUiFactory.Slate);
            TLSLinkedTask task=session.Selected;
            TLSUiFactory.Label(sheet,"Practice title",task.Title.ToUpperInvariant()+"  /  "+(session.Index+1)+" of "+TLSLinkedCodingCatalog.Count(session.Type),
                V(0.035f,0.898f),V(0.965f,0.973f),25,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(sheet,"Practice question",task.Question,V(0.035f,0.643f),V(0.967f,0.889f),21,TLSUiFactory.Pale);
            TLSUiFactory.Field(sheet,"Linked practice response",session.Answer,"C statement or trace answer",V(0.035f,0.486f),V(0.967f,0.62f),s=>session.Answer=s);
            TLSUiFactory.MakeButton(sheet,"GUIDED",V(0.035f,0.391f),V(0.222f,0.471f),()=>{session.SetGuided(true);redraw();},
                session.Guided?TLSUiFactory.Violet:TLSUiFactory.Deep,17);
            TLSUiFactory.MakeButton(sheet,"INDEPENDENT",V(0.238f,0.391f),V(0.461f,0.471f),()=>{session.SetGuided(false);redraw();},
                !session.Guided?TLSUiFactory.Violet:TLSUiFactory.Deep,16);
            TLSUiFactory.MakeButton(sheet,"HINT",V(0.477f,0.391f),V(0.645f,0.471f),()=>{session.ShowHint();redraw();},TLSUiFactory.Deep,16,session.Guided);
            TLSUiFactory.MakeButton(sheet,"REVEAL / REVIEW",V(0.661f,0.391f),V(0.962f,0.471f),()=>{session.Reveal();redraw();},TLSUiFactory.Deep,16);
            TLSUiFactory.Label(sheet,"Help line",session.ReferenceVisible?"Reference: "+task.Answer:
                session.HintVisible?"Hint: "+task.Hint:"Try the exercise before revealing the reference; hints invalidate unseen credit.",
                V(0.035f,0.255f),V(0.966f,0.384f),16,TLSUiFactory.Amber);
            TLSUiFactory.Label(sheet,"Feedback",session.Feedback,V(0.035f,0.14f),V(0.963f,0.254f),17,TLSUiFactory.Pale);
            TLSUiFactory.MakeButton(sheet,"CHECK",V(0.035f,0.025f),V(0.268f,0.124f),()=>{session.Check();redraw();},TLSUiFactory.Violet,17);
            TLSUiFactory.MakeButton(sheet,"PREVIOUS",V(0.285f,0.025f),V(0.511f,0.124f),()=>{session.Select(session.Type,Math.Max(0,session.Index-1));redraw();},TLSUiFactory.Deep,16,session.Index>0);
            TLSUiFactory.MakeButton(sheet,"NEXT QUESTION",V(0.528f,0.025f),V(0.776f,0.124f),()=>{session.Select(session.Type,Math.Min(TLSLinkedCodingCatalog.Count(session.Type)-1,session.Index+1));redraw();},TLSUiFactory.Deep,16,session.Index+1<TLSLinkedCodingCatalog.Count(session.Type));
            TLSUiFactory.MakeButton(sheet,"BACK TO LIST",V(0.791f,0.025f),V(0.966f,0.124f),back,TLSUiFactory.Deep,15);
            RectTransform footer=TLSUiFactory.Panel(root,"Linked practice footer",V(0.066f,0.024f),V(0.934f,0.155f),TLSUiFactory.Deep);
            TLSUiFactory.Label(footer,"Learning evidence","Session solved: "+session.SolvedCount+"/"+TLSLinkedCodingCatalog.All.Length+
                "   |   Unseen independent first passes: "+session.IndependentCount,
                V(0.028f,0.50f),V(0.965f,0.965f),18,TLSUiFactory.Cyan);
            TLSUiFactory.Label(footer,"Scope","Structured reference-token checks are NOT C compilation or independent full-function mastery.",
                V(0.028f,0.04f),V(0.965f,0.49f),15,TLSUiFactory.Amber);
        }
    }
}
