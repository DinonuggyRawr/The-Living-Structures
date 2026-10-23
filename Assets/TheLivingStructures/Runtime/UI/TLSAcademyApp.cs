using System;
using TLS.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TLS.UI
{
    /// <summary>
    /// 3D Academy, trace-driven 2D Stack Laboratory and structured C practice.
    /// Student-written C is compiled to WebAssembly only with opt-in toolchain installation.
    /// </summary>
    public sealed class TLSAcademyApp : MonoBehaviour
    {
        private enum ScreenMode { Academy, Stack, Challenge, CodePractice, FunctionLab, WasmCompilerLab, Arrays, Queues, ArrayCoding, QueueCoding, ArrayFunctions, QueueFunctions, PointerMemory, LinkedVisual, LinkedCoding, LinkedFunctions, Recursion, TreeVisual, TreeCoding, TreeFunctions, Heap, Graph, Hash, Phase5Coding, Phase5Functions, Analysis, Debugging, Quality, Phase6Functions, Phase7Dashboard, Phase7Exam, Phase7Reconstruct, Phase7TestDesign, Phase7Transfer }
        private ScreenMode mode = ScreenMode.Academy;
        private Canvas canvas;
        private RectTransform screenRoot;
        private Camera academyCamera;
        private StackState current = new StackState();
        private StackTrace activeTrace;
        private int frameIndex;
        private string valueToPush = "12";
        private string feedback = "Choose PUSH, POP, or PEEK to inspect a real C operation.";
        private int challengeIndex;
        private int challengeMistakes;
        private string topAnswer = "";
        private string countAnswer = "";
        private string challengeFeedback = "No visualization is provided during these independent tracing questions.";
        private bool challengeFinished;
        private float yawOffset;
        // Local presentation choices; neither is an Academy mastery or progress flag.
        private readonly StackPracticeSession codePractice = new StackPracticeSession();
        private readonly StackFunctionLabSession functionPractice = new StackFunctionLabSession();
        private readonly StackWasmLabSession wasmPractice = new StackWasmLabSession();
        private readonly CArrayLabSession arraysPractice = new CArrayLabSession();
        private readonly CQueueLabSession queuesPractice = new CQueueLabSession();
        private readonly LinearPracticeSession arraysCoding = new LinearPracticeSession(LinearPracticeDomain.Array);
        private readonly LinearPracticeSession queuesCoding = new LinearPracticeSession(LinearPracticeDomain.Queue);
        private readonly TLSLinearFunctionLabSession arrayFunctions = new TLSLinearFunctionLabSession(true);
        private readonly TLSLinearFunctionLabSession queueFunctions = new TLSLinearFunctionLabSession(false);
        private readonly TLSLinearWasmLabSession linearWasm = new TLSLinearWasmLabSession();
        private readonly TLSPointerMemorySession pointerMemory = new TLSPointerMemorySession();
        private readonly TLSLinkedLabSession linkedLab = new TLSLinkedLabSession();
        private readonly TLSLinkedCodingSession linkedCoding = new TLSLinkedCodingSession();
        private readonly TLSLinkedFunctionLabSession linkedFunctions = new TLSLinkedFunctionLabSession();
        private readonly TLSLinkedWasmLabSession linkedWasm = new TLSLinkedWasmLabSession();
        private readonly TLSRecursionLab phase4Recursion = new TLSRecursionLab();
        private readonly TLSTreeSession phase4Trees = new TLSTreeSession();
        private readonly TLSPhase4PracticeSession phase4Practice = new TLSPhase4PracticeSession();
        private readonly TLSPhase4FunctionLabSession phase4Functions = new TLSPhase4FunctionLabSession();
        private readonly TLSPhase4WasmLabSession phase4Wasm = new TLSPhase4WasmLabSession();
        private readonly TLSHeapSession phase5Heap = new TLSHeapSession();
        private readonly TLSGraphSession phase5Graph = new TLSGraphSession();
        private readonly TLSHashSession phase5Hash = new TLSHashSession();
        private readonly TLSPhase5PracticeSession phase5Practice = new TLSPhase5PracticeSession();
        private readonly TLSPhase5FunctionLabSession phase5Functions = new TLSPhase5FunctionLabSession();
        private readonly TLSPhase5WasmLabSession phase5Wasm = new TLSPhase5WasmLabSession();
        private readonly TLSAnalysisSession phase6Analysis = new TLSAnalysisSession();
        private readonly TLSDebugSession phase6Debug = new TLSDebugSession();
        private readonly TLSQualitySession phase6Quality = new TLSQualitySession();
        private readonly TLSPhase6FunctionSession phase6Functions = new TLSPhase6FunctionSession();
        private readonly TLSPhase6WasmLabSession phase6Wasm = new TLSPhase6WasmLabSession();
        private string phase6Prediction = "0";
        // Phase 7 has its own versioned learning evidence; never rewrite earlier Academy saves.
        private TLSPhase7ProgressBook phase7Book;
        private bool phase7RecordInvalid;
        private readonly TLSPhase7Assessment phase7Diagnostic = new TLSPhase7Assessment(false);
        private readonly TLSPhase7Assessment phase7Final = new TLSPhase7Assessment(true);
        private readonly TLSPhase7ReviewSession phase7Review = new TLSPhase7ReviewSession();
        private readonly TLSPhase7ReconstructionSession phase7Reconstruction = new TLSPhase7ReconstructionSession();
        private readonly TLSPhase7TestDesignSession phase7TestDesign = new TLSPhase7TestDesignSession();
        private readonly TLSPhase7FunctionSession phase7Functions = new TLSPhase7FunctionSession();
        private readonly TLSPhase7WasmSession phase7Wasm = new TLSPhase7WasmSession();
        private int phase7TopicPick, phase7ExamMode;
        private string phase7Order = "", phase7TestCsv="1,2,3", phase7TestArg="3", phase7TestExpected="2";
        private ScreenMode phase5ReturnScreen = ScreenMode.Heap;
        private int academyDirectoryGroup;
        // Phase 8: pointer-only selection of existing Academy scene landmarks.
        private int phase8SelectedHub = -1;
        // Academy badge only: active compiler sessions perform their own fresh integrity checks.
        private readonly TLSPhase8AcademyReadinessCache academyToolchainBadge =
            new TLSPhase8AcademyReadinessCache();
        private float nextSlowUiLogTime;
        private string phase5HeapInput = "12";
        private string phase5GraphA = "A";
        private string phase5GraphB = "B";
        private string phase5HashInput = "8";
        private string phase4RecursionInput = "4";
        private string phase4PredictionInput = "";
        private string phase4Value = "8";
        private string phase4Parent = "8";
        private string linkedValue = "42";
        private string linkedIndex = "0";
        private bool academyExpanded;
        private bool predictionPending;
        private StackOperation predictionOperation;
        private int predictionInput;
        private StackCountPrediction? predictionChoice;
        private string predictionReview = "";

        private static Vector2 V(float x, float y) { return new Vector2(x, y); }

        private void Start()
        {
            academyCamera = Camera.main;
            if (EventSystem.current == null)
            {
                GameObject system = new GameObject("TLS EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                system.transform.SetParent(transform, false);
            }
            GameObject root = new GameObject("TLS UI Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            screenRoot = (RectTransform)root.transform;
            LoadPhase7Evidence();
            Render();
        }

        private void Clear()
        {
            for (int i = screenRoot.childCount - 1; i >= 0; i--)
            {
                GameObject old = screenRoot.GetChild(i).gameObject;
                old.SetActive(false);
                Destroy(old);
            }
        }

        private void Render()
        {
            // Phase 8 responsiveness review: quantify synchronous uGUI rebuild work.
            // The profiler's later Canvas.BuildBatch/layout work is separate and must
            // be inspected in a Unity development build if total click lag remains high.
            long begun = System.Diagnostics.Stopwatch.GetTimestamp();
            UnityEngine.Profiling.Profiler.BeginSample("TLS.UI.FullRender");
            try
            {
                UnityEngine.Profiling.Profiler.BeginSample("TLS.UI.ClearOldScreen");
                try { Clear(); }
                finally { UnityEngine.Profiling.Profiler.EndSample(); }
                long cleared = System.Diagnostics.Stopwatch.GetTimestamp();
                if (academyCamera != null)
                    academyCamera.enabled = true;
                UnityEngine.Profiling.Profiler.BeginSample("TLS.UI.BuildScreen");
                try
                {
                    if (mode == ScreenMode.Academy) RenderAcademy();
                    else if (mode == ScreenMode.Stack) RenderStack();
                    else if (mode == ScreenMode.Challenge) RenderChallenge();
                    else if (mode == ScreenMode.CodePractice) RenderCodePractice();
                    else if (mode == ScreenMode.FunctionLab) RenderFunctionLab();
                    else if (mode == ScreenMode.Arrays) RenderArraysLab();
                    else if (mode == ScreenMode.Queues) RenderQueuesLab();
                    else if (mode == ScreenMode.ArrayCoding) RenderLinearCoding(true);
                    else if (mode == ScreenMode.QueueCoding) RenderLinearCoding(false);
                    else if (mode == ScreenMode.ArrayFunctions) RenderLinearFunctions(true);
                    else if (mode == ScreenMode.QueueFunctions) RenderLinearFunctions(false);
                    else if (mode == ScreenMode.PointerMemory) RenderPointerMemoryLab();
                    else if (mode == ScreenMode.LinkedVisual) RenderLinkedVisual();
                    else if (mode == ScreenMode.LinkedCoding) RenderLinkedCoding();
                    else if (mode == ScreenMode.LinkedFunctions) RenderLinkedFunctions();
                    else if (mode == ScreenMode.Recursion) RenderPhase4Recursion();
                    else if (mode == ScreenMode.TreeVisual) RenderPhase4Tree();
                    else if (mode == ScreenMode.TreeCoding) RenderPhase4Coding();
                    else if (mode == ScreenMode.TreeFunctions) RenderPhase4Functions();
                    else if (mode == ScreenMode.Heap) RenderPhase5Heap();
                    else if (mode == ScreenMode.Graph) RenderPhase5Graph();
                    else if (mode == ScreenMode.Hash) RenderPhase5Hash();
                    else if (mode == ScreenMode.Phase5Coding) RenderPhase5Coding();
                    else if (mode == ScreenMode.Phase5Functions) RenderPhase5Functions();
                    else if (mode == ScreenMode.Analysis) RenderPhase6Analysis();
                    else if (mode == ScreenMode.Debugging) RenderPhase6Debugging();
                    else if (mode == ScreenMode.Quality) RenderPhase6Quality();
                    else if (mode == ScreenMode.Phase6Functions) RenderPhase6Functions();
                    else if (mode == ScreenMode.Phase7Dashboard) RenderPhase7Dashboard();
                    else if (mode == ScreenMode.Phase7Exam) RenderPhase7Exam();
                    else if (mode == ScreenMode.Phase7Reconstruct) RenderPhase7Reconstruct();
                    else if (mode == ScreenMode.Phase7TestDesign) RenderPhase7TestDesign();
                    else if (mode == ScreenMode.Phase7Transfer) RenderPhase7Transfer();
                    else RenderWasmCompilerLab();
                }
                finally { UnityEngine.Profiling.Profiler.EndSample(); }
                long built = System.Diagnostics.Stopwatch.GetTimestamp();
                UnityEngine.Profiling.Profiler.BeginSample("TLS.UI.AccessibilityAndExit");
                try
                {
                    TLSPhase8Accessibility.Draw(screenRoot, Render);
                    TLSPhase8ExitMenu.Draw(screenRoot, QuitApplication);
                    TLSPhase8Accessibility.Apply(screenRoot);
                }
                finally { UnityEngine.Profiling.Profiler.EndSample(); }
                long completed = System.Diagnostics.Stopwatch.GetTimestamp();
                // Only report genuinely slow draws. Never log during EditMode tests,
                // and throttle runtime warnings to avoid creating additional stalls.
                double ticksToMs = 1000.0 / System.Diagnostics.Stopwatch.Frequency;
                double totalMs = (completed - begun) * ticksToMs;
                if (Application.isPlaying && totalMs >= 250.0f &&
                    Time.realtimeSinceStartup >= nextSlowUiLogTime)
                {
                    nextSlowUiLogTime = Time.realtimeSinceStartup + 5f;
                    Debug.LogWarning("TLS UI redraw " + mode + ": " +
                        totalMs.ToString("F0") + "ms (clear " +
                        ((cleared - begun) * ticksToMs).ToString("F0") +
                        "ms, build " + ((built - cleared) * ticksToMs).ToString("F0") +
                        "ms, styling " + ((completed - built) * ticksToMs).ToString("F0") +
                        "ms). If click lag is larger, inspect Canvas.BuildBatch and GC in the Unity Profiler.");
                }
            }
            finally { UnityEngine.Profiling.Profiler.EndSample(); }
        }

        // A standalone player exits normally; in the Editor, this stops Play Mode
        // without closing the developer's Unity Editor or modifying learning saves.
        private static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Header(string section, bool backToAcademy)
        {
            RectTransform bar = TLSUiFactory.Panel(screenRoot, "Header", V(0, 0.91f), V(1, 1), TLSUiFactory.Navy);
            TLSUiFactory.Label(bar, "App title", "THE LIVING STRUCTURES", V(0.018f, 0.37f), V(0.56f, 0.91f),
                29, TLSUiFactory.Pale, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(bar, "Section", section, V(0.018f, 0.08f), V(0.66f, 0.42f),
                17, TLSUiFactory.Violet);
            if (backToAcademy)
                TLSUiFactory.MakeButton(bar, "BACK TO ACADEMY", V(0.79f, 0.18f), V(0.985f, 0.78f),
                    () => { predictionPending = false; mode = ScreenMode.Academy; Render(); }, TLSUiFactory.Slate, 16);
        }

        private void RenderAcademy()
        {
            Header("Phase 8  |  Computing II Academy — Learning & Quality", false);
            // Badge status is stable during this application session. Never rehash
            // the large Zig/Wasmtime executables for each Academy button click.
            // The real C11 runners still validate bundle hashes on each run.
            TLSBundledToolchain.CheckResult offline = academyToolchainBadge.Get(
                TLSBundledToolchain.RootFromUnityDataPath(Application.dataPath),
                !Application.isEditor, TLSBundledToolchain.Check);
            TLSUiFactory.Label(screenRoot, "Offline C compiler readiness",
                offline.Ready ? (offline.ManifestVerified ? "C11 BUNDLE: EXE HASHES VERIFIED" : "C11 COMPILER: DEV TOOLS READY") :
                    "C11 COMPILER UNAVAILABLE — VISUAL LABS STILL WORK",
                V(.022f,.013f),V(.75f,.073f),15,offline.Ready?TLSUiFactory.Green:TLSUiFactory.Amber,TextAnchor.MiddleLeft,true);
            TLSUiFactory.MakeButton(screenRoot,academyExpanded?"FOCUS ON CAMPUS":"SHOW LEARNING PATH",
                V(.79f,.928f),V(.985f,.977f),()=>{academyExpanded=!academyExpanded;Render();},TLSUiFactory.Slate,16);
            // Directory deliberately stays left of the central buildings; no central stack of buttons.
            RectTransform directory=TLSUiFactory.Panel(screenRoot,"Academy laboratory directory",
                V(.022f,.175f),V(.303f,.874f),new Color(.09f,.16f,.25f,.96f));
            TLSUiFactory.Label(directory,"Directory header","LABORATORY DIRECTORY",V(.055f,.881f),V(.95f,.977f),22,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
            TLSUiFactory.Label(directory,"Building hint","Subject buildings organize these lessons. Quick navigation remains available.",
                V(.055f,.790f),V(.95f,.882f),13,TLSUiFactory.Muted);
            string[] groups={"FOUNDATIONS","STRUCTURES","ADVANCED","LEARNING"};
            for(int i=0;i<groups.Length;i++)
            {
                int pick=i;float x=.014f+i*.247f;
                TLSUiFactory.MakeButton(directory,groups[i],V(x,.713f),V(x+.238f,.785f),()=>{academyDirectoryGroup=pick;Render();},
                    academyDirectoryGroup==i?TLSUiFactory.Violet:TLSUiFactory.Deep,12);
            }
            string[] labels;
            Action[] actions;
            if(academyDirectoryGroup==0)
            {
                labels=new[]{"ENTER STACK LAB","ENTER ARRAY LAB","ENTER QUEUE LAB","ENTER POINTER & MEMORY LAB"};
                actions=new Action[]{OpenStack,()=>{mode=ScreenMode.Arrays;Render();},()=>{mode=ScreenMode.Queues;Render();},()=>{mode=ScreenMode.PointerMemory;Render();}};
            }
            else if(academyDirectoryGroup==1)
            {
                labels=new[]{"ENTER LINKED LIST LAB","ENTER RECURSION LAB","ENTER TREE & BST LAB","ENTER HEAP LAB"};
                actions=new Action[]{()=>{mode=ScreenMode.LinkedVisual;Render();},()=>{mode=ScreenMode.Recursion;Render();},()=>{mode=ScreenMode.TreeVisual;Render();},()=>{mode=ScreenMode.Heap;Render();}};
            }
            else if(academyDirectoryGroup==2)
            {
                labels=new[]{"ENTER GRAPH LAB","ENTER HASH TABLE LAB","ALGORITHM ANALYSIS CENTER","DEBUGGING & TESTING CENTER"};
                actions=new Action[]{()=>{mode=ScreenMode.Graph;Render();},()=>{mode=ScreenMode.Hash;Render();},
                    ()=>{mode=ScreenMode.Analysis;Render();},()=>{mode=ScreenMode.Debugging;Render();}};
            }
            else
            {
                labels=new[]{"LEARNING DASHBOARD","EXAMINATION HALL","RECONSTRUCT C CODE","STUDENT TEST DESIGN"};
                actions=new Action[]{()=>GoPhase7(ScreenMode.Phase7Dashboard),()=>GoPhase7(ScreenMode.Phase7Exam),
                    ()=>GoPhase7(ScreenMode.Phase7Reconstruct),()=>GoPhase7(ScreenMode.Phase7TestDesign)};
            }
            for(int i=0;i<labels.Length;i++)
            {
                int at=i;float top=.699f-i*.131f;
                TLSUiFactory.MakeButton(directory,labels[i],V(.053f,top-.115f),V(.95f,top),()=>actions[at](),TLSUiFactory.Deep,16);
            }
            // Keep availability notes below the fourth directory button; the previous
            // advanced-only label overlapped the Analysis Center entrance.
            TLSUiFactory.Label(directory,"Directory footer",
                academyDirectoryGroup==3 ? "PHASE 7 LEARNING — diagnostic, independent work, review." :
                    "DIRECTORY MODE — explore the 3D hub, or jump straight into a lab.",
                V(.054f,.045f),V(.953f,.16f),13,TLSUiFactory.Muted);
            RectTransform cameraControls=TLSUiFactory.Panel(screenRoot,"Compact camera controls",V(.766f,.105f),V(.982f,.196f),
                new Color(.09f,.16f,.25f,.94f));
            AddRotationControls(cameraControls,.14f,.86f);
            if(academyExpanded)
            {
                RectTransform info=TLSUiFactory.Panel(screenRoot,"Expanded learning path",V(.762f,.248f),V(.985f,.825f),
                    new Color(.09f,.16f,.25f,.96f));
                TLSUiFactory.Label(info,"Learning header","YOUR LEARNING PATH",V(.06f,.84f),V(.96f,.96f),20,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
                TLSUiFactory.Label(info,"Progress text",ProgressLabel(),V(.065f,.46f),V(.96f,.815f),16,TLSUiFactory.Muted);
                TLSUiFactory.Label(info,"Learning note","Viewed != guided completion != independent code mastery.\nUse the directory to reach every lab.",
                    V(.06f,.12f),V(.96f,.42f),16,TLSUiFactory.Amber);
            }
            // The five actual 3D buildings from the original Academy scene are
            // selectable destinations. A directory remains available for the other hubs.
            if (!academyExpanded)
            {
                RectTransform selected=TLSUiFactory.Panel(screenRoot,"Phase8 building inspector",
                    V(.763f,.260f),V(.985f,.705f),new Color(.09f,.16f,.25f,.95f));
                TLSUiFactory.Label(selected,"Building heading","ACADEMY BUILDINGS",V(.05f,.868f),V(.95f,.97f),19,TLSUiFactory.Cyan,TextAnchor.MiddleLeft,true);
                TLSUiFactory.Label(selected,"Building selection",phase8SelectedHub<0?
                    "SELECT A LANDMARK" : TLSPhase8Campus.Hubs[phase8SelectedHub].Title,
                    V(.05f,.69f),V(.95f,.858f),17,TLSUiFactory.Pale,TextAnchor.MiddleLeft,true);
                TLSUiFactory.Label(selected,"Building purpose",phase8SelectedHub<0?
                    "Click an existing 3D building or use PREVIOUS / NEXT. Original building models are preserved." :
                    TLSPhase8Campus.Hubs[phase8SelectedHub].Purpose,
                    V(.05f,.445f),V(.95f,.677f),15,TLSUiFactory.Muted);
                TLSUiFactory.MakeButton(selected,"PREVIOUS",V(.05f,.305f),V(.48f,.427f),()=>{
                    phase8SelectedHub=(phase8SelectedHub+TLSPhase8Campus.Hubs.Length-1)%TLSPhase8Campus.Hubs.Length;Render();},TLSUiFactory.Deep,13);
                TLSUiFactory.MakeButton(selected,"NEXT",V(.52f,.305f),V(.95f,.427f),()=>{
                    phase8SelectedHub=(phase8SelectedHub+1)%TLSPhase8Campus.Hubs.Length;Render();},TLSUiFactory.Deep,13);
                TLSUiFactory.MakeButton(selected,phase8SelectedHub<0?"CHOOSE BUILDING":TLSPhase8Campus.Hubs[phase8SelectedHub].LabLabel,
                    V(.05f,.114f),V(.95f,.275f),()=>OpenPhase8Hub(phase8SelectedHub<0?0:phase8SelectedHub),TLSUiFactory.Violet,13);
                TLSUiFactory.Label(selected,"Building note","Five original buildings are mapped. Other subjects are in the directory; interiors are not built.",
                    V(.05f,.007f),V(.95f,.107f),10,TLSUiFactory.Muted);
            }
            RectTransform footer=TLSUiFactory.Panel(screenRoot,"Academy Footer",V(0,0),V(1,.075f),TLSUiFactory.Navy);
            TLSUiFactory.Label(footer,"Footer caption","Explore by subject. Phase 7 adds spaced review, formative diagnostics and unseen C coding; existing labs remain available.",
                V(.025f,.08f),V(.975f,.91f),16,TLSUiFactory.Muted);
        }

        private void OpenPhase8Hub(int which)
        {
            if (which < 0 || which >= TLSPhase8Campus.Hubs.Length) return;
            phase8SelectedHub = which;
            if (which == 0) { OpenStack(); return; }
            mode = which == 1 ? ScreenMode.LinkedVisual :
                   which == 2 ? ScreenMode.Recursion :
                   which == 3 ? ScreenMode.TreeVisual : ScreenMode.Graph;
            Render();
        }

        private void AddRotationControls(Transform parent, float minY, float maxY)
        {
            TLSUiFactory.MakeButton(parent, "ROTATE  <", V(0.05f, minY), V(0.49f, maxY),
                () => Rotate(-20), TLSUiFactory.Deep, 14);
            TLSUiFactory.MakeButton(parent, "ROTATE  >", V(0.51f, minY), V(0.95f, maxY),
                () => Rotate(20), TLSUiFactory.Deep, 14);
        }

        private void RenderPointerMemoryLab()
        {
            Header("Phase 3.1  |  C Pointers, malloc, free & Memory Safety", true);
            TLSPointerMemoryScreen.Render(screenRoot, pointerMemory, Render);
        }

        private void RenderLinkedVisual()
        {
            Header("Phase 3  |  Singly, Doubly, Circular & Recursive Linked Lists", true);
            TLSLinkedListScreen.Render(screenRoot, linkedLab, Render,
                () => {mode=ScreenMode.LinkedCoding;Render();},
                () => {mode=ScreenMode.LinkedFunctions;Render();},
                () => linkedValue, () => linkedIndex, s => linkedValue=s, s => linkedIndex=s);
        }
        private void RenderLinkedCoding()
        {
            Header("Phase 3  |  Pointer Code Tracing, Repair & Challenges", true);
            TLSLinkedCodingScreen.Render(screenRoot,linkedCoding,Render,
                () => {mode=ScreenMode.LinkedVisual;Render();});
        }
        private void RenderLinkedFunctions()
        {
            Header("Phase 3  |  Linked-List Real C11 Function Lab", true);
            TLSLinkedWasmScreen.Render(screenRoot,linkedFunctions,linkedWasm,Render,
                () => {mode=ScreenMode.LinkedVisual;Render();});
        }

        private void RenderPhase4Recursion()
        {
            Header("Phase 4  |  Recursive Calls, Base Cases & Unwinding", true);
            TLSPhase4VisualScreens.Recursion(screenRoot, phase4Recursion, () => phase4RecursionInput,
                s => phase4RecursionInput = s, () => phase4PredictionInput, s => phase4PredictionInput = s, Render,
                () => { mode = ScreenMode.TreeCoding; Render(); },
                () => { mode = ScreenMode.TreeFunctions; Render(); });
        }
        private void RenderPhase4Tree()
        {
            Header("Phase 4  |  General Trees, Binary Trees & BST Operations", true);
            TLSPhase4VisualScreens.Tree(screenRoot, phase4Trees,
                () => phase4Value, () => phase4Parent, s => phase4Value = s, s => phase4Parent = s,
                Render, () => { mode = ScreenMode.TreeCoding; Render(); },
                () => { mode = ScreenMode.TreeFunctions; Render(); });
        }
        private void RenderPhase4Coding()
        {
            Header("Phase 4  |  Recursive C11 Tracing & BST Debugging", true);
            TLSPhase4PracticeScreen.Render(screenRoot, phase4Practice, Render,
                () => { mode = ScreenMode.TreeVisual; Render(); });
        }
        private void RenderPhase4Functions()
        {
            Header("Phase 4  |  Full C11 BST / Recursive Function Practice", true);
            TLSPhase4WasmScreen.Render(screenRoot, phase4Functions, phase4Wasm, Render,
                () => { mode = ScreenMode.TreeVisual; Render(); });
        }

        private void EnterPhase5Practice(TLSPhase5Domain domain)
        {
            phase5ReturnScreen=mode;
            phase5Practice.Select(domain,TLSPhase5QuestionType.Trace,0);
            mode=ScreenMode.Phase5Coding;Render();
        }
        private void EnterPhase5Functions(TLSPhase5Function function)
        {
            phase5ReturnScreen=mode;
            phase5Functions.Select(function);
            mode=ScreenMode.Phase5Functions;Render();
        }
        private void ReturnPhase5(){mode=phase5ReturnScreen;Render();}
        private void RenderPhase5Heap()
        {
            Header("Phase 5  |  Array-backed Min/Max Heaps and Priority Queues",true);
            TLSPhase5VisualScreens.Heap(screenRoot,phase5Heap,()=>phase5HeapInput,x=>phase5HeapInput=x,Render,
                ()=>EnterPhase5Practice(TLSPhase5Domain.Heap),()=>EnterPhase5Functions(TLSPhase5Function.HeapInsertMin));
        }
        private void RenderPhase5Graph()
        {
            Header("Phase 5  |  Directed Graphs, BFS and DFS",true);
            TLSPhase5VisualScreens.Graph(screenRoot,phase5Graph,()=>phase5GraphA,()=>phase5GraphB,
                x=>phase5GraphA=x,x=>phase5GraphB=x,Render,
                ()=>EnterPhase5Practice(TLSPhase5Domain.Graph),()=>EnterPhase5Functions(TLSPhase5Function.GraphBfs));
        }
        private void RenderPhase5Hash()
        {
            Header("Phase 5  |  Hash Tables, Chaining and Linear Probing",true);
            TLSPhase5VisualScreens.Hash(screenRoot,phase5Hash,()=>phase5HashInput,x=>phase5HashInput=x,Render,
                ()=>EnterPhase5Practice(TLSPhase5Domain.Hash),()=>EnterPhase5Functions(TLSPhase5Function.HashInsert));
        }
        private void RenderPhase5Coding()
        {
            Header("Phase 5  |  Heap, Graph and Hash C11 Practice",true);
            TLSPhase5PracticeScreen.Render(screenRoot,phase5Practice,Render,ReturnPhase5);
        }
        private void RenderPhase5Functions()
        {
            Header("Phase 5  |  Whole-function Heap, Graph and Hash C11 / WASM",true);
            TLSPhase5WasmScreen.Render(screenRoot,phase5Functions,phase5Wasm,Render,ReturnPhase5);
        }

        private void GoPhase6(ScreenMode destination) {mode=destination;Render();}
        private void RenderPhase6Analysis()
        {
            Header("Phase 6  |  Algorithm Analysis Center — Counts, Growth & Preconditions",true);
            TLSPhase6Screens.Analysis(screenRoot,phase6Analysis,()=>phase6Prediction,x=>phase6Prediction=x,Render,
                ()=>GoPhase6(ScreenMode.Debugging),()=>GoPhase6(ScreenMode.Quality),()=>GoPhase6(ScreenMode.Phase6Functions));
        }
        private void RenderPhase6Debugging()
        {
            Header("Phase 6  |  Debugging & Testing Center — Seeded C Faults",true);
            TLSPhase6Screens.Debugging(screenRoot,phase6Debug,()=>"",x=>{},Render,
                ()=>GoPhase6(ScreenMode.Analysis),()=>GoPhase6(ScreenMode.Quality),()=>GoPhase6(ScreenMode.Phase6Functions));
        }
        private void RenderPhase6Quality()
        {
            Header("Phase 6  |  Code Quality Workshop — C11 Review",true);
            TLSPhase6Screens.Quality(screenRoot,phase6Quality,Render,
                ()=>GoPhase6(ScreenMode.Analysis),()=>GoPhase6(ScreenMode.Debugging),()=>GoPhase6(ScreenMode.Phase6Functions));
        }
        private void RenderPhase6Functions()
        {
            Header("Phase 6  |  Real C11 Search & Sort — WASM Function Tests",true);
            TLSPhase6WasmScreen.Render(screenRoot,phase6Functions,phase6Wasm,Render,
                ()=>GoPhase6(ScreenMode.Analysis));
        }

        private void LoadPhase7Evidence()
        {
            phase7Book = new TLSPhase7ProgressBook();
            if(!PlayerPrefs.HasKey(TLSPhase7ProgressBook.SaveKey)) return;
            TLSPhase7ProgressBook loaded;
            if(TLSPhase7ProgressBook.TryParse(PlayerPrefs.GetString(TLSPhase7ProgressBook.SaveKey),out loaded)) phase7Book=loaded;
            else
            {
                phase7RecordInvalid=true; // Never silently overwrite damaged or unknown-version learning data.
                Debug.LogWarning("The Living Structures: Phase 7 learning record is incompatible or unreadable. Evidence writes are disabled; previous save remains intact.");
            }
        }
        private void SavePhase7Evidence()
        {
            if(phase7RecordInvalid||phase7Book==null)return;
            PlayerPrefs.SetString(TLSPhase7ProgressBook.SaveKey,phase7Book.ExportJson());
            PlayerPrefs.Save();
        }
        private void GoPhase7(ScreenMode destination)
        {mode=destination;Render();}
        private void OpenPhase7Topic(int index)
        {
            if(index<0||index>=TLSPhase7LearningCatalog.Topics.Length)return;
            phase7Book.MarkViewed(TLSPhase7LearningCatalog.Topics[index].Id);
            SavePhase7Evidence();
            switch(index)
            {
                case 0: OpenStack();return;
                case 1: mode=ScreenMode.Arrays;break;
                case 2: mode=ScreenMode.Queues;break;
                case 3: mode=ScreenMode.PointerMemory;break;
                case 4: mode=ScreenMode.LinkedVisual;break;
                case 5: mode=ScreenMode.Recursion;break;
                case 6: case 7: mode=ScreenMode.TreeVisual;break;
                case 8: mode=ScreenMode.Heap;break;
                case 9: mode=ScreenMode.Graph;break;
                case 10: mode=ScreenMode.Hash;break;
                case 11: mode=ScreenMode.Analysis;break;
                default: mode=ScreenMode.Debugging;break;
            }
            Render();
        }
        private void RenderPhase7Menu(int selected)
        {
            TLSPhase7Screens.Menu(screenRoot,selected,()=>GoPhase7(ScreenMode.Phase7Dashboard),
                ()=>GoPhase7(ScreenMode.Phase7Exam),()=>GoPhase7(ScreenMode.Phase7Reconstruct),
                ()=>GoPhase7(ScreenMode.Phase7TestDesign),()=>GoPhase7(ScreenMode.Phase7Transfer));
        }
        private void RenderPhase7Dashboard()
        {
            Header("Phase 7  |  Learning Paths, Honest Evidence & Spaced Review",true);
            RenderPhase7Menu(0);
            TLSPhase7Screens.Dashboard(screenRoot,phase7Book,phase7TopicPick,i=>{phase7TopicPick=i;Render();},
                OpenPhase7Topic,()=>GoPhase7(ScreenMode.Phase7Exam),()=>GoPhase7(ScreenMode.Phase7Reconstruct),
                ()=>GoPhase7(ScreenMode.Phase7TestDesign),()=>GoPhase7(ScreenMode.Phase7Transfer),Render,phase7RecordInvalid);
        }
        private void RenderPhase7Exam()
        {
            Header("Phase 7  |  Diagnostic, Final Check & Mixed Spaced Review",true);
            RenderPhase7Menu(1);
            TLSPhase7Screens.ExamRoom(screenRoot,phase7Diagnostic,phase7Final,phase7Review,phase7ExamMode,
                i=>
                {
                    phase7ExamMode=i;
                    if(i==2)
                    {
                        int suggested=TLSPhase7LearningCatalog.Find(phase7Book.Recommendation(DateTime.UtcNow));
                        if(suggested>=0)phase7Review.Pick(suggested);
                    }
                },answer=>
                {
                    if(phase7ExamMode==2)phase7Review.Choose(answer,phase7Book,DateTime.UtcNow);
                    else (phase7ExamMode==0?phase7Diagnostic:phase7Final).Answer(answer,phase7Book,DateTime.UtcNow);
                    SavePhase7Evidence();Render();
                },()=>
                {
                    if(phase7ExamMode==2)phase7Review.Next();
                    else (phase7ExamMode==0?phase7Diagnostic:phase7Final).Next(phase7Book);
                    SavePhase7Evidence();Render();
                },Render,phase7Book);
        }
        private void RenderPhase7Reconstruct()
        {
            Header("Phase 7  |  Reconstruct C — Fade the Scaffolding",true);
            RenderPhase7Menu(2);
            TLSPhase7Screens.Reconstruction(screenRoot,phase7Reconstruction,phase7Order,s=>phase7Order=s,Render,
                ()=>{phase7Reconstruction.Submit(phase7Order,phase7Book,DateTime.UtcNow);SavePhase7Evidence();},SavePhase7Evidence);
        }
        private void RenderPhase7TestDesign()
        {
            Header("Phase 7  |  Write Test Inputs & Catch Real Mistakes",true);
            RenderPhase7Menu(3);
            TLSPhase7Screens.TestDesign(screenRoot,phase7TestDesign,phase7TestCsv,phase7TestArg,phase7TestExpected,
                s=>phase7TestCsv=s,s=>phase7TestArg=s,s=>phase7TestExpected=s,Render,()=>
                {
                    int arg,expected;
                    if(int.TryParse(phase7TestArg,out arg)&&int.TryParse(phase7TestExpected,out expected))
                    {
                        phase7TestDesign.Submit(phase7TestCsv,arg,expected,phase7Book,DateTime.UtcNow);
                        SavePhase7Evidence();
                    }
                    else phase7TestDesign.InputError();
                });
        }
        private void RenderPhase7Transfer()
        {
            Header("Phase 7  |  Independent Whole-Function C11 Transfer",true);
            RenderPhase7Menu(4);
            TLSPhase7Screens.Transfer(screenRoot,phase7Functions,phase7Wasm,phase7Book,Render,
                ()=>GoPhase7(ScreenMode.Phase7Dashboard));
        }

        private void OpenStack()
        {
            mode = ScreenMode.Stack;
            Render();
        }

        private string ProgressLabel()
        {
            return "Stack concept: AVAILABLE\n\n" +
                "Trace practice: " + (StudyProgress.TracePracticeComplete ? "COMPLETED" : "NOT COMPLETED") + "\n\n" +
                "First-attempt trace pass: " + (StudyProgress.IndependentTracePass ? "RECORDED" : "NOT RECORDED");
        }

        private void Rotate(float change)
        {
            yawOffset += change;
            if (academyCamera != null)
            {
                Vector3 center = new Vector3(0, 1.3f, 1f);
                Quaternion yaw = Quaternion.Euler(0, yawOffset, 0);
                academyCamera.transform.position = center + yaw * new Vector3(0, 11f, -22f);
                academyCamera.transform.LookAt(center);
            }
            Render();
        }

        private void Begin(StackOperation operation)
        {
            if (!CanStart() || predictionPending) return;
            int input = 0;
            if (operation == StackOperation.Push && !int.TryParse(valueToPush, out input))
            {
                feedback = "Enter a valid 32-bit signed integer (-2147483648 to 2147483647).";
                Render();
                return;
            }
            predictionOperation = operation;
            predictionInput = input;
            predictionPending = true;
            // Prediction is recorded before any new trace is shown. It is formative, not mastery evidence.
            Render();
        }

        private void AnswerPrediction(StackCountPrediction? choice)
        {
            if (!predictionPending) return;
            predictionChoice = choice;
            predictionPending = false;
            predictionReview = choice.HasValue ? "Prediction locked. Step through to check it." :
                "Prediction skipped. Step through the operation to learn what happened.";
            activeTrace = StackSimulation.Execute(current, predictionOperation, predictionInput);
            frameIndex = 0;
            feedback = "Step through the C function and inspect exactly what changes.";
            Render();
        }

        private void CancelPrediction()
        {
            predictionPending = false;
            Render();
        }

        private bool CanStart() { return activeTrace == null || frameIndex == activeTrace.Frames.Count - 1; }

        private void Next()
        {
            if (activeTrace == null) return;
            frameIndex = Math.Min(activeTrace.Frames.Count - 1, frameIndex + 1);
            CommitIfAtEnd();
            Render();
        }

        private void Previous()
        {
            if (activeTrace == null) return;
            frameIndex = Math.Max(0, frameIndex - 1);
            Render();
        }

        private void Finish()
        {
            if (activeTrace == null) return;
            frameIndex = activeTrace.Frames.Count - 1;
            CommitIfAtEnd();
            Render();
        }

        private void CommitIfAtEnd()
        {
            if (activeTrace != null && frameIndex == activeTrace.Frames.Count - 1)
            {
                current = activeTrace.FinalState.Clone();
                feedback = activeTrace.Succeeded ? "Operation completed. Start another or replay the trace." :
                    "Operation rejected safely; the stack state did not change.";
                if (predictionChoice.HasValue)
                    predictionReview = StackPredictionFeedback.Explain(activeTrace, predictionChoice.Value);
                else
                    predictionReview = "No prediction submitted. This run does not count as independent assessment.";
            }
        }

        private void ResetStack()
        {
            current = new StackState();
            activeTrace = null;
            frameIndex = 0;
            predictionPending = false;
            predictionChoice = null;
            predictionReview = "";
            feedback = "Stack reset to empty. Trace practice progress was not erased.";
            Render();
        }

        private void RenderStack()
        {
            Header("Stack Laboratory  |  C11 array-backed stack  |  Phase 1.4", true);
            TLSUiFactory.Panel(screenRoot, "Backdrop", V(0, 0), V(1, 0.907f), TLSUiFactory.Navy);
            TLSUiFactory.MakeButton(screenRoot, "WRITE C FUNCTIONS", V(0.657f, 0.925f), V(0.782f, 0.972f), () =>
            {
                mode = ScreenMode.FunctionLab;
                Render();
            }, TLSUiFactory.Deep, 13);
            StackOperation operation = activeTrace != null ? activeTrace.Operation : StackOperation.Push;
            string[] snippet = StackSimulation.CodeFor(operation);
            int highlighted = activeTrace == null ? -1 : activeTrace.Frames[frameIndex].HighlightLine;
            StackState visibleState = activeTrace == null ? current : activeTrace.Frames[frameIndex].State;
            StackStepDelta delta = activeTrace == null ? null : StackStepDelta.At(activeTrace, frameIndex);
            int focus = delta == null ? -1 : delta.FocusCell;

            RectTransform code = TLSUiFactory.Panel(screenRoot, "C source", V(0.016f, 0.249f), V(0.405f, 0.888f), TLSUiFactory.Slate);
            TLSUiFactory.Label(code, "Code Header", "C CODE  /  EXECUTION LINE", V(0.038f, 0.893f), V(0.975f, 0.98f),
                21, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            for (int i = 0; i < snippet.Length; i++)
            {
                float yMax = 0.852f - i * 0.080f;
                float yMin = yMax - 0.074f;
                if (highlighted == i)
                {
                    TLSUiFactory.Panel(code, "Active C line", V(0.026f, yMin), V(0.98f, yMax), TLSUiFactory.Deep);
                    TLSUiFactory.Panel(code, "C line marker", V(0.026f, yMin), V(0.033f, yMax), TLSUiFactory.Cyan);
                }
                string line = (i + 1).ToString() + "   " + CCodeDisplay.HighlightTokens(snippet[i]);
                Text lineLabel = TLSUiFactory.Label(code, "C line " + (i + 1), line,
                    V(0.045f, yMin), V(0.977f, yMax), snippet[i].Length > 63 ? 16 : 19,
                    TLSUiFactory.Pale, TextAnchor.MiddleLeft);
                // Unity UI Text rich text is used only for syntax emphasis of fixed shipped snippets.
                lineLabel.supportRichText = true;
                lineLabel.resizeTextForBestFit = true;
                lineLabel.resizeTextMinSize = 15;
                lineLabel.resizeTextMaxSize = snippet[i].Length > 63 ? 17 : 19;
            }
            TLSUiFactory.Panel(code, "Info rule", V(0.035f, 0.245f), V(0.965f, 0.25f), TLSUiFactory.Deep);
            TLSUiFactory.Label(code, "Source", "C11 reference: Assets/TheLivingStructures/Examples/stack.c",
                V(0.045f, 0.165f), V(0.97f, 0.24f), 16, TLSUiFactory.Muted);
            TLSUiFactory.Label(code, "Simulation note", "Modeled C execution, not arbitrary student C compilation. Each step uses an authoritative state snapshot.",
                V(0.045f, 0.025f), V(0.97f, 0.165f), 16, TLSUiFactory.Muted);

            RectTransform visual = TLSUiFactory.Panel(screenRoot, "Visual stack", V(0.414f, 0.249f), V(0.754f, 0.888f), TLSUiFactory.Deep);
            TLSUiFactory.Label(visual, "Title", "STACK / LOGICAL ELEMENTS", V(0.04f, 0.89f), V(0.97f, 0.975f),
                20, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(visual, "Stack note", "New elements enter at the top. Index 0 is the base.",
                V(0.04f, 0.81f), V(0.97f, 0.88f), 16, TLSUiFactory.Muted);
            for (int i = 0; i < StackState.Capacity; i++)
            {
                float bottom = 0.158f + i * 0.127f;
                bool active = visibleState.IsActive(i);
                bool rawChanged = delta != null && delta.WroteRawCell && delta.ChangedCell == i;
                bool countChanged = delta != null && delta.CountDelta != 0 && focus == i;
                Color bg = rawChanged ? new Color(0.37f, 0.27f, 0.12f, 1) :
                    countChanged ? new Color(0.24f, 0.28f, 0.43f, 1) :
                    i == focus ? new Color(0.25f, 0.26f, 0.31f, 1) :
                    active ? TLSUiFactory.Slate : TLSUiFactory.Navy;
                RectTransform cell = TLSUiFactory.Panel(visual, "Cell " + i, V(0.1f, bottom), V(0.9f, bottom + 0.113f), bg);
                TLSUiFactory.Label(cell, "Index", "[" + i + "]", V(0.025f, 0.06f), V(0.18f, 0.94f),
                    17, TLSUiFactory.Muted);
                string value = active ? visibleState.RawAt(i).ToString() : rawChanged ?
                    visibleState.RawAt(i) + " (stored, inactive)" : "(inactive)";
                TLSUiFactory.Label(cell, "Value", value, V(0.20f, 0.05f), V(0.955f, 0.95f),
                    active ? 24 : rawChanged ? 17 : 16,
                    rawChanged ? TLSUiFactory.Amber : active ? TLSUiFactory.Pale : TLSUiFactory.Muted,
                    TextAnchor.MiddleCenter, active || rawChanged);
            }
            TLSUiFactory.Label(visual, "Top", visibleState.Count == 0 ? "TOP: none (empty)" :
                "TOP: data[" + (visibleState.Count - 1) + "] = " + visibleState.RawAt(visibleState.Count - 1),
                V(0.055f, 0.035f), V(0.945f, 0.12f), 20, TLSUiFactory.Amber, TextAnchor.MiddleCenter, true);

            RectTransform inspector = TLSUiFactory.Panel(screenRoot, "Memory and reasoning", V(0.763f, 0.249f), V(0.984f, 0.888f), TLSUiFactory.Slate);
            TLSUiFactory.Label(inspector, "Title", "MEMORY / WHY", V(0.06f, 0.895f), V(0.95f, 0.975f),
                20, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(inspector, "Count", "count = " + visibleState.Count + " / " + StackState.Capacity,
                V(0.06f, 0.803f), V(0.95f, 0.885f), 21,
                delta != null && delta.CountDelta != 0 ? TLSUiFactory.Amber : TLSUiFactory.Pale,
                TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(inspector, "RawHeader", "RAW BACKING ARRAY", V(0.06f, 0.725f), V(0.95f, 0.786f),
                17, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            for (int i = 0; i < StackState.Capacity; i++)
            {
                bool justWrote = delta != null && delta.ChangedCell == i;
                if (justWrote)
                    TLSUiFactory.Panel(inspector, "Raw write focus", V(0.047f, 0.651f - i * 0.076f),
                        V(0.96f, 0.718f - i * 0.076f), TLSUiFactory.Deep);
                TLSUiFactory.Label(inspector, "RawCell" + i,
                    "data[" + i + "] = " + visibleState.RawAt(i) + (visibleState.IsActive(i) ? "  active" : "  inactive"),
                    V(0.06f, 0.65f - i * 0.076f), V(0.96f, 0.716f - i * 0.076f),
                    16, justWrote ? TLSUiFactory.Amber : visibleState.IsActive(i) ? TLSUiFactory.Pale : TLSUiFactory.Muted);
            }
            TLSUiFactory.Label(inspector, "Why header", "WHY THIS MATTERS", V(0.06f, 0.245f), V(0.95f, 0.325f),
                17, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(inspector, "Why explanation",
                delta == null ? "Only cells below count belong to the logical stack. Inactive backing slots may retain old values." :
                    delta.WhyThisMatters(activeTrace, frameIndex),
                V(0.06f, 0.025f), V(0.95f, 0.25f), 16, TLSUiFactory.Muted);

            RectTransform changeBar = TLSUiFactory.Panel(screenRoot, "State change strip", V(0.018f, 0.212f), V(0.984f, 0.244f), TLSUiFactory.Deep);
            TLSUiFactory.Label(changeBar, "Delta", delta == null ? "STATE CHANGE: choose an operation, predict its effect, then step through C." :
                delta.ChangeLabel(), V(0.015f, 0.07f), V(0.98f, 0.92f), 17,
                delta != null && (delta.WroteRawCell || delta.CountDelta != 0) ? TLSUiFactory.Amber : TLSUiFactory.Muted);

            RectTransform controls = TLSUiFactory.Panel(screenRoot, "Controls", V(0.018f, 0.012f), V(0.984f, 0.205f), TLSUiFactory.Slate);
            TLSUiFactory.Label(controls, "Explain", activeTrace == null ? feedback :
                "Step " + (frameIndex + 1) + "/" + activeTrace.Frames.Count + ": " + activeTrace.Frames[frameIndex].Explanation,
                V(0.02f, 0.68f), V(0.98f, 0.96f), 17, TLSUiFactory.Pale);
            TLSUiFactory.Label(controls, "Prediction feedback", predictionReview,
                V(0.02f, 0.48f), V(0.98f, 0.69f), 15,
                predictionReview.StartsWith("PREDICTION CORRECT.") ? TLSUiFactory.Green : TLSUiFactory.Amber);
            TLSUiFactory.Field(controls, "Value to push", valueToPush, "integer", V(0.015f, 0.08f), V(0.13f, 0.46f),
                t => valueToPush = t);
            bool allow = CanStart() && !predictionPending;
            TLSUiFactory.MakeButton(controls, "PUSH", V(0.138f, 0.08f), V(0.213f, 0.46f),
                () => Begin(StackOperation.Push), TLSUiFactory.Deep, 17, allow);
            TLSUiFactory.MakeButton(controls, "POP", V(0.221f, 0.08f), V(0.286f, 0.46f),
                () => Begin(StackOperation.Pop), TLSUiFactory.Deep, 17, allow);
            TLSUiFactory.MakeButton(controls, "PEEK", V(0.294f, 0.08f), V(0.363f, 0.46f),
                () => Begin(StackOperation.Peek), TLSUiFactory.Deep, 17, allow);
            TLSUiFactory.MakeButton(controls, "< STEP", V(0.375f, 0.08f), V(0.463f, 0.46f),
                Previous, TLSUiFactory.Deep, 16, activeTrace != null && frameIndex > 0);
            TLSUiFactory.MakeButton(controls, "STEP >", V(0.472f, 0.08f), V(0.556f, 0.46f),
                Next, TLSUiFactory.Deep, 16, activeTrace != null && frameIndex < activeTrace.Frames.Count - 1);
            TLSUiFactory.MakeButton(controls, "FINISH", V(0.567f, 0.08f), V(0.65f, 0.46f),
                Finish, TLSUiFactory.Deep, 16, activeTrace != null);
            TLSUiFactory.MakeButton(controls, "RESET", V(0.66f, 0.08f), V(0.742f, 0.46f),
                ResetStack, TLSUiFactory.Deep, 16);
            TLSUiFactory.MakeButton(controls, "TRACE CHALLENGE", V(0.752f, 0.08f), V(0.859f, 0.46f), () =>
            {
                mode = ScreenMode.Challenge;
                Render();
            }, TLSUiFactory.Violet, 13);
            TLSUiFactory.MakeButton(controls, "C PRACTICE", V(0.869f, 0.08f), V(0.986f, 0.46f), () =>
            {
                mode = ScreenMode.CodePractice;
                Render();
            }, TLSUiFactory.Deep, 16);

            if (predictionPending) RenderPredictionPrompt();
        }

        private void RenderPredictionPrompt()
        {
            TLSUiFactory.Panel(screenRoot, "Prediction scrim", V(0, 0), V(1, 0.907f),
                new Color(0.02f, 0.04f, 0.08f, 0.89f));
            RectTransform prompt = TLSUiFactory.Panel(screenRoot, "Predict before running", V(0.278f, 0.15f), V(0.725f, 0.84f),
                TLSUiFactory.Slate);
            TLSUiFactory.Label(prompt, "Title", "PREDICT BEFORE YOU EXECUTE", V(0.055f, 0.85f), V(0.95f, 0.95f),
                26, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            string name = predictionOperation.ToString().ToUpperInvariant();
            string currentTop = current.Count == 0 ? "none (empty)" : current.RawAt(current.Count - 1).ToString();
            TLSUiFactory.Label(prompt, "Current state", "Operation: " + name +
                (predictionOperation == StackOperation.Push ? " " + predictionInput : "") +
                "   |   count = " + current.Count + "/5   |   top = " + currentTop,
                V(0.055f, 0.738f), V(0.95f, 0.85f), 19, TLSUiFactory.Pale);
            TLSUiFactory.Label(prompt, "Question", "Before seeing the new trace, what happens to count?",
                V(0.055f, 0.618f), V(0.95f, 0.74f), 21, TLSUiFactory.Pale, TextAnchor.MiddleLeft, true);
            TLSUiFactory.MakeButton(prompt, "INCREASES BY 1", V(0.065f, 0.505f), V(0.935f, 0.603f),
                () => AnswerPrediction(StackCountPrediction.Increases), TLSUiFactory.Deep, 19);
            TLSUiFactory.MakeButton(prompt, "DECREASES BY 1", V(0.065f, 0.39f), V(0.935f, 0.488f),
                () => AnswerPrediction(StackCountPrediction.Decreases), TLSUiFactory.Deep, 19);
            TLSUiFactory.MakeButton(prompt, "STAYS THE SAME", V(0.065f, 0.275f), V(0.935f, 0.373f),
                () => AnswerPrediction(StackCountPrediction.Unchanged), TLSUiFactory.Deep, 19);
            TLSUiFactory.Label(prompt, "Disclosure", "Your choice is formative practice only. Independent tracing results are tracked separately.",
                V(0.065f, 0.17f), V(0.94f, 0.267f), 16, TLSUiFactory.Muted);
            TLSUiFactory.MakeButton(prompt, "SKIP", V(0.065f, 0.05f), V(0.48f, 0.15f),
                () => AnswerPrediction(null), TLSUiFactory.Deep, 17);
            TLSUiFactory.MakeButton(prompt, "CANCEL", V(0.52f, 0.05f), V(0.935f, 0.15f),
                CancelPrediction, TLSUiFactory.Deep, 17);
        }

        private void RenderCodePractice()
        {
            Header("Stack Laboratory  |  C Reading, Completion and Debugging  |  Phase 1.3", true);
            TLSCodePracticeScreen.Render(screenRoot, codePractice, () =>
            {
                mode = ScreenMode.Stack;
                Render();
            }, Render);
        }

        private void RenderArraysLab()
        {
            Header("Arrays  |  C11 fixed-capacity insertion, shifting and reads  |  Phase 2.3", true);
            TLSLinearLabsScreen.RenderArray(screenRoot, arraysPractice, Render);
            TLSUiFactory.MakeButton(screenRoot, "C PRACTICE", V(0.544f, 0.925f), V(0.653f, 0.975f),
                () => { mode = ScreenMode.ArrayCoding; Render(); }, TLSUiFactory.Deep, 14);
            TLSUiFactory.MakeButton(screenRoot, "WRITE C FUNCTIONS", V(0.671f, 0.925f), V(0.785f, 0.975f),
                () => { mode = ScreenMode.ArrayFunctions; Render(); }, TLSUiFactory.Deep, 12);
        }

        private void RenderQueuesLab()
        {
            Header("Queues  |  C11 circular buffer, FIFO and wraparound  |  Phase 2.3", true);
            TLSLinearLabsScreen.RenderQueue(screenRoot, queuesPractice, Render);
            TLSUiFactory.MakeButton(screenRoot, "C PRACTICE", V(0.544f, 0.925f), V(0.653f, 0.975f),
                () => { mode = ScreenMode.QueueCoding; Render(); }, TLSUiFactory.Deep, 14);
            TLSUiFactory.MakeButton(screenRoot, "WRITE C FUNCTIONS", V(0.671f, 0.925f), V(0.785f, 0.975f),
                () => { mode = ScreenMode.QueueFunctions; Render(); }, TLSUiFactory.Deep, 12);
        }

        private void RenderLinearCoding(bool array)
        {
            Header(array ? "Array Lab  |  C11 Trace, Complete, Repair  |  Phase 2.2" :
                "Queue Lab  |  C11 Trace, Complete, Repair  |  Phase 2.2", true);
            TLSLinearPracticeScreen.Render(screenRoot, array ? arraysCoding : queuesCoding,
                () => { mode = array ? ScreenMode.Arrays : ScreenMode.Queues; Render(); }, Render);
        }

        private void RenderLinearFunctions(bool array)
        {
            Header(array ? "Array Lab  |  Whole-Function C11 WASM Testing  |  Phase 2.3" :
                "Queue Lab  |  Whole-Function C11 WASM Testing  |  Phase 2.3", true);
            TLSLinearFunctionScreen.Render(screenRoot, array ? arrayFunctions : queueFunctions, linearWasm,
                () => {mode = array ? ScreenMode.Arrays : ScreenMode.Queues; Render();}, Render);
        }

        private void RenderFunctionLab()
        {
            Header("Stack Laboratory  |  Complete C Functions  |  Phase 1.4 + WASM testing", true);
            TLSFunctionLabScreen.Render(screenRoot, functionPractice, () =>
            {
                mode = ScreenMode.Stack;
                Render();
            }, Render, () =>
            {
                mode = ScreenMode.WasmCompilerLab;
                Render();
            });
        }

        private void RenderWasmCompilerLab()
        {
            Header("Stack Lab  |  Optional C11 WebAssembly Testing  |  Phase 1.5", true);
            TLSWasmCompilerLabScreen.Render(screenRoot, functionPractice, wasmPractice, () =>
            {
                mode=ScreenMode.FunctionLab;
                Render();
            }, Render);
        }

        private void Update()
        {
            if (mode == ScreenMode.Academy && !TLSPhase8Campus.PointerIsOverUI())
            {
                // The original Academy uses legacy input. New Input-System-only
                // projects can always select buildings with the uGUI inspector.
                try
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        int picked;
                        if (TLSPhase8Campus.TrySelect(academyCamera,Input.mousePosition,out picked))
                        { phase8SelectedHub=picked; Render(); }
                    }
                }
                catch (InvalidOperationException) { /* Disabled legacy Input is not an application error. */ }
            }
            bool stackFinished = wasmPractice.Poll();
            bool linearFinished = linearWasm.Poll();
            bool linkedFinished = linkedWasm.Poll();
            bool phase4Finished = phase4Wasm.Poll();
            bool phase5Finished = phase5Wasm.Poll();
            bool phase6Finished = phase6Wasm.Poll();
            bool phase7Finished = phase7Wasm.Poll();
            if(phase7Finished&&phase7Wasm.IndependentJustRecorded)SavePhase7Evidence();
            if ((stackFinished && mode == ScreenMode.WasmCompilerLab) ||
                (linearFinished && (mode == ScreenMode.ArrayFunctions || mode == ScreenMode.QueueFunctions)) ||
                (linkedFinished && mode == ScreenMode.LinkedFunctions) ||
                (phase4Finished && mode == ScreenMode.TreeFunctions) ||
                (phase5Finished && mode == ScreenMode.Phase5Functions) ||
                (phase6Finished && mode == ScreenMode.Phase6Functions) ||
                (phase7Finished && mode == ScreenMode.Phase7Transfer))
                Render();
        }
        private void OnDestroy()
        {
            wasmPractice.Cancel();
            linearWasm.Cancel();
            linkedWasm.Cancel();
            phase4Wasm.Cancel();
            phase5Wasm.Cancel();
            phase6Wasm.Cancel();
            phase7Wasm.Cancel();
        }

        private void RenderChallenge()
        {
            Header("Stack Lab  |  Independent Tracing Practice", true);
            TLSUiFactory.Panel(screenRoot, "Background", V(0, 0), V(1, 0.907f), TLSUiFactory.Navy);
            RectTransform sheet = TLSUiFactory.Panel(screenRoot, "Question sheet", V(0.15f, 0.12f), V(0.85f, 0.87f), TLSUiFactory.Slate);
            TLSUiFactory.Label(sheet, "Heading", "INDEPENDENT STACK TRACING", V(0.055f, 0.84f), V(0.95f, 0.96f),
                28, TLSUiFactory.Cyan, TextAnchor.MiddleLeft, true);
            TLSUiFactory.Label(sheet, "Help", "Without using the visual stack, predict the final TOP value and COUNT after executing these C operations. Each question starts with an empty stack.",
                V(0.055f, 0.68f), V(0.95f, 0.83f), 21, TLSUiFactory.Pale);
            if (challengeFinished)
            {
                TLSUiFactory.Label(sheet, "Completed", "THREE QUESTIONS COMPLETED\n\n" +
                    (challengeMistakes == 0 ? "All first attempts were correct: independent trace pass recorded." :
                    "Practice completed with corrections; no first-attempt independent pass recorded for this run.") +
                    "\n\nThis is TRACE practice only. It does not certify independent C implementation mastery.",
                    V(0.055f, 0.27f), V(0.95f, 0.67f), 22, TLSUiFactory.Green);
                TLSUiFactory.MakeButton(sheet, "RESTART QUESTIONS", V(0.055f, 0.12f), V(0.44f, 0.22f), () =>
                {
                    challengeIndex = 0;
                    challengeMistakes = 0;
                    challengeFinished = false;
                    challengeFeedback = "No visualization is provided during these independent tracing questions.";
                    Render();
                });
            }
            else
            {
                StackChallenge question = StackChallenge.Get(challengeIndex);
                TLSUiFactory.Label(sheet, "Index", "QUESTION " + (challengeIndex + 1) + " OF " + StackChallenge.Total,
                    V(0.055f, 0.60f), V(0.95f, 0.67f), 20, TLSUiFactory.Violet, TextAnchor.MiddleLeft, true);
                TLSUiFactory.Label(sheet, "Question", question.Description, V(0.055f, 0.45f), V(0.95f, 0.59f),
                    23, TLSUiFactory.Pale);
                TLSUiFactory.Label(sheet, "Top label", "Final TOP value", V(0.055f, 0.365f), V(0.39f, 0.43f), 18);
                TLSUiFactory.Label(sheet, "Count label", "Final COUNT", V(0.43f, 0.365f), V(0.77f, 0.43f), 18);
                TLSUiFactory.Field(sheet, "Top answer", topAnswer, "top integer", V(0.055f, 0.27f), V(0.39f, 0.355f), s => topAnswer = s);
                TLSUiFactory.Field(sheet, "Count answer", countAnswer, "count", V(0.43f, 0.27f), V(0.77f, 0.355f), s => countAnswer = s);
                TLSUiFactory.Label(sheet, "Feedback", challengeFeedback, V(0.055f, 0.15f), V(0.95f, 0.25f), 17,
                    TLSUiFactory.Amber);
                TLSUiFactory.MakeButton(sheet, "CHECK ANSWER", V(0.055f, 0.045f), V(0.40f, 0.135f), CheckChallenge,
                    TLSUiFactory.Deep, 18);
            }
            TLSUiFactory.MakeButton(sheet, "RETURN TO STACK LAB", V(0.59f, 0.045f), V(0.945f, 0.135f), () =>
            {
                mode = ScreenMode.Stack;
                Render();
            }, TLSUiFactory.Deep, 16);
        }

        private void CheckChallenge()
        {
            int top, count;
            if (!int.TryParse(topAnswer, out top) || !int.TryParse(countAnswer, out count))
            {
                challengeFeedback = "Enter whole-number values for top and count.";
                Render();
                return;
            }
            bool correct = StackChallenge.Get(challengeIndex).Check(top, count);
            if (!correct)
            {
                challengeMistakes++;
                challengeFeedback = "Not quite. Reconsider LIFO and which operations actually change count. You can retry.";
            }
            else
            {
                challengeIndex++;
                topAnswer = "";
                countAnswer = "";
                challengeFeedback = "Correct. Continue without visual hints.";
                if (challengeIndex == StackChallenge.Total)
                {
                    challengeFinished = true;
                    StudyProgress.RecordTracePractice(challengeMistakes == 0);
                }
            }
            Render();
        }
    }
}
