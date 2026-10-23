using System;

namespace TLS.Core
{
    public enum TLSLinearCodingHelp { Guided, Independent }

    /// <summary>
    /// Seven separate editable C11 drafts and session-only assessment exposure.
    /// No grading, Unity progress writes, or save-state changes occur here.
    /// </summary>
    public sealed class TLSLinearFunctionLabSession
    {
        private readonly string[] drafts = new string[TLSLinearFunctionCatalog.Count];
        private readonly bool[] hintUsed = new bool[TLSLinearFunctionCatalog.Count];
        private readonly bool[] referenceUsed = new bool[TLSLinearFunctionCatalog.Count];
        private readonly bool[] firstSubmitted = new bool[TLSLinearFunctionCatalog.Count];
        private readonly bool[] firstIndependentPass = new bool[TLSLinearFunctionCatalog.Count];
        private readonly bool[] modeledCompilerPass = new bool[TLSLinearFunctionCatalog.Count];
        public readonly bool ArrayDomain;
        public TLSLinearCodingHelp Help { get; private set; }
        public TLSLinearFunction Selected { get; private set; }
        public bool HintVisible { get; private set; }
        public bool ReferenceVisible { get; private set; }
        public int FirstIndependentPassCount { get { return CountTrue(firstIndependentPass); } }
        public int CompiledPassCount { get { return CountTrue(modeledCompilerPass); } }
        public int DomainTotal { get { return ArrayDomain ? 4 : 3; } }

        public TLSLinearFunctionLabSession(bool array)
        {
            ArrayDomain = array;
            Selected = array ? TLSLinearFunction.ArrayAppend : TLSLinearFunction.QueueEnqueue;
            Help = TLSLinearCodingHelp.Guided;
            for (int i = 0; i < drafts.Length; i++) drafts[i] = TLSLinearFunctionCatalog.Starter((TLSLinearFunction)i);
        }
        private int CountTrue(bool[] values)
        {
            int n = 0;
            int start = ArrayDomain ? 0 : 4, stop = ArrayDomain ? 4 : 7;
            for (int i = start; i < stop; i++) if (values[i]) n++;
            return n;
        }
        public string Draft
        {
            get { return drafts[(int)Selected]; }
            set { drafts[(int)Selected] = value ?? ""; }
        }
        public void Select(TLSLinearFunction function)
        {
            if (!TLSLinearFunctionCatalog.IsValid(function) || TLSLinearFunctionCatalog.IsArray(function) != ArrayDomain)
                throw new ArgumentOutOfRangeException("function");
            Selected = function;
            HintVisible = false;
            ReferenceVisible = false;
        }
        public void SetHelp(TLSLinearCodingHelp choice)
        {
            if (choice != TLSLinearCodingHelp.Guided && choice != TLSLinearCodingHelp.Independent)
                throw new ArgumentOutOfRangeException("choice");
            Help = choice;
            HintVisible = false;
            ReferenceVisible = false;
        }
        public void ShowHint()
        {
            if (Help != TLSLinearCodingHelp.Guided) return;
            hintUsed[(int)Selected] = true;
            HintVisible = true;
        }
        public void RevealReference()
        {
            referenceUsed[(int)Selected] = true;
            Draft = TLSLinearFunctionCatalog.Reference(Selected);
            ReferenceVisible = true;
            HintVisible = false;
        }
        public void RestoreStarter()
        {
            Draft = TLSLinearFunctionCatalog.Starter(Selected);
            HintVisible = false;
            ReferenceVisible = false;
            // Never erase previous hint exposure, reference exposure or previous submissions.
        }
        public bool HasExposure(TLSLinearFunction f) { return hintUsed[(int)f] || referenceUsed[(int)f]; }
        public bool WasAttempted(TLSLinearFunction f) { return firstSubmitted[(int)f]; }
        public bool FirstIndependentPassed(TLSLinearFunction f) { return firstIndependentPass[(int)f]; }
        public void RecordCompilerResult(TLSLinearFunction function, bool evaluated, bool allPassed, bool wasIndependent)
        {
            if (!TLSLinearFunctionCatalog.IsValid(function) || TLSLinearFunctionCatalog.IsArray(function) != ArrayDomain)
                throw new ArgumentOutOfRangeException("function");
            // Missing tools, runtime cancellation/timeouts, etc. cannot burn a first attempt.
            if (!evaluated) return;
            int i = (int)function;
            if (!firstSubmitted[i] && wasIndependent && !HasExposure(function) && allPassed)
                firstIndependentPass[i] = true;
            firstSubmitted[i] = true;
            if (allPassed) modeledCompilerPass[i] = true;
        }
    }
}
