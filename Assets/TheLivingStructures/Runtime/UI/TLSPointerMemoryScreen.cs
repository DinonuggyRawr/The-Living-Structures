using System;
using TLS.Core;
using UnityEngine;

namespace TLS.UI
{
    // Purely presentational Unity uGUI screen. All memory states come from a deterministic trace.
    public static class TLSPointerMemoryScreen
    {
        private static Vector2 V(float x, float y) { return new Vector2(x, y); }
        private static readonly string[] PointerNames = { "head", "node", "alias", "second" };

        public static void Render(Transform root, TLSPointerMemorySession session, Action redraw)
        {
            if (root == null || session == null || redraw == null)
                throw new ArgumentNullException("Pointer view requires a root, session, and redraw callback.");
            TLSPointerMemoryLesson lesson = session.Lesson;
            TLSPointerMemoryFrame frame = session.Frame;
            TLSPointerMemoryState state = frame.State;

            TLSUiFactory.Panel(root, "Pointer Lab Background", V(0, 0), V(1, 0.906f), TLSUiFactory.Navy);
            RectTransform choices = TLSUiFactory.Panel(root, "Pointer lesson choices", V(0.019f, 0.787f),
                V(0.981f, 0.898f), TLSUiFactory.Slate);
            for (int i = 0; i < 9; i++)
            {
                TLSPointerLessonId lessonId = (TLSPointerLessonId)i;
                int column = i % 5;
                int row = i / 5;
                float x0 = 0.012f + column * 0.197f;
                float x1 = x0 + 0.187f;
                float y0 = row == 0 ? 0.53f : 0.08f;
                float y1 = row == 0 ? 0.945f : 0.485f;
                TLSUiFactory.MakeButton(choices, TLSPointerMemoryCatalog.ShortName(lessonId),
                    V(x0,y0), V(x1,y1), () => { session.Select(lessonId); redraw(); },
                    lessonId == lesson.Id ? new Color(0.13f, 0.39f, 0.52f, 1f) : TLSUiFactory.Deep, 13);
            }

            RectTransform code = TLSUiFactory.Panel(root, "Pointer C code", V(0.018f,0.284f),
                V(0.385f,0.775f), TLSUiFactory.Slate);
            TLSUiFactory.Label(code, "Code heading", "C11 / " + lesson.Title.ToUpperInvariant(),
                V(0.035f,0.898f),V(0.97f,0.98f),20,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(code, "Code context", "Fragments assume Node typedef and stdlib.h. SIMULATED, not compiled.",
                V(0.035f,0.806f),V(0.975f,0.89f),14,TLSUiFactory.Muted);
            float height = 0.69f / Math.Max(9, lesson.Code.Length);
            for (int i = 0; i < lesson.Code.Length; i++)
            {
                float yTop=0.798f-i*height;
                float yBottom=yTop-height*0.92f;
                bool highlighted=frame.CodeLine==i;
                RectTransform line = TLSUiFactory.Panel(code, "C line " + i, V(0.027f,yBottom),V(0.973f,yTop),
                    highlighted ? new Color(0.12f,0.32f,0.43f,1f) : TLSUiFactory.Slate);
                TLSUiFactory.Label(line,"Text",(i+1).ToString("00")+"  "+lesson.Code[i],
                    V(0.021f,0.03f),V(0.988f,0.97f),16,
                    highlighted ? TLSUiFactory.Pale : TLSUiFactory.Muted,TextAnchor.MiddleLeft,highlighted);
            }
            TLSUiFactory.Label(code, "Code scope", "Synthetic teaching trace: not a C debugger; invalid access is never executed.",
                V(0.035f,0.016f),V(0.968f,0.103f),13,TLSUiFactory.Amber);

            RectTransform memory = TLSUiFactory.Panel(root,"Pointer memory viewport",V(0.395f,0.284f),
                V(0.736f,0.775f),TLSUiFactory.Deep);
            TLSUiFactory.Label(memory,"Memory heading","POINTERS / SYMBOLIC HEAP",
                V(0.04f,0.912f),V(0.97f,0.99f),18,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(memory,"Memory legend","Arrows show references, not copied structs. 0x1000 / 0x1020 are invented.",
                V(0.04f,0.848f),V(0.97f,0.92f),13,TLSUiFactory.Muted);
            for (int i = 0; i < PointerNames.Length; i++)
            {
                string name = PointerNames[i];
                float top = 0.834f-i*0.093f;
                string status = state.PointerStatus(name);
                bool selected = name == frame.FocusVariable;
                bool dangling=status.StartsWith("DANGLING",StringComparison.Ordinal);
                RectTransform ptr = TLSUiFactory.Panel(memory,"Pointer_"+name,V(0.055f,top-0.084f),V(0.946f,top),
                    selected ? new Color(0.15f,0.36f,0.43f,1) : TLSUiFactory.Slate);
                TLSUiFactory.Label(ptr,"Symbol",name,V(0.03f,0.02f),V(0.29f,0.98f),17,TLSUiFactory.Pale,TextAnchor.MiddleLeft,true);
                TLSUiFactory.Label(ptr,"Arrow and target","→  "+status,V(0.3f,0.02f),V(0.98f,0.98f),
                    14,dangling?TLSUiFactory.Amber:selected?TLSUiFactory.Cyan:TLSUiFactory.Muted);
            }
            TLSUiFactory.Label(memory,"Heap label","MODELED HEAP / NODE STORAGE",
                V(0.055f,0.378f),V(0.945f,0.451f),15,TLSUiFactory.Violet,TextAnchor.MiddleLeft,true);
            for (int i = 0; i < 2; i++)
            {
                string id = "object"+(i+1);
                TLSHeapNode node = state.Node(id);
                float x0 = i == 0 ? 0.055f : 0.512f;
                float x1 = i == 0 ? 0.49f : 0.946f;
                bool focus = frame.FocusNode == id;
                RectTransform tile = TLSUiFactory.Panel(memory,"Heap_"+id,V(x0,0.084f),V(x1,0.369f),
                    focus ? new Color(0.15f,0.34f,0.41f,1f):TLSUiFactory.Slate);
                if (node == null)
                {
                    TLSUiFactory.Label(tile,"Unallocated",i==0?"0x1000\nNO ALLOCATION":"0x1020\nNO ALLOCATION",
                        V(0.06f,0.08f),V(0.95f,0.93f),15,TLSUiFactory.Muted);
                }
                else
                {
                    string availability = node.Released ? "FREED / NO ACCESS" : state.IsUnreachable(node.Id) ?
                        "UNREACHABLE (LEAK?)" : "LIVE";
                    string fields = node.Released ? "Fields cannot be read" :
                        "data: "+(node.DataInitialized?node.Data.ToString():"UNINIT")+"\nnext: "+
                        (node.NextInitialized?(node.NextId==null?"NULL":state.Node(node.NextId).Address):"UNINIT");
                    TLSUiFactory.Label(tile,"Address",node.Address+"  /  "+availability,
                        V(0.05f,0.67f),V(0.98f,0.98f),14,
                        node.Released||state.IsUnreachable(node.Id)?TLSUiFactory.Amber:TLSUiFactory.Green,
                        TextAnchor.MiddleLeft,true);
                    TLSUiFactory.Label(tile,"Fields",fields,V(0.05f,0.09f),V(0.98f,0.69f),16,TLSUiFactory.Pale);
                }
            }
            TLSUiFactory.Label(memory,"Memory disclaimer","Model allocations only. Freed-pointer drawings are conceptual, not usable C values.",
                V(0.045f,0.008f),V(0.97f,0.08f),12,TLSUiFactory.Amber);

            RectTransform why=TLSUiFactory.Panel(root,"Pointer explanation",V(0.746f,0.284f),
                V(0.981f,0.775f),TLSUiFactory.Slate);
            TLSUiFactory.Label(why,"Learning objective","LEARNING OBJECTIVE",V(0.05f,0.885f),V(0.96f,0.968f),
                18,TLSUiFactory.Violet,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(why,"Goal",lesson.Goal,V(0.05f,0.722f),V(0.955f,0.872f),17,TLSUiFactory.Pale);
            TLSUiFactory.Label(why,"What happened","WHY THIS STEP MATTERS",V(0.05f,0.625f),V(0.96f,0.708f),
                17,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(why,"Explanation",frame.Explanation,V(0.05f,0.316f),V(0.955f,0.618f),
                17,frame.UnsafeAccessBlocked?TLSUiFactory.Amber:TLSUiFactory.Pale);
            string safety=frame.UnsafeAccessBlocked ? "INVALID DEREFERENCE: BLOCKED" :
                frame.Event==TLSPointerEvent.AllocationRejected ? "ALLOCATION FAILED / GUARDED" :
                frame.Event==TLSPointerEvent.Release ? "OBJECT LIFETIME ENDED" :
                "TRACE STEP "+session.Step+" / "+(lesson.Frames.Count-1);
            TLSUiFactory.Label(why,"Safety",safety,V(0.05f,0.213f),V(0.96f,0.305f),15,TLSUiFactory.Amber,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(why,"Assessment disclaimer","Prediction = formative practice only. No C mastery or saved progress is awarded.",
                V(0.05f,0.025f),V(0.96f,0.205f),14,TLSUiFactory.Muted);

            RectTransform controls=TLSUiFactory.Panel(root,"Pointer trace controls",V(0.019f,0.016f),
                V(0.981f,0.268f),TLSUiFactory.Slate);
            TLSUiFactory.Label(controls,"Trace header","EXECUTION / "+lesson.Title,
                V(0.016f,0.82f),V(0.58f,0.99f),18,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(controls,"Step count","STEP "+session.Step+" / "+(lesson.Frames.Count-1),
                V(0.77f,0.82f),V(0.985f,0.99f),16,TLSUiFactory.Muted,TextAnchor.MiddleRight,true);
            TLSUiFactory.MakeButton(controls,"< PREVIOUS",V(0.014f,0.58f),V(0.192f,0.79f),
                () => {session.Previous();redraw();},TLSUiFactory.Deep,16,session.Step>0);
            TLSUiFactory.MakeButton(controls,"NEXT STEP >",V(0.203f,0.58f),V(0.383f,0.79f),
                () => {session.Next();redraw();},TLSUiFactory.Deep,16,!session.IsFinished);
            TLSUiFactory.MakeButton(controls,"FINISH",V(0.395f,0.58f),V(0.57f,0.79f),
                () => {session.Finish();redraw();},TLSUiFactory.Deep,16,!session.IsFinished);
            TLSUiFactory.MakeButton(controls,"RESET TRACE",V(0.582f,0.58f),V(0.772f,0.79f),
                () => {session.Reset();redraw();},TLSUiFactory.Deep,16);
            TLSUiFactory.Label(controls,"Prediction label",lesson.PredictionQuestion,V(0.014f,0.40f),
                V(0.99f,0.565f),17,TLSUiFactory.Pale,TextAnchor.MiddleLeft,true);
            bool canPredict=session.Step==0 && !session.Prediction.HasValue;
            TLSUiFactory.MakeButton(controls,"A: "+lesson.PredictionA,V(0.014f,0.158f),V(0.484f,0.393f),
                () => {session.Choose(0);redraw();},TLSUiFactory.Deep,14,canPredict);
            TLSUiFactory.MakeButton(controls,"B: "+lesson.PredictionB,V(0.502f,0.158f),V(0.985f,0.393f),
                () => {session.Choose(1);redraw();},TLSUiFactory.Deep,14,canPredict);
            TLSUiFactory.Label(controls,"Prediction feedback",session.PredictionReview+
                "  ("+session.Correct+"/"+session.Attempts+" correct in this session)",
                V(0.014f,0.007f),V(0.985f,0.15f),14,session.IsFinished?TLSUiFactory.Green:TLSUiFactory.Muted);
        }
    }
}
