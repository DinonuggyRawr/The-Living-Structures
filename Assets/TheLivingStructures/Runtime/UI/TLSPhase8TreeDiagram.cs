using System;
using System.Collections.Generic;
using TLS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>
    /// Read-only, connected layout of the authoritative Phase 4 tree state.
    /// Never mutates TLSTreeState or produces an invented edge. Zoom and pan are
    /// presentation-only. General trees use ordered child links, not binary fields.
    /// </summary>
    public static class TLSPhase8TreeDiagram
    {
        public sealed class Position { public int Id, Depth; public float X, Y; }
        public sealed class Edge { public int From, To; public Edge(int a,int b){From=a;To=b;} }
        public sealed class Layout
        {
            public readonly List<Position> Nodes = new List<Position>();
            public readonly List<Edge> Edges = new List<Edge>();
            public int LeafCount, Levels;
            public bool Valid = true;
            public Position Find(int id) { foreach (Position p in Nodes) if(p.Id==id)return p;return null; }
        }
        private static float zoom=1f, panX, panY;
        private static int selected=-1;
        private static Vector2 V(float x,float y){return new Vector2(x,y);}
        public static Layout Compute(TLSTreeState state)
        {
            if (state == null) throw new ArgumentNullException("state");
            if (!state.Invariants()) return new Layout { Valid = false };
            Layout result=new Layout { Levels=state.Height };
            if(state.Root<0)return result;
            int leaves=0;
            Add(state,state.Root,0,result,ref leaves);
            result.LeafCount=leaves;
            foreach (Position p in result.Nodes)
            {
                p.X=(p.X+.5f)/Mathf.Max(1,leaves);
                p.Y=result.Levels<=1?.52f:.82f-p.Depth*(.68f/(result.Levels-1));
            }
            return result;
        }
        private static float Add(TLSTreeState state,int id,int depth,Layout result,ref int leaves)
        {
            TLSTreeNode node=state.Find(id);
            List<int> children=new List<int>();
            if(state.Kind==TLSTreeKind.General)children.AddRange(node.Children);
            else{if(node.Left>=0)children.Add(node.Left);if(node.Right>=0)children.Add(node.Right);}
            float min=float.MaxValue,max=float.MinValue;
            foreach(int child in children)
            {
                float x=Add(state,child,depth+1,result,ref leaves);
                min=Math.Min(min,x);max=Math.Max(max,x);
                result.Edges.Add(new Edge(id,child));
            }
            float ownX=children.Count==0?leaves++:(min+max)*.5f;
            result.Nodes.Add(new Position{Id=id,Depth=depth,X=ownX});
            return ownX;
        }
        private static Vector2 Screen(Position node)
        { return new Vector2(.5f+(node.X-.5f)*zoom+panX,.5f+(node.Y-.5f)*zoom+panY); }
        public static void Draw(Transform graph,TLSTreeState state,int focus,Action redraw)
        {
            Layout layout=Compute(state);
            TLSUiFactory.Label(graph,"Connected tree heading",state.Kind.ToString().ToUpperInvariant()+"  |  ROOT "+state.Address(state.Root)+
                "  |  NODES "+state.Count+"  |  HEIGHT "+(layout.Valid?layout.Levels.ToString():"PENDING")+" LEVELS",V(.026f,.914f),V(.97f,.994f),15,TLSUiFactory.Cyan);
            TLSUiFactory.MakeButton(graph,"ZOOM +",V(.48f,.825f),V(.616f,.91f),()=>{zoom=Math.Min(2.2f,zoom+.2f);redraw();},TLSUiFactory.Slate,11);
            TLSUiFactory.MakeButton(graph,"ZOOM -",V(.624f,.825f),V(.755f,.91f),()=>{zoom=Math.Max(.7f,zoom-.2f);redraw();},TLSUiFactory.Slate,11);
            TLSUiFactory.MakeButton(graph,"CENTER",V(.763f,.825f),V(.97f,.91f),()=>{zoom=1;panX=panY=0;redraw();},TLSUiFactory.Slate,11);
            RectTransform viewport=TLSUiFactory.Panel(graph,"Connected tree viewport",V(.026f,.145f),V(.972f,.815f),TLSUiFactory.Deep);
            viewport.gameObject.AddComponent<RectMask2D>();
            TLSPhase8DiagramLines lines=TLSUiFactory.Rect(viewport,"True parent child links",Vector2.zero,Vector2.one).gameObject.AddComponent<TLSPhase8DiagramLines>();
            lines.raycastTarget=false;
            foreach(Edge edge in layout.Edges)
            {
                Position a=layout.Find(edge.From),b=layout.Find(edge.To);
                if(a!=null&&b!=null)lines.Segments.Add(new TLSPhase8DiagramLines.Segment(Screen(a),Screen(b)));
            }
            if (!layout.Valid)
                TLSUiFactory.Label(graph,"Partial trace note","Intermediate pointer state: links are pending; no diagram is invented.",V(.035f,.49f),V(.965f,.74f),15,TLSUiFactory.Amber);
            float nodeWidth=Mathf.Clamp(.76f/Mathf.Max(layout.LeafCount,3),.075f,.22f)*zoom;
            float nodeHeight=Mathf.Min(.20f,layout.Levels<=1?.24f:.60f/Mathf.Max(1,layout.Levels-1))*zoom;
            foreach(Position node in layout.Nodes)
            {
                int id=node.Id;TLSTreeNode item=state.Find(id);Vector2 center=Screen(node);
                bool highlighted=id==focus||id==selected;
                RectTransform tile=TLSUiFactory.Panel(viewport,"Tree node "+id,
                    V(center.x-nodeWidth*.5f,center.y-nodeHeight*.5f),V(center.x+nodeWidth*.5f,center.y+nodeHeight*.5f),
                    highlighted?new Color(.14f,.36f,.48f,1):TLSUiFactory.Slate);
                Button clickable=tile.gameObject.AddComponent<Button>();
                clickable.onClick.AddListener(()=>{selected=id;redraw();});
                TLSUiFactory.Label(tile,"Node value",item.Value.ToString()+"\n"+state.Address(id),V(.03f,.04f),V(.97f,.97f),
                    11,TLSUiFactory.Pale,TextAnchor.MiddleCenter,highlighted);
            }
            int target=state.Find(selected)!=null?selected:focus;
            TLSTreeNode chosen=state.Find(target);
            string description=chosen==null?"Select a node for exact symbolic pointers." :
                "SELECTED "+chosen.Value+"  "+state.Address(chosen.Id)+"  P="+state.Address(chosen.Parent)+
                (state.Kind==TLSTreeKind.General?"  CHILDREN="+chosen.Children.Count:
                    "  L="+state.Address(chosen.Left)+"  R="+state.Address(chosen.Right));
            TLSUiFactory.Label(graph,"Tree node inspector",description,V(.022f,.095f),V(.98f,.14f),12,TLSUiFactory.Amber);
            TLSUiFactory.MakeButton(graph,"<",V(.024f,.02f),V(.135f,.093f),()=>{panX=Mathf.Min(.35f,panX+.08f);redraw();},TLSUiFactory.Slate,11);
            TLSUiFactory.MakeButton(graph,">",V(.143f,.02f),V(.249f,.093f),()=>{panX=Mathf.Max(-.35f,panX-.08f);redraw();},TLSUiFactory.Slate,11);
            TLSUiFactory.MakeButton(graph,"UP",V(.26f,.02f),V(.365f,.093f),()=>{panY=Mathf.Max(-.35f,panY-.08f);redraw();},TLSUiFactory.Slate,11);
            TLSUiFactory.MakeButton(graph,"DOWN",V(.373f,.02f),V(.513f,.093f),()=>{panY=Mathf.Min(.35f,panY+.08f);redraw();},TLSUiFactory.Slate,11);
            TLSUiFactory.Label(graph,"Diagram disclaimer","Actual modeled links; symbolic addresses. Pan / zoom never changes the tree.",V(.53f,.02f),V(.97f,.095f),11,TLSUiFactory.Muted);
        }
    }

    /// <summary>UI graphic producing pixel-correct line quads after canvas sizing.</summary>
    public sealed class TLSPhase8DiagramLines : Graphic
    {
        public struct Segment
        {
            public Vector2 A,B;
            public Segment(Vector2 a,Vector2 b){A=a;B=b;}
        }
        public readonly List<Segment> Segments=new List<Segment>();
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r=rectTransform.rect;
            foreach(Segment edge in Segments)
            {
                Vector2 a=new Vector2(r.xMin+edge.A.x*r.width,r.yMin+edge.A.y*r.height);
                Vector2 b=new Vector2(r.xMin+edge.B.x*r.width,r.yMin+edge.B.y*r.height);
                Vector2 vector=b-a;
                if(vector.sqrMagnitude<.1f)continue;
                Vector2 normal=new Vector2(-vector.y,vector.x).normalized*1.5f;
                int index=vh.currentVertCount;
                Add(vh,a-normal);Add(vh,a+normal);Add(vh,b+normal);Add(vh,b-normal);
                vh.AddTriangle(index,index+1,index+2);vh.AddTriangle(index,index+2,index+3);
            }
        }
        private static void Add(VertexHelper vh,Vector2 p)
        {
            UIVertex vertex=UIVertex.simpleVert;
            vertex.color=new Color32(56,200,244,235);
            vertex.position=new Vector3(p.x,p.y,0);
            vh.AddVert(vertex);
        }
    }
}
