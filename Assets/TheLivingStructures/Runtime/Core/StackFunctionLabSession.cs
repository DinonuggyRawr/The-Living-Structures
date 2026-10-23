using System;

namespace TLS.Core
{
    /// <summary>
    /// Session-only practice drafts and results. Deliberately never writes saved learning-progress keys.
    /// A passing bounded-subset result is not evidence of C compiler-backed mastery.
    /// </summary>
    public sealed class StackFunctionLabSession
    {
        private sealed class Attempt
        {
            public string Draft;
            public bool Hinted, Revealed, Submitted, Passed, FirstAttempt;
            public StackFunctionEvaluator.Result Latest;
        }
        private readonly Attempt[] attempts = new Attempt[3];
        public StackFunctionEvaluator.Function Selected { get; private set; }
        public bool Guided { get; private set; } = true;
        public bool HintShowing { get; private set; }
        public string Status { get; private set; } = "Write a complete function, then run the modeled tests. No native C compilation occurs.";
        public StackFunctionLabSession()
        {
            for(int i=0;i<attempts.Length;i++) attempts[i]=new Attempt {Draft=StackFunctionEvaluator.Starter((StackFunctionEvaluator.Function)i)};
        }
        private Attempt Current {get {return attempts[(int)Selected];}}
        public string Draft {get {return Current.Draft;} set {Current.Draft=value??"";}}
        public bool Revealed {get {return Current.Revealed;}}
        public bool Passed {get {return Current.Passed;}}
        public bool FirstAttempt {get {return Current.FirstAttempt;}}
        public StackFunctionEvaluator.Result Latest {get {return Current.Latest;}}
        public int PassedCount {get {int n=0;foreach(Attempt a in attempts)if(a.Passed)n++;return n;}}
        public int IndependentFirstAttemptCount {get {int n=0;foreach(Attempt a in attempts)if(a.FirstAttempt)n++;return n;}}
        public void Select(StackFunctionEvaluator.Function selected)
        {
            if((int)selected<0||(int)selected>2) throw new ArgumentOutOfRangeException(nameof(selected));
            Selected=selected;HintShowing=false;
            Status="Selected "+selected.ToString().ToUpperInvariant()+". Draft kept locally for this session.";
        }
        public void SetGuided(bool guided)
        { Guided=guided;HintShowing=false; Status=guided?"Guided hints available; assistance prevents first-attempt credit.":"Independent practice selected; hints are hidden."; }
        public void ShowHint()
        {
            if(!Guided) {Status="Switch to Guided if you want a hint.";return;}
            Current.Hinted=true; HintShowing=true;Status=StackFunctionEvaluator.Hint(Selected);
        }
        public void Reveal()
        {
            Current.Revealed=true;Current.Hinted=true;HintShowing=false;
            Current.Draft=StackFunctionEvaluator.Reference(Selected);
            Status="Reference inserted for review. This function cannot earn unseen first-attempt credit in this session.";
        }
        public void ResetDraft()
        {
            Current.Draft=StackFunctionEvaluator.Starter(Selected);Current.Latest=null;
            Status="Starter restored. Prior attempts and assistance flags remain recorded for this session.";
        }
        public StackFunctionEvaluator.Result Run()
        {
            bool clean=!Guided&&!Current.Submitted&&!Current.Hinted&&!Current.Revealed;
            Current.Submitted=true;
            Current.Latest=StackFunctionEvaluator.Check(Selected,Current.Draft);
            if(Current.Latest.Accepted)
            {
                Current.Passed=true;
                if(clean) Current.FirstAttempt=true;
            }
            Status=Current.Latest.Message;
            return Current.Latest;
        }
    }
}
