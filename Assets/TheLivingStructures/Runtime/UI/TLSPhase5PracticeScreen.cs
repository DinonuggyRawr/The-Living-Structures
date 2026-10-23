using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;
namespace TLS.UI
{
    public static class TLSPhase5PracticeScreen
    {
        private static Vector2 V(float x,float y){return new Vector2(x,y);}
        public static void Render(Transform root,TLSPhase5PracticeSession s,Action redraw,Action back)
        {
            TLSUiFactory.Panel(root,"Phase5 coding practice background",V(0,0),V(1,.907f),TLSUiFactory.Navy);
            RectTransform domain=TLSUiFactory.Panel(root,"Phase5 practice domain tabs",V(.035f,.818f),V(.965f,.898f),TLSUiFactory.Slate);
            for(int i=0;i<3;i++)
            {TLSPhase5Domain d=(TLSPhase5Domain)i;float x=.013f+i*.329f;
                TLSUiFactory.MakeButton(domain,d+" PRACTICE",V(x,.11f),V(x+.313f,.9f),()=>{s.Select(d,TLSPhase5QuestionType.Trace,0);redraw();},s.Domain==d?TLSUiFactory.Violet:TLSUiFactory.Deep,15);}
            RectTransform tabs=TLSUiFactory.Panel(root,"Phase5 practice categories",V(.035f,.735f),V(.965f,.805f),TLSUiFactory.Slate);
            for(int i=0;i<4;i++)
            {TLSPhase5QuestionType t=(TLSPhase5QuestionType)i;float x=.009f+i*.248f;
                TLSUiFactory.MakeButton(tabs,t+" C",V(x,.11f),V(x+.237f,.9f),()=>{s.Select(s.Domain,t,0);redraw();},s.Type==t?TLSUiFactory.Violet:TLSUiFactory.Deep,16);}
            RectTransform sheet=TLSUiFactory.Panel(root,"Phase5 practice worksheet",V(.065f,.175f),V(.935f,.717f),TLSUiFactory.Slate);
            TLSPhase5Question q=s.Selected;
            TLSUiFactory.Label(sheet,"Heading",s.Domain.ToString().ToUpperInvariant()+"  /  "+q.Title+"  /  "+(s.Index+1)+" of "+TLSPhase5PracticeCatalog.Count(s.Domain,s.Type),
                V(.031f,.880f),V(.967f,.973f),22,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(sheet,"Question",q.Prompt,V(.031f,.623f),V(.969f,.870f),19,TLSUiFactory.Pale);
            TLSUiFactory.Field(sheet,"Phase5 C answer",s.Answer,"integer or exact C reference tokens",V(.032f,.469f),V(.967f,.606f),x=>s.Answer=x);
            TLSUiFactory.MakeButton(sheet,"GUIDED",V(.032f,.360f),V(.230f,.454f),()=>{s.SetGuided(true);redraw();},s.Guided?TLSUiFactory.Violet:TLSUiFactory.Deep,16);
            TLSUiFactory.MakeButton(sheet,"INDEPENDENT",V(.241f,.360f),V(.448f,.454f),()=>{s.SetGuided(false);redraw();},!s.Guided?TLSUiFactory.Violet:TLSUiFactory.Deep,16);
            TLSUiFactory.MakeButton(sheet,"HINT",V(.462f,.360f),V(.655f,.454f),()=>{s.Hint();redraw();},TLSUiFactory.Deep,16,s.Guided);
            TLSUiFactory.MakeButton(sheet,"REVEAL / REVIEW",V(.669f,.360f),V(.969f,.454f),()=>{s.Reveal();redraw();},TLSUiFactory.Deep,15);
            TLSUiFactory.Label(sheet,"Help",s.ReferenceVisible?"REFERENCE: "+q.Answer:s.HintVisible?"HINT: "+q.Hint:
                "Try before hints. Short answers use token reference matching; equivalent C statements may be rejected.",
                V(.032f,.244f),V(.97f,.349f),15,TLSUiFactory.Amber);
            TLSUiFactory.Label(sheet,"Feedback",s.Feedback,V(.032f,.132f),V(.968f,.244f),16,TLSUiFactory.Pale);
            TLSUiFactory.MakeButton(sheet,"CHECK ANSWER",V(.031f,.018f),V(.246f,.119f),()=>{s.Check();redraw();},TLSUiFactory.Violet,15);
            TLSUiFactory.MakeButton(sheet,"PREVIOUS",V(.266f,.018f),V(.473f,.119f),()=>{s.Select(s.Domain,s.Type,s.Index-1);redraw();},TLSUiFactory.Deep,15,s.Index>0);
            TLSUiFactory.MakeButton(sheet,"NEXT",V(.488f,.018f),V(.687f,.119f),()=>{s.Select(s.Domain,s.Type,s.Index+1);redraw();},TLSUiFactory.Deep,15,s.Index+1<TLSPhase5PracticeCatalog.Count(s.Domain,s.Type));
            TLSUiFactory.MakeButton(sheet,"RETURN TO LAB",V(.705f,.018f),V(.969f,.119f),back,TLSUiFactory.Deep,15);
            RectTransform footer=TLSUiFactory.Panel(root,"Phase5 practice footer",V(.065f,.023f),V(.935f,.155f),TLSUiFactory.Deep);
            TLSUiFactory.Label(footer,"Session result","SESSION: "+s.Solved+"/"+TLSPhase5PracticeCatalog.All.Length+" solved  |  "+s.Independent+" unseen independent first passes",
                V(.025f,.54f),V(.975f,.96f),17,TLSUiFactory.Cyan);
            TLSUiFactory.Label(footer,"Caution","Guided answers, reviewed references and these exact-token checks are not C implementation mastery. No progress saved.",
                V(.025f,.06f),V(.974f,.46f),14,TLSUiFactory.Amber);
        }
    }
}
