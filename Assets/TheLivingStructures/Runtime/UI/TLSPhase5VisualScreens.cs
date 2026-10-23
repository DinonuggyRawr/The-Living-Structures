using System;
using System.Collections.Generic;
using System.Linq;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>Accessible, non-clickable connector artwork for actual modeled node links.</summary>
    public sealed class TLSPhase5EdgeGraphic : MaskableGraphic
    {
        private readonly List<Vector2> points = new List<Vector2>();
        private readonly List<Vector2Int> links = new List<Vector2Int>();
        private bool arrows;
        public void Configure(List<Vector2> vertices, List<Vector2Int> edges, bool directed)
        { points.Clear();points.AddRange(vertices);links.Clear();links.AddRange(edges);arrows=directed;raycastTarget=false;SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Rect r=rectTransform.rect;
            for(int i=0;i<links.Count;i++)
            {
                Vector2Int e=links[i];if(e.x<0||e.y<0||e.x>=points.Count||e.y>=points.Count)continue;
                Vector2 a=new Vector2(r.xMin+r.width*points[e.x].x,r.yMin+r.height*points[e.x].y);
                Vector2 b=new Vector2(r.xMin+r.width*points[e.y].x,r.yMin+r.height*points[e.y].y);
                Vector2 delta=b-a;if(delta.sqrMagnitude<1f)continue;
                Vector2 n=new Vector2(-delta.y,delta.x).normalized*1.7f;
                AddQuad(vh,a+n,a-n,b-n,b+n,color);
                if(arrows)
                {
                    Vector2 dir=delta.normalized;
                    Vector2 tip=b-dir*16f,basePt=tip-dir*10f;Vector2 wide=new Vector2(-dir.y,dir.x)*6f;
                    int v=vh.currentVertCount;
                    vh.AddVert(tip,color,Vector2.zero);vh.AddVert(basePt+wide,color,Vector2.zero);vh.AddVert(basePt-wide,color,Vector2.zero);
                    vh.AddTriangle(v,v+1,v+2);
                }
            }
        }
        private static void AddQuad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color tint)
        {
            UIVertex x=UIVertex.simpleVert;x.color=tint;
            x.position=a;UIVertex y=x;y.position=b;UIVertex z=x;z.position=c;UIVertex w=x;w.position=d;
            vh.AddUIVertexQuad(new[]{x,y,z,w});
        }
    }
    public static class TLSPhase5VisualScreens
    {
        private static Vector2 V(float x,float y){return new Vector2(x,y);}
        private static RectTransform Panel(Transform root,string name,Vector2 a,Vector2 b,Color c)
        {return TLSUiFactory.Panel(root,name,a,b,c);}
        private static void Code(Transform root,string title,string[] source,int focused,string note)
        {
            RectTransform code=Panel(root,"Phase5 C code",V(.018f,.343f),V(.444f,.693f),TLSUiFactory.Slate);
            TLSUiFactory.Label(code,"Title",title,V(.03f,.89f),V(.97f,.99f),19,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            for(int i=0;i<Math.Min(source.Length,7);i++)
            {
                float top=.864f-i*.116f;
                RectTransform line=Panel(code,"Code line "+i,V(.023f,top-.109f),V(.979f,top),focused==i?new Color(.12f,.33f,.42f,1):TLSUiFactory.Deep);
                TLSUiFactory.Label(line,"C",(i+1).ToString("00")+"  "+source[i],V(.024f,.03f),V(.977f,.963f),14,
                    focused==i?TLSUiFactory.Pale:TLSUiFactory.Muted,TextAnchor.MiddleLeft);
            }
            TLSUiFactory.Label(code,"Scope",note,V(.029f,.003f),V(.965f,.090f),12,TLSUiFactory.Amber);
        }
        private static void Explanation(Transform root,string why,string diagnostic,string prediction,int step,int last)
        {
            RectTransform p=Panel(root,"Phase5 explanation",V(.019f,.222f),V(.981f,.329f),TLSUiFactory.Slate);
            TLSUiFactory.Label(p,"Cause",why,V(.019f,.45f),V(.98f,.98f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(p,"Summary",diagnostic+"   |   STEP "+step+"/"+last,V(.019f,.14f),V(.98f,.47f),13,TLSUiFactory.Cyan);
            TLSUiFactory.Label(p,"Prediction",prediction,V(.019f,.002f),V(.98f,.166f),12,TLSUiFactory.Amber);
        }
        private static void PredictionButtons<T>(Transform parent,TLSPhase5Session<T> session,Action redraw)
        {
            bool enable=session.Trace!=null&&session.Step==0&&!session.Predicted;
            TLSUiFactory.MakeButton(parent,"MORE",V(.675f,.012f),V(.765f,.24f),()=>{session.Predict(1);redraw();},TLSUiFactory.Deep,12,enable);
            TLSUiFactory.MakeButton(parent,"FEWER",V(.777f,.012f),V(.868f,.24f),()=>{session.Predict(-1);redraw();},TLSUiFactory.Deep,12,enable);
            TLSUiFactory.MakeButton(parent,"SAME",V(.88f,.012f),V(.984f,.24f),()=>{session.Predict(0);redraw();},TLSUiFactory.Deep,12,enable);
        }
        private static void Timeline<T>(Transform ctrl,TLSPhase5Session<T> session,Action redraw)
        {
            bool trace=session.Trace!=null;
            TLSUiFactory.MakeButton(ctrl,"< STEP",V(.36f,.52f),V(.505f,.80f),()=>{session.Previous();redraw();},TLSUiFactory.Deep,14,trace&&session.Step>0);
            TLSUiFactory.MakeButton(ctrl,"STEP >",V(.513f,.52f),V(.651f,.80f),()=>{session.Next();redraw();},TLSUiFactory.Deep,14,trace&&session.Step<session.Trace.Frames.Count-1);
            TLSUiFactory.MakeButton(ctrl,"FINISH",V(.665f,.52f),V(.81f,.80f),()=>{session.Finish();redraw();},TLSUiFactory.Deep,14,trace&&session.Step<session.Trace.Frames.Count-1);
            TLSUiFactory.MakeButton(ctrl,"RESET TRACE",V(.824f,.52f),V(.98f,.80f),()=>{session.ResetTrace();redraw();},TLSUiFactory.Deep,13,trace);
            PredictionButtons(ctrl,session,redraw);
        }
        private static void SetEdges(Transform p,List<Vector2> xy,List<Vector2Int> links,bool arrows)
        {
            GameObject g=new GameObject("Phase5 modeled pointer edges",typeof(RectTransform),typeof(CanvasRenderer),typeof(TLSPhase5EdgeGraphic));
            RectTransform rt=(RectTransform)g.transform;rt.SetParent(p,false);rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
            TLSPhase5EdgeGraphic graphic=g.GetComponent<TLSPhase5EdgeGraphic>();graphic.color=new Color(.32f,.76f,.88f,.9f);graphic.Configure(xy,links,arrows);
        }
        public static void Heap(Transform root,TLSHeapSession session,Func<string> readValue,Action<string> setValue,Action redraw,Action practice,Action functions)
        {
            TLSUiFactory.Panel(root,"Phase5 heap background",V(0,0),V(1,.907f),TLSUiFactory.Navy);
            RectTransform tabs=Panel(root,"Heap tabs",V(.019f,.817f),V(.981f,.893f),TLSUiFactory.Slate);
            TLSUiFactory.MakeButton(tabs,"MIN HEAP",V(.014f,.10f),V(.245f,.89f),()=>{session.Select(TLSHeapKind.Min);redraw();},session.Current.Kind==TLSHeapKind.Min?TLSUiFactory.Violet:TLSUiFactory.Deep,16);
            TLSUiFactory.MakeButton(tabs,"MAX HEAP",V(.256f,.10f),V(.485f,.89f),()=>{session.Select(TLSHeapKind.Max);redraw();},session.Current.Kind==TLSHeapKind.Max?TLSUiFactory.Violet:TLSUiFactory.Deep,16);
            TLSUiFactory.MakeButton(tabs,"C PRACTICE",V(.509f,.10f),V(.732f,.89f),practice,TLSUiFactory.Deep,16);
            TLSUiFactory.MakeButton(tabs,"WRITE C HEAP",V(.745f,.10f),V(.987f,.89f),functions,TLSUiFactory.Deep,15);
            RectTransform bar=Panel(root,"Heap operations",V(.019f,.718f),V(.981f,.809f),TLSUiFactory.Slate);
            TLSHeapAction[] acts={TLSHeapAction.Insert,TLSHeapAction.Extract,TLSHeapAction.BuildExample,TLSHeapAction.Reset};
            for(int i=0;i<acts.Length;i++)
            {TLSHeapAction a=acts[i];float x=.012f+i*.246f;
                TLSUiFactory.MakeButton(bar,a==TLSHeapAction.BuildExample?"BUILD / HEAPIFY":a.ToString().ToUpperInvariant(),V(x,.12f),V(x+.23f,.88f),()=>{
                    int n=0;if(a==TLSHeapAction.Insert&&!int.TryParse(readValue(),out n))session.Reject("Enter a valid integer value.");
                    else session.Begin(a,n);redraw();},TLSUiFactory.Deep,16);
            }
            TLSPhase5Trace<TLSHeapState> trace=session.Trace;TLSPhase5Frame<TLSHeapState> f=session.Frame;TLSHeapState s=session.Visible;
            Code(root,"HEAP / C11 ALGORITHM STEPS",trace==null?new[]{"// Choose an operation above","// parent = (i - 1) / 2","// children = 2*i+1, 2*i+2"}:trace.Code,f==null?-1:f.Line,
                "Array-backed heap; these are modeled C algorithm sketches, not student code execution.");
            RectTransform graph=Panel(root,"Heap tree graph",V(.454f,.343f),V(.981f,.693f),TLSUiFactory.Deep);
            TLSUiFactory.Label(graph,"Title",s.Kind.ToString().ToUpperInvariant()+" HEAP  |  "+s.Data.Count+" / "+TLSHeapState.Capacity+" entries",
                V(.03f,.89f),V(.97f,.98f),17,TLSUiFactory.Cyan);
            List<Vector2> xy=new List<Vector2>();List<Vector2Int> edges=new List<Vector2Int>();
            for(int i=0;i<s.Data.Count;i++)
            {int level=0;while((1<<(level+1))-1<=i)level++;int first=(1<<level)-1;int offset=i-first;
                xy.Add(V((offset+1f)/((1<<level)+1f),.78f-level*.215f));if(i>0)edges.Add(new Vector2Int((i-1)/2,i));}
            SetEdges(graph,xy,edges,false);
            for(int i=0;i<xy.Count;i++)
            {
                float x=xy[i].x,y=xy[i].y;
                RectTransform tile=Panel(graph,"Heap node "+i,V(x-.043f,y-.055f),V(x+.043f,y+.055f),f!=null&&f.Focus==i?new Color(.24f,.44f,.48f,1):TLSUiFactory.Slate);
                TLSUiFactory.Label(tile,"Value",s.Data[i].ToString(),V(.02f,.12f),V(.98f,.94f),16,TLSUiFactory.Pale,TextAnchor.MiddleCenter,true);
            }
            TLSUiFactory.Label(graph,"Array",s.Data.Count==0?"ARRAY: (empty)":"ARRAY: ["+string.Join(", ",s.Data.Select(n=>n.ToString()).ToArray())+"]",
                V(.03f,.02f),V(.98f,.10f),14,TLSUiFactory.Amber);
            Explanation(root,f==null?"A heap maintains parent priority, not global sorted order.":f.Why,session.Diagnostic,session.Prediction,
                session.Step,trace==null?0:trace.Frames.Count-1);
            RectTransform ctrl=Panel(root,"Heap controls",V(.019f,.016f),V(.981f,.209f),TLSUiFactory.Slate);
            TLSUiFactory.Label(ctrl,"Value label","INSERT VALUE",V(.02f,.82f),V(.345f,.96f),14,TLSUiFactory.Muted);
            TLSUiFactory.Field(ctrl,"Heap input value",readValue(),"integer",V(.02f,.52f),V(.342f,.80f),setValue);
            Timeline(ctrl,session,redraw);
            TLSUiFactory.Label(ctrl,"Hint","Watch sift-up and sift-down swaps. Predict size before revealing the operation.",V(.025f,.009f),V(.66f,.25f),14,TLSUiFactory.Muted);
        }
        public static void Graph(Transform root,TLSGraphSession session,Func<string> readA,Func<string> readB,
            Action<string> setA,Action<string> setB,Action redraw,Action practice,Action functions)
        {
            TLSUiFactory.Panel(root,"Phase5 graph background",V(0,0),V(1,.907f),TLSUiFactory.Navy);
            RectTransform tabs=Panel(root,"Graph tabs",V(.019f,.817f),V(.981f,.893f),TLSUiFactory.Slate);
            TLSUiFactory.MakeButton(tabs,"UNDIRECTED",V(.009f,.12f),V(.236f,.89f),()=>{session.SetDirected(false);redraw();},!session.Current.Directed?TLSUiFactory.Violet:TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"DIRECTED",V(.244f,.12f),V(.459f,.89f),()=>{session.SetDirected(true);redraw();},session.Current.Directed?TLSUiFactory.Violet:TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"C PRACTICE",V(.484f,.12f),V(.728f,.89f),practice,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"WRITE C GRAPH",V(.748f,.12f),V(.99f,.89f),functions,TLSUiFactory.Deep,15);
            RectTransform bar=Panel(root,"Graph operations",V(.019f,.718f),V(.981f,.809f),TLSUiFactory.Slate);
            TLSGraphAction[] actions={TLSGraphAction.AddVertex,TLSGraphAction.AddEdge,TLSGraphAction.RemoveEdge,TLSGraphAction.BFS,TLSGraphAction.DFS,TLSGraphAction.Example,TLSGraphAction.Clear};
            for(int i=0;i<actions.Length;i++){TLSGraphAction a=actions[i];float x=.007f+i*.141f;
                TLSUiFactory.MakeButton(bar,a==TLSGraphAction.Example?"LOAD SAMPLE":a.ToString().ToUpperInvariant(),V(x,.12f),V(x+.133f,.88f),
                    ()=>{session.Begin(a,readA(),readB());redraw();},TLSUiFactory.Deep,12);}
            TLSPhase5Trace<TLSGraphState> trace=session.Trace;TLSPhase5Frame<TLSGraphState> f=session.Frame;TLSGraphState s=session.Visible;
            Code(root,"GRAPH / C11 ALGORITHM STEPS",trace==null?new[]{"// Adjacency matrix: adj[u][v]","// BFS: FIFO queue of vertices","// DFS: LIFO stack of vertices"}:trace.Code,
                f==null?-1:f.Line,"Node links are real modeled adjacency cells. Deterministic alphabetical traversal; no network access.");
            RectTransform graph=Panel(root,"Graph spatial topology",V(.454f,.343f),V(.981f,.693f),TLSUiFactory.Deep);
            TLSUiFactory.Label(graph,"Title",(s.Directed?"DIRECTED":"UNDIRECTED")+"  |  VERTICES "+s.Vertices+"  |  EDGES "+s.EdgeCount,
                V(.026f,.89f),V(.982f,.98f),17,TLSUiFactory.Cyan);
            List<Vector2> coords=new List<Vector2>();List<Vector2Int> links=new List<Vector2Int>();
            for(int i=0;i<s.Vertices;i++)
            {float angle=(float)(2*Math.PI*i/Math.Max(1,s.Vertices)+Math.PI/2);coords.Add(V(.5f+.37f*(float)Math.Cos(angle),.53f+.28f*(float)Math.Sin(angle)));}
            for(int i=0;i<s.Vertices;i++)for(int j=0;j<s.Vertices;j++)if(s.Edges[i,j]&&(s.Directed||i<j))links.Add(new Vector2Int(i,j));
            SetEdges(graph,coords,links,s.Directed);
            for(int i=0;i<coords.Count;i++)
            {float x=coords[i].x,y=coords[i].y;
                RectTransform tile=Panel(graph,"Graph vertex "+i,V(x-.036f,y-.062f),V(x+.036f,y+.062f),f!=null&&f.Focus==i?new Color(.22f,.45f,.48f,1):TLSUiFactory.Slate);
                TLSUiFactory.Label(tile,"Letter",((char)('A'+i)).ToString(),V(.06f,.1f),V(.94f,.90f),17,TLSUiFactory.Pale,TextAnchor.MiddleCenter,true);
            }
            List<string> adjacent=new List<string>();
            for(int i=0;i<Math.Min(4,s.Vertices);i++)
            {
                string neighbors="";
                for(int j=0;j<s.Vertices;j++)if(s.Edges[i,j])neighbors+=(char)('A'+j);
                adjacent.Add(((char)('A'+i)).ToString()+":"+(neighbors.Length==0?"-":neighbors));
            }
            TLSUiFactory.Label(graph,"Adjacency list notation","ADJACENCY (from matrix): "+string.Join("  |  ",adjacent.ToArray())+
                (s.Vertices>4?"  |  ...":""),V(.025f,.101f),V(.975f,.171f),12,TLSUiFactory.Muted);
            TLSUiFactory.Label(graph,"Frontier",f==null?"TRAVERSAL FRONTIER: none":f.Frontier,
                V(.025f,.007f),V(.975f,.095f),13,TLSUiFactory.Amber);
            Explanation(root,f==null?"BFS explores by FIFO layers; DFS follows LIFO depth. The drawing is a representation, not a network.":f.Why,
                session.Diagnostic,session.Prediction,session.Step,trace==null?0:trace.Frames.Count-1);
            RectTransform ctrl=Panel(root,"Graph controls",V(.019f,.016f),V(.981f,.209f),TLSUiFactory.Slate);
            TLSUiFactory.Label(ctrl,"Labels","START / SOURCE                  DESTINATION (A-I)",V(.018f,.82f),V(.348f,.974f),14,TLSUiFactory.Muted);
            TLSUiFactory.Field(ctrl,"Graph input A",readA(),"A",V(.019f,.515f),V(.16f,.802f),setA);
            TLSUiFactory.Field(ctrl,"Graph input B",readB(),"B",V(.184f,.515f),V(.337f,.802f),setB);
            Timeline(ctrl,session,redraw);
            TLSUiFactory.Label(ctrl,"Hint","BFS/DFS visit reachable nodes in alphabetically ordered neighbor scans; results are session-only.",V(.024f,.01f),V(.66f,.24f),13,TLSUiFactory.Muted);
        }
        public static void Hash(Transform root,TLSHashSession session,Func<string> readKey,Action<string> setKey,Action redraw,Action practice,Action functions)
        {
            TLSUiFactory.Panel(root,"Phase5 hash background",V(0,0),V(1,.907f),TLSUiFactory.Navy);
            RectTransform tabs=Panel(root,"Hash tabs",V(.019f,.817f),V(.981f,.893f),TLSUiFactory.Slate);
            TLSUiFactory.MakeButton(tabs,"SEPARATE CHAINING",V(.012f,.11f),V(.278f,.89f),()=>{session.Select(TLSHashKind.Chaining);redraw();},session.Current.Kind==TLSHashKind.Chaining?TLSUiFactory.Violet:TLSUiFactory.Deep,14);
            TLSUiFactory.MakeButton(tabs,"LINEAR PROBING",V(.288f,.11f),V(.515f,.89f),()=>{session.Select(TLSHashKind.LinearProbing);redraw();},session.Current.Kind==TLSHashKind.LinearProbing?TLSUiFactory.Violet:TLSUiFactory.Deep,14);
            TLSUiFactory.MakeButton(tabs,"C PRACTICE",V(.533f,.11f),V(.75f,.89f),practice,TLSUiFactory.Deep,15);
            TLSUiFactory.MakeButton(tabs,"WRITE C HASH",V(.765f,.11f),V(.988f,.89f),functions,TLSUiFactory.Deep,15);
            RectTransform bar=Panel(root,"Hash operations",V(.019f,.718f),V(.981f,.809f),TLSUiFactory.Slate);
            TLSHashAction[] actions={TLSHashAction.Insert,TLSHashAction.Find,TLSHashAction.Remove,TLSHashAction.CollisionExample,TLSHashAction.Reset};
            for(int i=0;i<actions.Length;i++){TLSHashAction a=actions[i];float x=.012f+i*.197f;
                TLSUiFactory.MakeButton(bar,a==TLSHashAction.CollisionExample?"COLLISION EXAMPLE":a.ToString().ToUpperInvariant(),
                    V(x,.1f),V(x+.18f,.9f),()=>{int key=0;if(a!=TLSHashAction.Reset&&a!=TLSHashAction.CollisionExample&&!int.TryParse(readKey(),out key))session.Reject("Enter a valid integer key.");else session.Begin(a,key);redraw();},TLSUiFactory.Deep,14);
            }
            TLSPhase5Trace<TLSHashState> trace=session.Trace;TLSPhase5Frame<TLSHashState> f=session.Frame;TLSHashState s=session.Visible;
            Code(root,"HASH / C11 ALGORITHM STEPS",trace==null?new[]{"// Normalize negative modulo","// Chaining keeps colliding keys together","// Probing uses EMPTY / DELETED / FULL"}:trace.Code,f==null?-1:f.Line,
                "Fixed 7-slot teaching hash set. Linear probing needs TOMBSTONES after deletion.");
            RectTransform graph=Panel(root,"Hash bucket map",V(.454f,.343f),V(.981f,.693f),TLSUiFactory.Deep);
            TLSUiFactory.Label(graph,"Title",s.Kind.ToString().ToUpperInvariant()+"  |  "+s.Count+" KEYS  |  7 BUCKETS",
                V(.03f,.88f),V(.97f,.98f),17,TLSUiFactory.Cyan);
            for(int i=0;i<TLSHashState.Capacity;i++)
            {
                float top=.85f-i*.112f;
                RectTransform tile=Panel(graph,"Hash bucket "+i,V(.025f,top-.103f),V(.973f,top),f!=null&&f.Focus==i?new Color(.20f,.40f,.44f,1):TLSUiFactory.Slate);
                string value=s.Kind==TLSHashKind.Chaining?
                    (s.Chains[i].Count==0?"EMPTY":string.Join(" -> ",s.Chains[i].Select(n=>n.ToString()).ToArray())):
                    s.Kinds[i]==TLSHashSlotKind.Occupied?s.Values[i].ToString():s.Kinds[i]==TLSHashSlotKind.Deleted?"DELETED (tombstone)":"EMPTY";
                TLSUiFactory.Label(tile,"Bucket value","["+i+"]   "+value,V(.03f,.07f),V(.97f,.94f),15,TLSUiFactory.Pale);
            }
            Explanation(root,f==null?"Hash keys map to buckets. Chaining and probing handle collisions differently.":f.Why,
                session.Diagnostic,session.Prediction,session.Step,trace==null?0:trace.Frames.Count-1);
            RectTransform ctrl=Panel(root,"Hash controls",V(.019f,.016f),V(.981f,.209f),TLSUiFactory.Slate);
            TLSUiFactory.Label(ctrl,"Key label","INTEGER KEY",V(.02f,.825f),V(.345f,.975f),14,TLSUiFactory.Muted);
            TLSUiFactory.Field(ctrl,"Hash key",readKey(),"integer",V(.02f,.52f),V(.344f,.802f),setKey);
            Timeline(ctrl,session,redraw);
            TLSUiFactory.Label(ctrl,"Hint","For negative keys, normalize modulo. After deletion, a probe slot must stay DELETED.",V(.024f,.005f),V(.662f,.248f),14,TLSUiFactory.Muted);
        }
    }
}
