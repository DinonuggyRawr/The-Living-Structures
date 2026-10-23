using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    public static class TLSPhase4WasmScreen
    {
        private static Vector2 V(float x,float y){return new Vector2(x,y);}
        public static void Render(Transform root,TLSPhase4FunctionLabSession practice,TLSPhase4WasmLabSession wasm,
            Action redraw,Action back)
        {
            TLSUiFactory.Panel(root,"Tree WASM background",V(0,0),V(1,0.907f),TLSUiFactory.Navy);
            RectTransform tabs=TLSUiFactory.Panel(root,"Tree C11 function tabs",V(0.019f,0.808f),V(0.982f,0.894f),TLSUiFactory.Slate);
            for(int i=0;i<TLSPhase4FunctionCatalog.Count;i++)
            {
                TLSPhase4Function selected=(TLSPhase4Function)i;
                float x=0.008f+i*0.165f;
                TLSUiFactory.MakeButton(tabs,TLSPhase4FunctionCatalog.Name(selected),V(x,0.10f),V(x+0.154f,0.89f),
                    ()=>{practice.Select(selected);redraw();},practice.Selected==selected?TLSUiFactory.Violet:TLSUiFactory.Deep,13,!wasm.Busy);
            }
            RectTransform editorPanel=TLSUiFactory.Panel(root,"Tree C editor",V(0.019f,0.163f),V(0.628f,0.798f),TLSUiFactory.Slate);
            TLSUiFactory.Label(editorPanel,"Function heading","WRITE BST / TREE C11 — "+TLSPhase4FunctionCatalog.Name(practice.Selected),
                V(0.029f,0.92f),V(0.974f,0.985f),20,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(editorPanel,"Function objective",TLSPhase4FunctionCatalog.Goal(practice.Selected),
                V(0.03f,0.819f),V(0.973f,0.913f),16,TLSUiFactory.Pale);
            TLSUiFactory.Label(editorPanel,"Function assistance",practice.ReferenceVisible?
                "Reference revealed; cannot earn unseen first-attempt credit for this function.":
                practice.HintVisible?"HINT: "+TLSPhase4FunctionCatalog.Hint(practice.Selected):
                "Implement real C11. TreeNode {int value; TreeNode *left, *right;} and stdlib.h are supplied; tests run isolated in WASM.",
                V(0.03f,0.735f),V(0.973f,0.812f),14,TLSUiFactory.Amber);
            Text headline=null, status=null;
            Text[] caseTexts=new Text[TLSPhase4WasmRunner.CasesPerFunction];
            InputField source=TLSUiFactory.Field(editorPanel,"Tree student C source",practice.Draft,
                "Type the complete C11 function",V(0.03f,0.160f),V(0.975f,0.73f),s=>
                {
                    practice.Draft=s;
                    if(wasm.Latest!=null&&(wasm.LastSource!=s||wasm.LastFunction!=practice.Selected))
                    {
                        if(headline!=null)headline.text="SOURCE EDITED — RETEST REQUIRED";
                        if(status!=null)status.text="Previous WASM result no longer applies to this draft.";
                        for(int j=0;j<caseTexts.Length;j++)if(caseTexts[j]!=null)caseTexts[j].text=(j+1)+". " +TLSPhase4WasmRunner.CaseName(practice.Selected,j)+"  —";
                    }
                });
            source.lineType=InputField.LineType.MultiLineNewline;
            source.characterLimit=TLSPhase4WasmRunner.SourceLimit;
            source.textComponent.fontSize=17;
            source.textComponent.alignment=TextAnchor.UpperLeft;
            source.textComponent.horizontalOverflow=HorizontalWrapMode.Wrap;
            source.textComponent.verticalOverflow=VerticalWrapMode.Truncate;
            source.interactable=!wasm.Busy;
            TLSUiFactory.MakeButton(editorPanel,"RUN REAL C11 WASM TESTS",V(0.03f,0.034f),V(0.435f,0.149f),
                ()=>{wasm.Start(practice,Application.dataPath);redraw();},TLSUiFactory.Violet,15,wasm.OptedIn&&!wasm.Busy);
            TLSUiFactory.MakeButton(editorPanel,"RESTORE STARTER",V(0.446f,0.034f),V(0.706f,0.149f),
                ()=>{practice.Restore();redraw();},TLSUiFactory.Deep,15,!wasm.Busy);
            TLSUiFactory.MakeButton(editorPanel,wasm.Busy?"CANCEL":"COPY DIAGNOSTICS",V(0.717f,0.034f),V(0.975f,0.149f),()=>
                {if(wasm.Busy)wasm.Cancel();else if(wasm.Latest!=null)GUIUtility.systemCopyBuffer=wasm.Latest.Diagnostic;redraw();},
                TLSUiFactory.Deep,13,wasm.Busy||(wasm.Latest!=null&&!string.IsNullOrWhiteSpace(wasm.Latest.Diagnostic)));
            RectTransform results=TLSUiFactory.Panel(root,"Tree C11 results",V(0.640f,0.163f),V(0.982f,0.798f),TLSUiFactory.Slate);
            TLSUiFactory.Label(results,"Results header","REAL C11 / WEBASSEMBLY",V(0.042f,0.923f),V(0.97f,0.988f),19,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(results,"WASM scope","BST + recursive binary-tree C11. Eight finite cases; not memory-safety proof or public sandbox.",
                V(0.042f,0.864f),V(0.97f,0.927f),13,TLSUiFactory.Amber);
            TLSUiFactory.MakeButton(results,wasm.OptedIn?"DISABLE WASM":"ENABLE WASM (OPT IN)",V(0.044f,0.792f),V(0.956f,0.865f),
                ()=>{if(wasm.OptedIn)wasm.Disable();else wasm.Enable();redraw();},TLSUiFactory.Deep,16,!wasm.Busy);
            TLSUiFactory.MakeButton(results,practice.Guided?"MODE: GUIDED":"MODE: INDEPENDENT",V(0.044f,0.718f),V(0.512f,0.78f),
                ()=>{practice.SetGuided(!practice.Guided);redraw();},TLSUiFactory.Deep,13,!wasm.Busy);
            TLSUiFactory.MakeButton(results,"SHOW HINT",V(0.526f,0.718f),V(0.956f,0.78f),
                ()=>{practice.Hint();redraw();},TLSUiFactory.Deep,13,practice.Guided&&!wasm.Busy);
            TLSUiFactory.MakeButton(results,"SHOW REFERENCE",V(0.044f,0.651f),V(0.956f,0.707f),
                ()=>{practice.Reveal();redraw();},TLSUiFactory.Deep,13,!wasm.Busy);
            bool stale=wasm.Latest!=null&&(wasm.LastFunction!=practice.Selected||wasm.LastSource!=practice.Draft);
            TLSPhase4WasmRunner.Result r=stale?null:wasm.Latest;
            headline=TLSUiFactory.Label(results,"Tree WASM tally",r==null?"NO CURRENT PASS RESULT":
                (r.Completed?r.Passed+" / "+r.Total+" C11 TESTS PASSED":"RUN INCOMPLETE"),
                V(0.044f,0.596f),V(0.956f,0.651f),18,r!=null&&r.AllPassed?TLSUiFactory.Green:TLSUiFactory.Pale,
                TextAnchor.MiddleLeft,true);
            for(int i=0;i<TLSPhase4WasmRunner.CasesPerFunction;i++)
            {
                string label=(i+1)+". "+TLSPhase4WasmRunner.CaseName(practice.Selected,i)+"    "+
                    (r==null||!r.Completed?"—":r.Cases[i]?"PASS":"FAIL");
                float top=0.585f-i*0.043f;
                caseTexts[i]=TLSUiFactory.Label(results,"Tree WASM case "+i,label,
                    V(0.047f,top-0.041f),V(0.964f,top),13,r==null||!r.Completed?TLSUiFactory.Muted:
                    r.Cases[i]?TLSUiFactory.Green:TLSUiFactory.Amber);
            }
            status=TLSUiFactory.Label(results,"Tree WASM status",stale?"Edited after last run: result hidden.":r==null?wasm.Status:r.Status,
                V(0.047f,0.085f),V(0.958f,0.147f),12,TLSUiFactory.Pale);
            TLSUiFactory.Label(results,"Tree WASM diagnostics",r==null?"Compiler diagnostics appear after RUN.":
                string.IsNullOrWhiteSpace(r.Diagnostic)?TLSPhase4WasmRunner.FirstFailureDescription(r):r.Diagnostic,
                V(0.047f,0.008f),V(0.958f,0.080f),12,TLSUiFactory.Muted);
            RectTransform footer=TLSUiFactory.Panel(root,"Tree C11 practice footer",V(0.019f,0.024f),V(0.982f,0.151f),TLSUiFactory.Deep);
            TLSUiFactory.Label(footer,"WASM evidence","Session: "+practice.PassedFunctions+"/6 functions passed finite C11 cases.  "+
                practice.IndependentFunctions+" unseen independent first passes.",V(0.025f,0.535f),V(0.97f,0.98f),16,TLSUiFactory.Pale);
            TLSUiFactory.Label(footer,"WASM learning scope","Reference, hint and retry use is tracked in-session. No saved mastery or full proof of correctness.",
                V(0.025f,0.22f),V(0.71f,0.54f),14,TLSUiFactory.Amber);
            TLSUiFactory.MakeButton(footer,"RETURN TO TREE LAB",V(0.712f,0.055f),V(0.976f,0.42f),back,TLSUiFactory.Slate,14,!wasm.Busy);
        }
    }
}
