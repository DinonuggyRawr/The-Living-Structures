using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>Phase 6 learning screens, no saves and no measured-runtime claims.</summary>
    public static class TLSPhase6Screens
    {
        private static Vector2 V(float x,float y){return new Vector2(x,y);}
        private static RectTransform P(Transform parent,string name,float x0,float y0,float x1,float y1,Color color)
        {return TLSUiFactory.Panel(parent,name,V(x0,y0),V(x1,y1),color);}
        private static void L(Transform parent,string name,string value,float x0,float y0,float x1,float y1,int font,Color c)
        {TLSUiFactory.Label(parent,name,value,V(x0,y0),V(x1,y1),font,c,TextAnchor.MiddleLeft);}
        public static void Analysis(Transform root,TLSAnalysisSession session,Func<string> prediction,Action<string> setPrediction,Action redraw,Action debug,Action quality,Action wasm)
        {
            P(root,"Phase6 analysis background",0,0,1,.907f,TLSUiFactory.Navy);
            RectTransform tabs=P(root,"Phase6 analysis navigation",.017f,.818f,.983f,.892f,TLSUiFactory.Slate);
            TLSUiFactory.MakeButton(tabs,"BIG-O / OP COUNTS",V(.012f,.10f),V(.257f,.90f),redraw,TLSUiFactory.Violet,15);
            TLSUiFactory.MakeButton(tabs,"DEBUGGING & TESTS",V(.268f,.10f),V(.510f,.90f),debug,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"C CODE QUALITY",V(.524f,.10f),V(.754f,.90f),quality,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"WRITE C11",V(.764f,.10f),V(.986f,.90f),wasm,TLSUiFactory.Deep,15);
            RectTransform picks=P(root,"Complexity algorithm tabs",.017f,.746f,.983f,.813f,TLSUiFactory.Slate);
            for(int i=0;i<5;i++)
            {
                TLSComplexityAlgorithm a=(TLSComplexityAlgorithm)i;float x=.008f+i*.199f;
                TLSUiFactory.MakeButton(picks,TLSAnalysisEngine.Name(a),V(x,.10f),V(x+.189f,.90f),()=>{session.Select(a);redraw();},
                    session.Selected==a?TLSUiFactory.Violet:TLSUiFactory.Deep,11);
            }
            RectTransform plot=P(root,"Phase6 operation growth chart",.017f,.394f,.602f,.733f,TLSUiFactory.Deep);
            L(plot,"Growth title","OPERATION COUNTS VS. INPUT SIZE",.025f,.895f,.975f,.984f,20,TLSUiFactory.Cyan);
            L(plot,"Growth caveat","Defined C-like comparisons, reads, writes and visits. NOT runtime, NOT benchmark ticks.",.025f,.820f,.976f,.905f,12,TLSUiFactory.Amber);
            TLSAnalysisTrace[] series=new TLSAnalysisTrace[5];
            long largest=1;
            for(int i=0;i<5;i++){series[i]=TLSAnalysisEngine.Build((TLSComplexityAlgorithm)i,session.Size);largest=Math.Max(largest,series[i].End.Total);}
            for(int i=0;i<5;i++)
            {
                TLSAnalysisTrace t=series[i];float top=.792f-i*.142f;
                L(plot,"Growth name "+i,TLSAnalysisEngine.Name((TLSComplexityAlgorithm)i)+"  ["+t.GrowthClass+"]",.026f,top-.084f,.340f,top,12,TLSUiFactory.Pale);
                float fraction=(float)t.End.Total/largest;
                P(plot,"Growth bar track "+i,.352f,top-.073f,.838f,top-.025f,TLSUiFactory.Slate);
                if(fraction>.002f)P(plot,"Growth bar "+i,.352f,top-.073f,.352f+.486f*fraction,top-.025f,
                    i==(int)session.Selected?TLSUiFactory.Violet:TLSUiFactory.Cyan);
                L(plot,"Growth units "+i,t.End.Total.ToString(),.858f,top-.081f,.971f,top,14,TLSUiFactory.Pale);
            }
            L(plot,"Chart definition","All algorithms receive the same n; inputs intentionally differ by documented scenario.",.025f,.014f,.97f,.10f,12,TLSUiFactory.Muted);
            RectTransform model=P(root,"Phase6 operation timeline",.617f,.394f,.983f,.733f,TLSUiFactory.Slate);
            TLSAnalysisFrame f=session.Current;
            L(model,"Current step","CURRENT TRACE  "+(session.Trace==null?"NOT STARTED":session.Step+" / "+(session.Trace.Frames.Count-1)),.039f,.888f,.969f,.972f,20,TLSUiFactory.Cyan);
            L(model,"Operation counts",f==null?"BUILD TRACE to inspect cumulative operations.":f.Counts.ToString(),.038f,.763f,.967f,.885f,14,TLSUiFactory.Pale);
            L(model,"Why this step",f==null?TLSAnalysisEngine.Scenario(session.Selected):f.Explanation,.038f,.555f,.966f,.767f,16,TLSUiFactory.Pale);
            L(model,"C code sketch",TLSAnalysisEngine.CSketch(session.Selected),.038f,.467f,.966f,.555f,13,TLSUiFactory.Cyan);
            string arrayPreview="";
            if(f!=null)
            {
                int limit=Math.Min(f.Snapshot.Length,16);for(int i=0;i<limit;i++)arrayPreview+=(i>0?", ":"")+f.Snapshot[i];
                if(f.Snapshot.Length>limit)arrayPreview+=", ...";
            }
            L(model,"Array snapshot",f==null?"":("a[] = ["+arrayPreview+"]  | focus "+f.Focus),.036f,.316f,.968f,.462f,14,TLSUiFactory.Amber);
            L(model,"Model result",session.Feedback+"  |  Space: "+TLSAnalysisEngine.SpaceComplexity(session.Selected),.036f,.138f,.969f,.31f,13,TLSUiFactory.Muted);
            L(model,"Prediction feedback",session.PredictionFeedback,.036f,.012f,.967f,.140f,12,TLSUiFactory.Green);
            RectTransform tips=P(root,"Phase6 complexity principles",.017f,.213f,.983f,.381f,TLSUiFactory.Slate);
            L(tips,"Principal header","BIG-O IS A GROWTH BOUND, NOT A TIMER",.025f,.73f,.980f,.970f,19,TLSUiFactory.Cyan);
            L(tips,"Principle explanation","Search assumptions matter: binary search needs sorted data; hashing needs collision/load assumptions. " +
                "The growth class describes input scaling, not exact runtime. Reverse insertion-sort incurs repeated element shifts.",.025f,.10f,.969f,.738f,17,TLSUiFactory.Pale);
            RectTransform ctl=P(root,"Phase6 analysis controls",.017f,.014f,.983f,.203f,TLSUiFactory.Slate);
            L(ctl,"Selected size","INPUT n: "+session.Size,.026f,.793f,.17f,.960f,15,TLSUiFactory.Cyan);
            int[] sizes={4,8,16,32,64};
            for(int i=0;i<sizes.Length;i++)
            {int n=sizes[i];float x=.190f+i*.12f;
                TLSUiFactory.MakeButton(ctl,""+n,V(x,.768f),V(x+.107f,.953f),()=>{session.SetSize(n);redraw();},n==session.Size?TLSUiFactory.Violet:TLSUiFactory.Deep,14);}
            TLSUiFactory.MakeButton(ctl,"BUILD TRACE",V(.016f,.428f),V(.168f,.711f),()=>{session.Build();redraw();},TLSUiFactory.Violet,15);
            TLSUiFactory.MakeButton(ctl,"< STEP",V(.182f,.428f),V(.310f,.711f),()=>{session.Previous();redraw();},TLSUiFactory.Deep,14,session.Trace!=null&&session.Step>0);
            TLSUiFactory.MakeButton(ctl,"STEP >",V(.325f,.428f),V(.452f,.711f),()=>{session.Next();redraw();},TLSUiFactory.Deep,14,session.Trace!=null&&session.Step<session.Trace.Frames.Count-1);
            TLSUiFactory.MakeButton(ctl,"FINISH",V(.468f,.428f),V(.59f,.711f),()=>{session.Finish();redraw();},TLSUiFactory.Deep,14,session.Trace!=null);
            TLSUiFactory.MakeButton(ctl,"RESET",V(.606f,.428f),V(.738f,.711f),()=>{session.Reset();redraw();},TLSUiFactory.Deep,14);
            L(ctl,"Prediction label","Before finishing, predict comparisons (0–5000):",.027f,.04f,.449f,.31f,14,TLSUiFactory.Muted);
            TLSUiFactory.Field(ctl,"Comparison prediction",prediction(),"count",V(.454f,.052f),V(.726f,.342f),setPrediction);
            TLSUiFactory.MakeButton(ctl,"PREDICT",V(.745f,.052f),V(.962f,.342f),()=>{int count;if(int.TryParse(prediction(),out count))session.Predict(count);redraw();},TLSUiFactory.Deep,14,
                session.Trace!=null&&session.Step==0&&!session.OutcomeSeen&&!session.PredictedComparisons.HasValue);
        }
        public static void Debugging(Transform root,TLSDebugSession session,Func<string> getAnswer,Action<string> setAnswer,Action redraw,Action analysis,Action quality,Action wasm)
        {
            P(root,"Phase6 debug background",0,0,1,.907f,TLSUiFactory.Navy);
            RectTransform tabs=P(root,"Phase6 debugging top menu",.017f,.817f,.983f,.891f,TLSUiFactory.Slate);
            TLSUiFactory.MakeButton(tabs,"BIG-O / OP COUNTS",V(.012f,.1f),V(.257f,.9f),analysis,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"DEBUGGING & TESTS",V(.268f,.1f),V(.510f,.9f),redraw,TLSUiFactory.Violet,15);
            TLSUiFactory.MakeButton(tabs,"C CODE QUALITY",V(.524f,.1f),V(.754f,.9f),quality,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"WRITE C11",V(.764f,.1f),V(.986f,.9f),wasm,TLSUiFactory.Deep,15);
            RectTransform picks=P(root,"Phase6 test bug selector",.017f,.735f,.983f,.807f,TLSUiFactory.Slate);
            for(int i=0;i<5;i++)
            {TLSDebugFault fault=(TLSDebugFault)i;float x=.008f+i*.199f;
                TLSUiFactory.MakeButton(picks,TLSDebugCatalog.Name(fault),V(x,.1f),V(x+.19f,.9f),()=>{session.Select(fault);redraw();},
                    session.Selected==fault?TLSUiFactory.Violet:TLSUiFactory.Deep,11);}
            RectTransform details=P(root,"Seeded C bug",.017f,.288f,.49f,.721f,TLSUiFactory.Slate);
            L(details,"Fault label","SEEDED C FAULT — "+session.Scenario.Title,.036f,.883f,.968f,.979f,20,TLSUiFactory.Cyan);
            L(details,"Broken code",session.Scenario.BrokenC,.038f,.653f,.968f,.868f,17,TLSUiFactory.Amber);
            L(details,"Test principle",session.Scenario.TestPrinciple,.038f,.387f,.965f,.637f,17,TLSUiFactory.Pale);
            L(details,"Run feedback",session.Feedback,.037f,.17f,.968f,.38f,16,TLSUiFactory.Pale);
            L(details,"Fix after reveal",session.Revealed?session.Scenario.CorrectC:
                "Pick the cause A–E after RUN CASES. A revealed fix cannot earn an unseen first pass.",.037f,.016f,.966f,.165f,14,TLSUiFactory.Muted);
            RectTransform results=P(root,"Phase6 real boundary cases",.503f,.288f,.983f,.721f,TLSUiFactory.Deep);
            L(results,"Results header","BOUNDARY TESTS: REFERENCE VS SEEDED BUG",.025f,.888f,.972f,.982f,18,TLSUiFactory.Cyan);
            for(int i=0;i<session.Scenario.Cases.Count;i++)
            {
                TLSCaseComparison c=session.Scenario.Cases[i];float top=.859f-i*.094f;
                string status=session.Ran?(c.Pass?"PASS":"FAIL"):"NOT RUN";
                RectTransform row=P(results,"Boundary test row "+i,.018f,top-.091f,.981f,top,i%2==0?TLSUiFactory.Slate:TLSUiFactory.Deep);
                L(row,"Test label "+i,(i+1)+". "+c.Label+"  "+status,.023f,.49f,.973f,.973f,13,c.Pass&&session.Ran?TLSUiFactory.Green:session.Ran?TLSUiFactory.Amber:TLSUiFactory.Muted);
                L(row,"Actual vs expected "+i,session.Ran?"Expected: "+c.Expected+"    Seeded: "+c.Actual:"Expected and actual hidden until RUN CASES",.023f,.026f,.979f,.49f,12,TLSUiFactory.Pale);
            }
            RectTransform choices=P(root,"Phase6 fault diagnosis key",.017f,.171f,.983f,.273f,TLSUiFactory.Slate);
            L(choices,"Cause heading","SELECT ROOT CAUSE:  A full scan   B binary bound   C shift index 0   D modulo wrap   E tombstones",.02f,.60f,.98f,.975f,16,TLSUiFactory.Cyan);
            for(int i=0;i<5;i++)
            {string letter=((char)('A'+i)).ToString();float x=.012f+i*.197f;
                TLSUiFactory.MakeButton(choices,"CAUSE "+letter,V(x,.08f),V(x+.183f,.55f),()=>{setAnswer(letter);session.Submit(letter);redraw();},TLSUiFactory.Deep,14,session.Ran);}
            RectTransform ctl=P(root,"Phase6 debug controls",.017f,.015f,.983f,.160f,TLSUiFactory.Slate);
            TLSUiFactory.MakeButton(ctl,"RUN 8 CASES",V(.017f,.49f),V(.203f,.92f),()=>{session.Run();redraw();},TLSUiFactory.Violet,15);
            TLSUiFactory.MakeButton(ctl,session.Guided?"GUIDED":"INDEPENDENT",V(.217f,.49f),V(.393f,.92f),()=>{session.SetGuided(!session.Guided);redraw();},TLSUiFactory.Deep,14);
            TLSUiFactory.MakeButton(ctl,"HINT",V(.408f,.49f),V(.588f,.92f),()=>{session.Hint();redraw();},TLSUiFactory.Deep,14,session.Guided);
            TLSUiFactory.MakeButton(ctl,"REVEAL FIX",V(.603f,.49f),V(.785f,.92f),()=>{session.Reveal();redraw();},TLSUiFactory.Deep,14);
            L(ctl,"Session result","Independent first diagnoses: "+session.Passed+"/5  |  Each case is a deterministic modeled example, not arbitrary compiled C.",.026f,.05f,.977f,.426f,15,TLSUiFactory.Muted);
        }
        public static void Quality(Transform root,TLSQualitySession session,Action redraw,Action analysis,Action debug,Action wasm)
        {
            P(root,"Phase6 quality background",0,0,1,.907f,TLSUiFactory.Navy);
            RectTransform tabs=P(root,"Phase6 quality top menu",.017f,.817f,.983f,.891f,TLSUiFactory.Slate);
            TLSUiFactory.MakeButton(tabs,"BIG-O / OP COUNTS",V(.012f,.1f),V(.257f,.9f),analysis,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"DEBUGGING & TESTS",V(.268f,.1f),V(.510f,.9f),debug,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"C CODE QUALITY",V(.524f,.1f),V(.754f,.9f),redraw,TLSUiFactory.Violet,15);
            TLSUiFactory.MakeButton(tabs,"WRITE C11",V(.764f,.1f),V(.986f,.9f),wasm,TLSUiFactory.Deep,15);
            TLSQualityQuestion q=session.Current;
            RectTransform header=P(root,"Phase6 quality objective",.044f,.688f,.955f,.799f,TLSUiFactory.Slate);
            L(header,"Question counter","CODE REVIEW  "+(session.Index+1)+" / "+TLSQualityCatalog.Questions.Length+"  |  "+q.Topic,.035f,.515f,.97f,.96f,19,TLSUiFactory.Cyan);
            L(header,"Question prompt",q.Goal,.035f,.02f,.968f,.522f,18,TLSUiFactory.Pale);
            RectTransform a=P(root,"C candidate A",.045f,.352f,.491f,.669f,TLSUiFactory.Slate);
            RectTransform b=P(root,"C candidate B",.510f,.352f,.955f,.669f,TLSUiFactory.Slate);
            L(a,"A title","OPTION A",.045f,.806f,.96f,.978f,22,TLSUiFactory.Violet);
            L(a,"A code",q.OptionA,.046f,.197f,.96f,.806f,18,TLSUiFactory.Pale);
            L(b,"B title","OPTION B",.045f,.806f,.96f,.978f,22,TLSUiFactory.Violet);
            L(b,"B code",q.OptionB,.046f,.197f,.96f,.806f,18,TLSUiFactory.Pale);
            TLSUiFactory.MakeButton(a,"CHOOSE A",V(.045f,.034f),V(.965f,.192f),()=>{session.Choose("A");redraw();},TLSUiFactory.Deep,16);
            TLSUiFactory.MakeButton(b,"CHOOSE B",V(.045f,.034f),V(.965f,.192f),()=>{session.Choose("B");redraw();},TLSUiFactory.Deep,16);
            RectTransform feedback=P(root,"Phase6 quality feedback",.045f,.179f,.955f,.333f,TLSUiFactory.Deep);
            L(feedback,"Quality feedback",session.Feedback,.033f,.24f,.96f,.964f,17,TLSUiFactory.Pale);
            L(feedback,"Help note",session.ReferenceVisible?"Answer exposed: "+q.Correct:session.HintVisible?"HINT: "+q.Hint:
                "Guided review and reference exposure disqualify unseen first-pass credit. Readable code must also be correct C.",.033f,.019f,.96f,.27f,14,TLSUiFactory.Amber);
            RectTransform controls=P(root,"Phase6 quality controls",.045f,.019f,.955f,.164f,TLSUiFactory.Slate);
            TLSUiFactory.MakeButton(controls,session.Guided?"GUIDED":"INDEPENDENT",V(.02f,.48f),V(.217f,.89f),()=>{session.SetGuided(!session.Guided);redraw();},TLSUiFactory.Deep,14);
            TLSUiFactory.MakeButton(controls,"HINT",V(.233f,.48f),V(.386f,.89f),()=>{session.Hint();redraw();},TLSUiFactory.Deep,14,session.Guided);
            TLSUiFactory.MakeButton(controls,"REVEAL ANSWER",V(.40f,.48f),V(.602f,.89f),()=>{session.Reveal();redraw();},TLSUiFactory.Deep,14);
            TLSUiFactory.MakeButton(controls,"PREVIOUS",V(.625f,.48f),V(.789f,.89f),()=>{session.Select(session.Index-1);redraw();},TLSUiFactory.Deep,14,session.Index>0);
            TLSUiFactory.MakeButton(controls,"NEXT",V(.802f,.48f),V(.986f,.89f),()=>{session.Select(session.Index+1);redraw();},TLSUiFactory.Deep,14,session.Index+1<TLSQualityCatalog.Questions.Length);
            L(controls,"Result","Independent unassisted first-pass reviews: "+session.Independent+"/"+TLSQualityCatalog.Questions.Length+". Session-only; not proof of coding mastery.",.022f,.025f,.972f,.425f,14,TLSUiFactory.Muted);
        }
    }
}
