using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>Phase 7 uGUI pages. No modifications to the Phase 1–6 labs or their legacy progress.</summary>
    public static class TLSPhase7Screens
    {
        // The Unity project ships no fonts. Request a system monospace font for code
        // and gracefully fall back to the existing Unity UI font if unavailable.
        private static Font editorFont;
        private static Font EditorFont
        {
            get
            {
                if (editorFont == null)
                {
                    try
                    {
                        editorFont = Font.CreateDynamicFontFromOSFont(
                            new[] { "Consolas", "Cascadia Mono", "Courier New", "Liberation Mono" }, 20);
                    }
                    catch (Exception) { editorFont = null; }
                    if (editorFont == null) editorFont = TLSUiFactory.Font;
                }
                return editorFont;
            }
        }
        private static Vector2 V(float x,float y){return new Vector2(x,y);}
        private static RectTransform P(Transform t,string id,float l,float b,float r,float u,Color c)
        {return TLSUiFactory.Panel(t,id,V(l,b),V(r,u),c);}
        private static Text L(Transform t,string id,string text,float l,float b,float r,float u,int size=16,Color? color=null,bool bold=false)
        {return TLSUiFactory.Label(t,id,text,V(l,b),V(r,u),size,color??TLSUiFactory.Pale,TextAnchor.MiddleLeft,bold);}
        private static void B(Transform t,string caption,float l,float b,float r,float u,Action action,bool active=false,int size=15,bool enabled=true)
        {TLSUiFactory.MakeButton(t,caption,V(l,b),V(r,u),action,active?TLSUiFactory.Violet:TLSUiFactory.Deep,size,enabled);}
        public static void Menu(Transform root,int current,Action dashboard,Action exam,Action reconstruction,Action tests,Action coding)
        {
            P(root,"Phase7 background",0,0,1,.906f,TLSUiFactory.Navy);
            RectTransform m=P(root,"Phase7 navigation",.018f,.806f,.982f,.891f,TLSUiFactory.Slate);
            string[] captions={"STUDY PATH","EXAM HALL","REBUILD C","DESIGN TESTS","WRITE REAL C11"};
            Action[] a={dashboard,exam,reconstruction,tests,coding};
            for(int i=0;i<captions.Length;i++)
            {float x=.012f+i*.198f;B(m,captions[i],x,.13f,x+.187f,.88f,a[i],current==i,13);}
        }
        public static void Dashboard(Transform root,TLSPhase7ProgressBook progress,int selected,
            Action<int> pick,Action<int> enterTopic,Action exam,Action reconstruction,Action tests,Action coding,Action redraw,bool invalidRecord=false)
        {
            RectTransform list=P(root,"Phase7 topic directory",.019f,.160f,.382f,.791f,TLSUiFactory.Slate);
            L(list,"Topics heading","COMPUTING II / SUBJECT PATH",.045f,.906f,.965f,.985f,19,TLSUiFactory.Cyan,true);
            L(list,"Prerequisites note","Recommended prerequisites; no lessons are locked.",.045f,.835f,.96f,.900f,13,TLSUiFactory.Muted);
            int page=selected/7;
            B(list,"< SUBJECTS",.045f,.752f,.46f,.824f,()=>pick(page==0?7:0),false,12);
            B(list,"SUBJECTS >",.54f,.752f,.959f,.824f,()=>pick(page==0?7:0),false,12);
            for(int i=0;i<7;i++)
            {
                int at=page*7+i;
                if(at>=TLSPhase7LearningCatalog.Topics.Length)continue;
                var topic=TLSPhase7LearningCatalog.Topics[at];
                var evidence=progress.Topic(topic.Id);
                float top=.731f-i*.096f;
                string state=evidence.independent>0?"[INDEPENDENT EVIDENCE]":evidence.guided>0?"[GUIDED]":evidence.views>0?"[VIEWED]":"[UNTRACKED]";
                B(list,topic.Name.ToUpperInvariant()+" "+state,.045f,top-.081f,.963f,top,()=>{pick(at);redraw();},selected==at,11);
            }
            var t=TLSPhase7LearningCatalog.Topics[selected];var e=progress.Topic(t.Id);
            RectTransform content=P(root,"Phase7 selected subject",.396f,.292f,.981f,.791f,TLSUiFactory.Slate);
            L(content,"Subject header",t.Hub.ToUpperInvariant(),.043f,.882f,.97f,.971f,20,TLSUiFactory.Cyan,true);
            L(content,"Subject title",t.Name,.043f,.790f,.967f,.889f,24,TLSUiFactory.Pale,true);
            L(content,"Prerequisite","SUGGESTED FIRST: "+t.Prerequisite,.044f,.714f,.962f,.781f,15,TLSUiFactory.Amber);
            L(content,"Subject objective",t.Objective,.044f,.560f,.961f,.707f,18);
            L(content,"Subject evidence","Phase 7 observed actions: VIEWED "+e.views+"  |  GUIDED "+e.guided+
                "  |  INDEPENDENT "+e.independent+"  |  WASM FIRST PASS "+e.compilerCases+
                "\nMISTAKES RECORDED "+e.misconceptions+"  |  STUDENT-DESIGNED TESTS "+e.testDesign,
                .044f,.330f,.961f,.552f,15,TLSUiFactory.Muted);
            L(content,"Review hint","REVIEW FOCUS: "+t.ReviewHint,.044f,.201f,.960f,.314f,14,TLSUiFactory.Amber);
            B(content,"OPEN TOPIC LAB",.043f,.057f,.44f,.190f,()=>enterTopic(selected),true,16);
            B(content,"INDEPENDENT CHECKUP",.462f,.057f,.963f,.190f,exam,false,15);
            RectTransform summary=P(root,"Phase7 progress and review",.396f,.163f,.981f,.280f,TLSUiFactory.Deep);
            int viewed=0,assisted=0,independent=0,compiler=0;
            foreach(var item in progress.Data.topics){if(item.views>0)viewed++;if(item.guided>0)assisted++;if(item.independent>0)independent++;compiler+=item.compilerCases;}
            L(summary,"Dashboard totals","TOPICS VIEWED "+viewed+"/13   GUIDED "+assisted+"   INDEPENDENT EVIDENCE "+independent+
                "   WASM FIRST PASSES "+compiler,.026f,.475f,.974f,.968f,15,TLSUiFactory.Pale,true);
            string recommended=progress.Recommendation(DateTime.UtcNow);
            L(summary,"Recommended next","RECOMMENDED: "+TLSPhase7LearningCatalog.Topics[TLSPhase7LearningCatalog.Find(recommended)].Name+
                "  |  REVIEWS DUE "+progress.DueCount(DateTime.UtcNow)+"  |  Former lab progress: not imported",.026f,.076f,.975f,.475f,13,TLSUiFactory.Muted);
            RectTransform foot=P(root,"Phase7 dashboard explanation",.019f,.02f,.981f,.148f,TLSUiFactory.Deep);
            L(foot,"Evidence caveat",invalidRecord ?
                "SAVED PHASE 7 RECORD IS INVALID OR INCOMPATIBLE. No new Phase 7 evidence will be written. Back up your PlayerPrefs before repair.\nExisting Phase 1–6 learning progress remains untouched.":
                "A view is NOT a completion. Guided answers, first-unseen attempts and compiler-backed practice are separate evidence.\nNo automatic mastery is awarded; earlier progress is preserved under its existing save keys.",.025f,.17f,.968f,.90f,15,TLSUiFactory.Amber);
        }
        private static void Question(Transform root,TLSPhase7Question q,bool answered,string feedback,
            Action<string> choose,Action next,string nextText)
        {
            RectTransform main=P(root,"Phase7 exam question",.020f,.289f,.632f,.684f,TLSUiFactory.Slate);
            L(main,"Topic","TOPIC: "+TLSPhase7LearningCatalog.Topics[TLSPhase7LearningCatalog.Find(q.TopicId)].Name.ToUpperInvariant(),.041f,.864f,.963f,.968f,16,TLSUiFactory.Cyan,true);
            L(main,"Question",q.Prompt,.044f,.635f,.957f,.854f,19,TLSUiFactory.Pale,true);
            B(main,"A  "+q.OptionA,.043f,.449f,.964f,.608f,()=>choose("A"),false,15,!answered);
            B(main,"B  "+q.OptionB,.043f,.260f,.964f,.420f,()=>choose("B"),false,15,!answered);
            B(main,"C  "+q.OptionC,.043f,.071f,.964f,.232f,()=>choose("C"),false,15,!answered);
            RectTransform result=P(root,"Phase7 exam feedback",.648f,.289f,.980f,.684f,TLSUiFactory.Slate);
            L(result,"Answer reveal state",answered?"ANSWER CHECKED":"UNSEEN QUESTION",.047f,.844f,.953f,.965f,19,TLSUiFactory.Cyan,true);
            L(result,"Exam feedback",feedback,.048f,.403f,.956f,.843f,17,TLSUiFactory.Pale);
            L(result,"Reasoning policy",answered?"Feedback is given AFTER the answer.":
                "No hints or visual trace here. Use course labs for preparation.",.049f,.230f,.956f,.395f,14,TLSUiFactory.Muted);
            B(result,nextText,.05f,.055f,.951f,.211f,next,true,15,answered);
        }
        public static void ExamRoom(Transform root,TLSPhase7Assessment diagnostic,TLSPhase7Assessment final,TLSPhase7ReviewSession review,
            int selectedMode,Action<int> setMode,Action<string> submit,Action next,Action redraw,TLSPhase7ProgressBook book)
        {
            RectTransform tabs=P(root,"Phase7 exam tabs",.019f,.697f,.981f,.791f,TLSUiFactory.Slate);
            string[] names={"BASELINE DIAGNOSTIC","FINAL TRANSFER CHECK","MIXED REVIEW"};
            for(int i=0;i<3;i++)
            {int which=i;float x=.017f+i*.330f;B(tabs,names[i],x,.11f,x+.308f,.88f,()=>{setMode(which);redraw();},selectedMode==i,15);}
            if(selectedMode==2)
                Question(root,review.Current,review.Answered,review.Feedback,submit,next,"NEXT REVIEW TOPIC");
            else
            {
                var assessment=selectedMode==0?diagnostic:final;
                if(!assessment.Complete)Question(root,assessment.Current,assessment.Answered,assessment.Feedback,
                    submit,next,assessment.Index==12?"FINISH CHECKUP":"NEXT QUESTION");
                else
                {
                    RectTransform result=P(root,"Phase7 assessment completion",.019f,.283f,.981f,.686f,TLSUiFactory.Slate);
                    L(result,"Finished","CHECKUP COMPLETE: "+assessment.CorrectCount+"/13",.040f,.776f,.952f,.974f,24,TLSUiFactory.Green,true);
                    L(result,"Meaning","This short conceptual check provides formative evidence only.\nIt is not a proctored exam, compiler-backed implementation score, or certified C mastery.",.041f,.42f,.965f,.76f,19);
                    L(result,"Record","The FIRST completed baseline/final summary is preserved locally in Phase 7's separate save key.",.041f,.15f,.957f,.399f,16,TLSUiFactory.Amber);
                }
            }
            RectTransform foot=P(root,"Phase7 exam information",.019f,.024f,.981f,.274f,TLSUiFactory.Deep);
            string mode=selectedMode==0?"BASELINE — BEFORE LESSONS":selectedMode==1?"TRANSFER — DIFFERENT QUESTIONS":"REVIEW — MIXED RETRIEVAL";
            L(foot,"Assessment name",mode,.03f,.690f,.966f,.938f,19,TLSUiFactory.Cyan,true);
            L(foot,"Assessment description","Answer without simulator assistance, then inspect causal feedback. Wrong answers identify a specific concept to revisit.\nRepeated known questions do not earn fresh first-unseen credit. Due reviews appear when the Academy reopens.",.03f,.242f,.962f,.683f,16);
            L(foot,"Saved scores","Recorded first checkups: baseline "+(book.Data.diagnosticCorrect<0?"UNTRACKED":book.Data.diagnosticCorrect+"/13")+
                "  |  final "+(book.Data.finalCorrect<0?"UNTRACKED":book.Data.finalCorrect+"/13"),.03f,.038f,.966f,.231f,15,TLSUiFactory.Amber);
        }
        public static void Reconstruction(Transform root,TLSPhase7ReconstructionSession practice,string order,
            Action<string> setOrder,Action redraw,Action submit,Action save)
        {
            RectTransform selector=P(root,"Phase7 reconstruction selector",.019f,.704f,.981f,.788f,TLSUiFactory.Slate);
            for(int i=0;i<TLSPhase7ReconstructionCatalog.Cards.Length;i++)
            {int at=i;float x=.011f+i*.198f;B(selector,TLSPhase7ReconstructionCatalog.Cards[i].Topic.ToUpperInvariant(),x,.12f,x+.184f,.88f,
                ()=>{practice.Select(at);setOrder("");redraw();},practice.Index==i,14);}
            var card=practice.Current;
            RectTransform left=P(root,"Phase7 reconstruction statements",.018f,.279f,.633f,.689f,TLSUiFactory.Slate);
            L(left,"Reconstruction title","RECONSTRUCT C STATEMENTS",.042f,.858f,.96f,.971f,19,TLSUiFactory.Cyan,true);
            L(left,"Reconstruction objective",card.Prompt,.042f,.739f,.96f,.856f,16);
            for(int i=0;i<card.Shuffled.Length;i++)
            {float top=.707f-i*.195f;L(left,"Statement "+i,(i+1)+".  "+card.Shuffled[i],.055f,top-.174f,.972f,top,17);}
            RectTransform right=P(root,"Phase7 reconstruction answer",.648f,.279f,.981f,.689f,TLSUiFactory.Slate);
            L(right,"Reconstruction mode",practice.Guided?"GUIDED RECONSTRUCTION":"INDEPENDENT RECONSTRUCTION",.049f,.870f,.962f,.97f,16,TLSUiFactory.Cyan,true);
            B(right,practice.Guided?"SWITCH TO INDEPENDENT":"SWITCH TO GUIDED",.050f,.747f,.950f,.851f,
                ()=>{practice.SetGuided(!practice.Guided);redraw();},false,13);
            B(right,"SHOW HINT",.052f,.625f,.48f,.729f,()=>{practice.Hint();redraw();},false,13,practice.Guided);
            B(right,"REVEAL ORDER",.507f,.625f,.949f,.729f,()=>{practice.Reveal();redraw();},false,13);
            L(right,"Reconstruction instructions","Enter a permutation of the statement numbers, separated by commas.",.052f,.481f,.951f,.618f,14);
            TLSUiFactory.Field(right,"Order answer",order,"3,2,1",V(.052f,.351f),V(.685f,.473f),setOrder);
            B(right,"CHECK ORDER",.706f,.348f,.948f,.472f,()=>{submit();redraw();},true,13);
            L(right,"Reconstruction feedback",practice.Feedback,.054f,.029f,.95f,.335f,15,TLSUiFactory.Pale);
            RectTransform foot=P(root,"Phase7 reconstruction footer",.019f,.023f,.981f,.264f,TLSUiFactory.Deep);
            L(foot,"Reconstruction caveat","FADED SCAFFOLDING: order statements first, then write and compile whole functions independently.\nA correct order is useful practice but does not prove syntax, correctness or memory safety in C11.",.038f,.31f,.955f,.89f,17);
            L(foot,"Reconstruction feedback rule","Hints, revealed answers and unsuccessful first submissions block unseen first-attempt credit for that exercise.",.038f,.052f,.96f,.300f,14,TLSUiFactory.Amber);
        }
        public static void TestDesign(Transform root,TLSPhase7TestDesignSession session,string csv,string arg,string expected,
            Action<string> csvChanged,Action<string> argChanged,Action<string> expectedChanged,Action redraw,Action submit)
        {
            RectTransform picks=P(root,"Phase7 test-case selector",.019f,.704f,.981f,.789f,TLSUiFactory.Slate);
            for(int i=0;i<TLSPhase7TestDesign.Cases.Length;i++)
            {int at=i;float x=.012f+i*.33f;B(picks,TLSPhase7TestDesign.Cases[i].Title.ToUpperInvariant(),x,.12f,x+.312f,.875f,
                ()=>{session.Select(at);redraw();},session.Selected==i,13);}
            var scenario=TLSPhase7TestDesign.Cases[session.Selected];
            RectTransform left=P(root,"Phase7 test creator",.019f,.282f,.578f,.690f,TLSUiFactory.Slate);
            L(left,"Test goal","DESIGN A TEST THAT EXPOSES A BUG",.045f,.858f,.965f,.972f,20,TLSUiFactory.Cyan,true);
            L(left,"Test prompt",scenario.Prompt,.045f,.681f,.966f,.856f,17);
            L(left,"Input csv label",session.Selected==2?"INITIAL QUEUE VALUES (max 4)":"INPUT ARRAY (1–6 integers)",.043f,.567f,.943f,.667f,14,TLSUiFactory.Amber);
            TLSUiFactory.Field(left,"Student test values",csv,"1,2,3",V(.043f,.441f),V(.953f,.566f),csvChanged);
            L(left,"Test argument label",session.Selected==2?"HEAD INDEX (0–4)":"TARGET VALUE",.043f,.330f,.443f,.429f,14);
            L(left,"Reference oracle label",session.Selected==2?"EXPECTED PHYSICAL TAIL":"EXPECTED RETURN INDEX",.509f,.330f,.952f,.429f,14);
            TLSUiFactory.Field(left,"Student case argument",arg,"target / head",V(.043f,.197f),V(.471f,.325f),argChanged);
            TLSUiFactory.Field(left,"Student expected output",expected,"reference result",V(.514f,.197f),V(.955f,.325f),expectedChanged);
            B(left,"RUN MY TEST",.043f,.037f,.953f,.169f,()=>{submit();redraw();},true,16);
            RectTransform right=P(root,"Phase7 bug comparison",.590f,.282f,.982f,.690f,TLSUiFactory.Slate);
            L(right,"Test result heading","REFERENCE VS SEEDED BUG",.043f,.85f,.952f,.973f,19,TLSUiFactory.Cyan,true);
            B(right,session.Guided?"MODE: GUIDED":"MODE: INDEPENDENT",.044f,.722f,.95f,.838f,
                ()=>{session.SetGuided(!session.Guided);redraw();},false,14);
            B(right,"CONTEXTUAL HINT",.044f,.603f,.95f,.707f,()=>{session.Hint();redraw();},false,14,session.Guided);
            L(right,"Reference value",session.Latest==null?"REFERENCE: hidden until RUN":"REFERENCE: "+session.Latest.Reference,.050f,.455f,.947f,.581f,16,TLSUiFactory.Green);
            L(right,"Faulty value",session.Latest==null?"SEEDED BUG: hidden until RUN":"SEEDED BUG: "+session.Latest.Faulty,.050f,.319f,.948f,.456f,16,TLSUiFactory.Amber);
            L(right,"Test feedback",session.Feedback,.050f,.027f,.952f,.308f,15);
            RectTransform foot=P(root,"Phase7 student test explanation",.019f,.022f,.981f,.267f,TLSUiFactory.Deep);
            L(foot,"Test philosophy","DON'T ONLY TEST THE HAPPY PATH",.038f,.756f,.967f,.952f,20,TLSUiFactory.Cyan,true);
            L(foot,"Test policy","A useful student-authored test must predict the reference behavior correctly AND distinguish a specific wrong implementation.\nThese are deterministic teaching fixtures—not executable student unit tests or a substitute for C11 compiler-backed testing.",.039f,.277f,.961f,.738f,17);
            L(foot,"Test record scope","Only valid first-unseen independent discriminating cases earn separate test-design evidence; no saved mastery is implied.",.039f,.064f,.964f,.254f,14,TLSUiFactory.Amber);
        }
        public static void Transfer(Transform root,TLSPhase7FunctionSession practice,TLSPhase7WasmSession wasm,
            TLSPhase7ProgressBook book,Action redraw,Action returnToStudy)
        {
            RectTransform tabs=P(root,"Phase7 transfer function tabs",.019f,.697f,.981f,.791f,TLSUiFactory.Slate);
            for(int i=0;i<TLSPhase7FunctionCatalog.Count;i++)
            {TLSPhase7Function which=(TLSPhase7Function)i;float x=.015f+i*.49f;
                B(tabs,TLSPhase7FunctionCatalog.Name(which),x,.10f,x+.470f,.89f,
                    ()=>{practice.Select(which);redraw();},practice.Selected==which,16,!wasm.Busy);}
            RectTransform editor=P(root,"Phase7 C transfer editor",.019f,.160f,.623f,.683f,TLSUiFactory.Slate);
            L(editor,"Transfer heading","UNSEEN C11 TRANSFER — "+TLSPhase7FunctionCatalog.Name(practice.Selected),.037f,.915f,.971f,.983f,19,TLSUiFactory.Cyan,true);
            L(editor,"Transfer objective",TLSPhase7FunctionCatalog.Objective(practice.Selected),.038f,.817f,.965f,.908f,15);
            L(editor,"Transfer hints",practice.ReferenceVisible?"REFERENCE SHOWN — no unseen first-attempt evidence.":
                practice.HintVisible?"HINT: "+TLSPhase7FunctionCatalog.Hint(practice.Selected):
                "Write the complete C11 function. Independent mode hides hints and reference examples.",
                .037f,.734f,.963f,.808f,14,TLSUiFactory.Amber);
            Text tally=null,status=null;
            Text[] testLabels=new Text[TLSPhase7WasmRunner.CasesPerFunction];
            InputField field=TLSUiFactory.Field(editor,"Phase7 C source",practice.Draft,"Complete C11 function",V(.037f,.175f),V(.965f,.725f),s=>
            {
                practice.Draft=s;
                if(wasm.Latest!=null&&(s!=wasm.LastSource||wasm.LastFunction!=practice.Selected))
                {if(tally!=null)tally.text="SOURCE EDITED — RETEST REQUIRED";
                 if(status!=null)status.text="Old result hidden; run compiler tests again.";
                 for(int k=0;k<testLabels.Length;k++)if(testLabels[k]!=null)testLabels[k].text=(k+1)+". —";}
            });
            // TLSUiFactory.Field initializes its text with SingleLine defaults.
            // Switch modes first, then restore the EXACT authored code (including
            // newlines) without invoking onValueChanged or changing attempt evidence.
            field.lineType=InputField.LineType.MultiLineNewline;
            field.characterLimit=TLSPhase7WasmRunner.SourceLimit;
            field.textComponent.font=EditorFont;
            field.textComponent.fontSize=20;
            field.textComponent.lineSpacing=1.12f;
            field.textComponent.supportRichText=false; // Literal C tokens, not markup.
            field.textComponent.alignment=TextAnchor.UpperLeft;
            field.textComponent.horizontalOverflow=HorizontalWrapMode.Wrap;
            field.textComponent.verticalOverflow=VerticalWrapMode.Truncate;
            field.SetTextWithoutNotify(practice.Draft);
            field.interactable=!wasm.Busy;
            B(editor,"RUN C11 / WASM",.035f,.034f,.450f,.162f,()=>{wasm.Start(practice,book,Application.dataPath);redraw();},true,16,wasm.OptedIn&&!wasm.Busy);
            B(editor,"RESET DRAFT",.464f,.034f,.723f,.162f,()=>{practice.Restore();redraw();},false,14,!wasm.Busy);
            B(editor,wasm.Busy?"CANCEL RUN":"COPY DIAGNOSTICS",.734f,.034f,.969f,.162f,
                ()=>{if(wasm.Busy)wasm.Cancel();else if(wasm.Latest!=null)GUIUtility.systemCopyBuffer=wasm.Latest.Diagnostic;redraw();},false,12);
            RectTransform results=P(root,"Phase7 WASM test evidence",.637f,.160f,.982f,.683f,TLSUiFactory.Slate);
            L(results,"Runner heading","REAL C11 / WEBASSEMBLY",.049f,.927f,.96f,.982f,18,TLSUiFactory.Cyan,true);
            L(results,"Safety caveat","Trusted native Zig + bounded Wasmtime. Only your own learning code; finite tests, not public security certification.",.048f,.850f,.961f,.920f,12,TLSUiFactory.Amber);
            B(results,wasm.OptedIn?"DISABLE WASM":"ENABLE WASM (OPT IN)",.045f,.766f,.950f,.845f,
                ()=>{if(wasm.OptedIn)wasm.Disable();else wasm.Enable();redraw();},false,14,!wasm.Busy);
            B(results,practice.Guided?"MODE: GUIDED":"MODE: INDEPENDENT",.047f,.688f,.493f,.752f,
                ()=>{practice.SetGuided(!practice.Guided);redraw();},false,13,!wasm.Busy);
            B(results,"HINT",.516f,.688f,.949f,.752f,()=>{practice.Hint();redraw();},false,13,practice.Guided&&!wasm.Busy);
            B(results,"REVEAL REFERENCE",.046f,.615f,.951f,.676f,()=>{practice.Reveal();redraw();},false,13,!wasm.Busy);
            bool stale=wasm.Latest!=null&&(wasm.LastFunction!=practice.Selected||wasm.LastSource!=practice.Draft);
            var r=stale?null:wasm.Latest;
            tally=L(results,"Transfer tally",r==null?"NO CURRENT C11 PASS RESULT":r.Passed+"/"+r.Total+" REAL C11 TESTS PASSED",
                .044f,.547f,.952f,.614f,18,r!=null&&r.AllPassed?TLSUiFactory.Green:TLSUiFactory.Pale,true);
            for(int i=0;i<TLSPhase7WasmRunner.CasesPerFunction;i++)
            {int at=i;float top=.533f-i*.045f;
                testLabels[i]=L(results,"Transfer test "+at,(at+1)+". "+TLSPhase7WasmRunner.CaseName(practice.Selected,at)+"   "+
                    (r==null||!r.Completed?"—":r.Cases[at]?"PASS":"FAIL"),.050f,top-.039f,.950f,top,12,
                    r==null||!r.Completed?TLSUiFactory.Muted:r.Cases[at]?TLSUiFactory.Green:TLSUiFactory.Amber);
            }
            status=L(results,"WASM status",stale?"Edited since last test; old result hidden.":wasm.Status,.052f,.057f,.955f,.164f,12);
            L(results,"WASM errors",r==null?"Compiler errors and first failure appear here.":
                string.IsNullOrWhiteSpace(r.Diagnostic)?TLSPhase7WasmRunner.FirstFailureDescription(r):r.Diagnostic,
                .052f,.006f,.953f,.063f,11,TLSUiFactory.Muted);
            RectTransform footer=P(root,"Phase7 independent compiler footer",.019f,.024f,.982f,.147f,TLSUiFactory.Deep);
            L(footer,"Compiler evidences","SESSION: "+practice.Passed+"/2 functions passed finite cases; "+practice.Independent+
                " unseen independent first runs.  Saved Phase 7 evidence is separate.",.024f,.556f,.962f,.958f,15);
            L(footer,"Compiler meaning","Passing hidden cases is valuable evidence, NOT proof of mastery or memory safety. Retrying, hints and revealed references block first-unseen credit.",
                .025f,.092f,.704f,.532f,13,TLSUiFactory.Amber);
            B(footer,"RETURN TO STUDY PATH",.730f,.12f,.969f,.480f,returnToStudy,false,13,!wasm.Busy);
        }
    }
}
