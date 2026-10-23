using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    public static class TLSLinkedListScreen
    {
        private static Vector2 V(float x,float y){return new Vector2(x,y);}
        private static void Op(Transform bar,string label,float left,float right,Action click,int font=15)
        {TLSUiFactory.MakeButton(bar,label,V(left,0.08f),V(right,0.92f),click,TLSUiFactory.Deep,font);}
        public static void Render(Transform root,TLSLinkedLabSession lab,Action redraw,Action openPractice,Action openFunctions,
            Func<string> getValue,Func<string> getIndex,Action<string> setValue,Action<string> setIndex)
        {
            TLSUiFactory.Panel(root,"Linked background",V(0,0),V(1,0.908f),TLSUiFactory.Navy);
            RectTransform kinds=TLSUiFactory.Panel(root,"Linked kinds and navigation",V(0.019f,0.833f),V(0.982f,0.899f),TLSUiFactory.Slate);
            for(int i=0;i<3;i++){
                TLSListKind kind=(TLSListKind)i;float x=0.009f+0.155f*i;
                TLSUiFactory.MakeButton(kinds,kind.ToString().ToUpperInvariant(),V(x,0.13f),V(x+0.146f,0.9f),
                    ()=>{lab.SelectKind(kind);redraw();},kind==lab.Current.Kind?TLSUiFactory.Violet:TLSUiFactory.Deep,16);
            }
            TLSUiFactory.MakeButton(kinds,"C PRACTICE",V(0.514f,0.13f),V(0.741f,0.9f),openPractice,TLSUiFactory.Deep,16);
            TLSUiFactory.MakeButton(kinds,"WRITE C FUNCTIONS",V(0.751f,0.13f),V(0.99f,0.9f),openFunctions,TLSUiFactory.Deep,16);
            RectTransform operations=TLSUiFactory.Panel(root,"Linked operations",V(0.019f,0.696f),V(0.982f,0.822f),TLSUiFactory.Slate);
            TLSListAction[] acts={TLSListAction.Prepend,TLSListAction.Append,TLSListAction.InsertAt,TLSListAction.RemoveAt,
                TLSListAction.Search,TLSListAction.Reverse,TLSListAction.RecursiveCount,TLSListAction.RecursiveFind,TLSListAction.Clear};
            for(int i=0;i<acts.Length;i++)
            {
                TLSListAction selected=acts[i];int row=i/5,column=i%5;
                float x0=0.01f+column*0.198f, y0=row==0?0.525f:0.07f,y1=row==0?0.945f:0.49f;
                TLSUiFactory.MakeButton(operations,selected.ToString().ToUpperInvariant(),V(x0,y0),V(x0+0.187f,y1),()=>{
                    int val=0,idx=0;
                    if(!int.TryParse(getValue(),out val))val=0;
                    if(!int.TryParse(getIndex(),out idx))idx=0;
                    lab.Start(selected,val,idx);redraw();
                },TLSUiFactory.Deep,14);
            }
            RectTransform code=TLSUiFactory.Panel(root,"Linked C trace",V(0.019f,0.343f),V(0.484f,0.683f),TLSUiFactory.Slate);
            TLSUiFactory.Label(code,"C heading","C11 / STEPWISE POINTER OPERATIONS",V(0.035f,0.893f),V(0.965f,0.985f),19,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(code,"Scope","Simplified modeled trace. Singly C11 recursive code is not valid for circular lists without a boundary.",
                V(0.037f,0.803f),V(0.965f,0.891f),13,TLSUiFactory.Amber);
            TLSListTrace trace=lab.Trace;
            string[] lines=trace==null?new[]{"Node *head = NULL;","/* Select an operation and inspect each step. */"}:trace.Code;
            TLSListFrame frame=trace==null?null:lab.Frame;
            for(int i=0;i<lines.Length;i++)
            {
                float top=0.79f-i*0.095f;
                RectTransform bar=TLSUiFactory.Panel(code,"C line "+i,V(0.027f,top-0.092f),V(0.976f,top),
                    frame!=null&&frame.CodeLine==i?new Color(0.15f,0.36f,0.44f,1f):TLSUiFactory.Slate);
                TLSUiFactory.Label(bar,"Statement",(i+1).ToString("00")+" "+lines[i],V(0.021f,0.05f),V(0.99f,0.96f),
                    14,frame!=null&&frame.CodeLine==i?TLSUiFactory.Pale:TLSUiFactory.Muted,TextAnchor.MiddleLeft,frame!=null&&frame.CodeLine==i);
            }
            RectTransform graph=TLSUiFactory.Panel(root,"Linked memory visualization",V(0.494f,0.343f),V(0.982f,0.683f),TLSUiFactory.Deep);
            TLSListState state=frame==null?lab.Current:frame.State;
            TLSUiFactory.Label(graph,"Graph heading",state.Kind.ToString().ToUpperInvariant()+" / SYMBOLIC HEAP",
                V(0.035f,0.896f),V(0.97f,0.99f),19,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            int[] reachable;
            try{reachable=state.Walk();}catch{reachable=new int[0];}
            TLSUiFactory.Label(graph,"Graph summary","HEAD "+TLSListState.Address(state.Head)+"   TAIL "+TLSListState.Address(state.Tail)+
                "   REACHABLE "+reachable.Length+" / 8",V(0.035f,0.798f),V(0.97f,0.888f),15,TLSUiFactory.Pale);
            string chain = reachable.Length == 0 ? "HEAD -> NULL (empty)" : "HEAD";
            for(int i=0;i<reachable.Length;i++) chain += " -> " + state.Find(reachable[i]).Value;
            if(reachable.Length>0) chain += state.Kind==TLSListKind.Circular ? " -> HEAD (ring)" : " -> NULL";
            TLSUiFactory.Label(graph,"Visual order path",chain,V(0.035f,0.718f),V(0.97f,0.795f),14,TLSUiFactory.Green);
            int cells=Math.Min(state.Heap.Count,8);
            for(int i=0;i<cells;i++)
            {
                TLSListNode n=state.Heap[i];int row=i/4,col=i%4;
                float x0=0.031f+col*0.243f,x1=x0+0.229f;
                float y0=row==0?0.400f:0.105f,y1=row==0?0.705f:0.380f;
                RectTransform tile=TLSUiFactory.Panel(graph,"Node_"+n.Id,V(x0,y0),V(x1,y1),
                    n.Freed?new Color(0.31f,0.18f,0.18f,1f):frame!=null&&frame.FocusId==n.Id?
                    new Color(0.13f,0.37f,0.46f,1f):TLSUiFactory.Slate);
                string label=TLSListState.Address(n.Id)+"  "+(n.Freed?"FREED":"LIVE")+"\n"+
                    (n.Freed?"NO ACCESS":"data="+n.Value)+"\n"+
                    "next: "+TLSListState.Address(n.Next)+(state.Kind==TLSListKind.Doubly?"\nprev: "+TLSListState.Address(n.Prev):"");
                TLSUiFactory.Label(tile,"Fields",label,V(0.042f,0.045f),V(0.985f,0.969f),
                    state.Kind==TLSListKind.Doubly?12:13,n.Freed?TLSUiFactory.Amber:TLSUiFactory.Pale);
            }
            TLSUiFactory.Label(graph,"Graph legend","Labels show exact modeled links. FREED nodes stay visible for learning, but are not dereferenced.",
                V(0.034f,0.005f),V(0.975f,0.092f),12,TLSUiFactory.Amber);
            RectTransform explanation=TLSUiFactory.Panel(root,"Linked explanation",V(0.019f,0.217f),V(0.982f,0.328f),TLSUiFactory.Slate);
            TLSUiFactory.Label(explanation,"What changed",frame==null?"Choose an operation above to start the trace.":frame.Explanation,
                V(0.021f,0.44f),V(0.975f,0.975f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(explanation,"Call stack",frame==null?"" : frame.CallStack,
                V(0.021f,0.03f),V(0.975f,0.43f),14,TLSUiFactory.Violet);
            RectTransform controls=TLSUiFactory.Panel(root,"Linked control ribbon",V(0.019f,0.015f),V(0.982f,0.204f),TLSUiFactory.Slate);
            TLSUiFactory.Label(controls,"Trace status",trace==null?"NO TRACE YET":
                trace.Diagnostic+"   |   STEP "+lab.Step+" / "+(trace.Frames.Count-1),V(0.016f,0.785f),V(0.986f,0.983f),16,TLSUiFactory.Cyan);
            TLSUiFactory.Label(controls,"Input labels","VALUE (target for SEARCH / RECURSIVE FIND)                                    INDEX (INSERT / REMOVE)",
                V(0.018f,0.578f),V(0.57f,0.787f),13,TLSUiFactory.Muted);
            TLSUiFactory.Field(controls,"Linked value input",getValue(),"integer",V(0.017f,0.31f),V(0.201f,0.582f),setValue);
            TLSUiFactory.Field(controls,"Linked index input",getIndex(),"index",V(0.215f,0.31f),V(0.391f,0.582f),setIndex);
            TLSUiFactory.MakeButton(controls,"< PREVIOUS",V(0.402f,0.315f),V(0.554f,0.589f),()=>{lab.Previous();redraw();},TLSUiFactory.Deep,14,trace!=null&&lab.Step>0);
            TLSUiFactory.MakeButton(controls,"NEXT >",V(0.565f,0.315f),V(0.698f,0.589f),()=>{lab.Next();redraw();},TLSUiFactory.Deep,14,trace!=null&&lab.Step<trace.Frames.Count-1);
            TLSUiFactory.MakeButton(controls,"FINISH",V(0.709f,0.315f),V(0.834f,0.589f),()=>{lab.Finish();redraw();},TLSUiFactory.Deep,14,trace!=null&&lab.Step<trace.Frames.Count-1);
            TLSUiFactory.MakeButton(controls,"RESET TRACE",V(0.845f,0.315f),V(0.984f,0.589f),()=>{lab.ResetTrace();redraw();},TLSUiFactory.Deep,13);
            TLSUiFactory.Label(controls,"Prediction prompt","BEFORE STEPPING: Predict change in reachable node count.",
                V(0.017f,0.14f),V(0.47f,0.3f),13,TLSUiFactory.Pale);
            bool pred=trace!=null&&lab.Step==0&&!lab.PredictionUsed;
            TLSUiFactory.MakeButton(controls,"MORE",V(0.475f,0.04f),V(0.583f,0.286f),()=>{lab.Predict(1);redraw();},TLSUiFactory.Deep,12,pred);
            TLSUiFactory.MakeButton(controls,"FEWER",V(0.592f,0.04f),V(0.699f,0.286f),()=>{lab.Predict(-1);redraw();},TLSUiFactory.Deep,12,pred);
            TLSUiFactory.MakeButton(controls,"SAME",V(0.709f,0.04f),V(0.813f,0.286f),()=>{lab.Predict(0);redraw();},TLSUiFactory.Deep,12,pred);
            TLSUiFactory.Label(controls,"Prediction result",lab.PredictionReview,V(0.017f,0.005f),V(0.473f,0.131f),12,TLSUiFactory.Amber);
            TLSUiFactory.Label(controls,"Learning disclaimer","Formative only / no Academy mastery saved",V(0.815f,0.016f),V(0.982f,0.29f),12,TLSUiFactory.Muted);
        }
    }
}
