using System;
using System.Collections.Generic;

namespace TLS.Core
{
    public enum TLSRecursionExample { SumToN, Factorial, Fibonacci }
    public sealed class TLSRecursionFrame
    {
        public readonly string Calls, Explanation, Result;
        public readonly int Line, Depth;
        public TLSRecursionFrame(string calls, int line, int depth, string explanation, string result)
        { Calls = calls; Line = line; Depth = depth; Explanation = explanation; Result = result; }
    }
    /// <summary>Bounded deterministic C11 algorithm trace; it is not an interpreter or process call stack.</summary>
    public sealed class TLSRecursionLab
    {
        public TLSRecursionExample Example {get;private set;}=TLSRecursionExample.SumToN;
        public int Argument {get;private set;}=4;
        public readonly List<TLSRecursionFrame> Frames=new List<TLSRecursionFrame>();
        private readonly List<string> active=new List<string>();
        public int Step {get;private set;}
        public TLSRecursionFrame Current {get{return Frames[Step];}}
        public bool PredictionAnswered {get;private set;}
        public string PredictionFeedback {get;private set;}="Predict the return value before revealing the trace.";
        public static string[] Code(TLSRecursionExample e)
        {
            switch(e)
            {
                case TLSRecursionExample.SumToN: return new[]{"int sum(int n) {","    if (n == 0) return 0;","    int rest = sum(n - 1);","    return n + rest;","}"};
                case TLSRecursionExample.Factorial: return new[]{"int factorial(int n) {","    if (n == 0) return 1;","    int rest = factorial(n - 1);","    return n * rest;","}"};
                default: return new[]{"int fib(int n) {","    if (n < 2) return n;","    int left = fib(n - 1);","    int right = fib(n - 2);","    return left + right;","}"};
            }
        }
        private void Record(int line,string explanation,string result="")
        {Frames.Add(new TLSRecursionFrame(string.Join("\n",active.ToArray()),line,active.Count,explanation,result));}
        private int Eval(int n)
        {
            if(Frames.Count>320)throw new InvalidOperationException("Trace limit exceeded.");
            string name=Example==TLSRecursionExample.SumToN?"sum":Example==TLSRecursionExample.Factorial?"factorial":"fib";
            active.Add(name+"("+n+")");
            Record(0,"CALL "+name+"("+n+") — a new C stack frame has its own n.");
            if(n==0 || Example==TLSRecursionExample.Fibonacci && n==1)
            {
                int baseResult=Example==TLSRecursionExample.Factorial?1:Example==TLSRecursionExample.Fibonacci?n:0;
                Record(1,"Base case stops recursion. Return "+baseResult+".",baseResult.ToString());
                active.RemoveAt(active.Count-1);
                Record(1,"Unwind to caller with "+baseResult+".",baseResult.ToString());
                return baseResult;
            }
            Record(Example==TLSRecursionExample.Fibonacci?2:2,"No base case yet: recursively evaluate a smaller argument.");
            int first=Eval(n-1);
            int value;
            if(Example==TLSRecursionExample.Fibonacci)
            {
                Record(3,"Left branch returned "+first+"; evaluate fib("+(n-2)+").");
                int second=Eval(n-2);
                value=first+second;
                Record(4,"Add independent returns: "+first+" + "+second+" = "+value+".",value.ToString());
            }
            else
            {
                value=Example==TLSRecursionExample.Factorial?n*first:n+first;
                Record(3,Example==TLSRecursionExample.Factorial?
                    "Multiply n by returned factorial(n-1): "+n+" * "+first+" = "+value+".":
                    "Add n to returned sum(n-1): "+n+" + "+first+" = "+value+".",value.ToString());
            }
            active.RemoveAt(active.Count-1);
            Record(Example==TLSRecursionExample.Fibonacci?4:3,"Return "+value+" to the waiting caller.",value.ToString());
            return value;
        }
        public string InputError {get;private set;}="";
        public void RejectInput(string explanation){InputError=explanation;}
        public int FinalValue {get;private set;}
        public void Begin(TLSRecursionExample example,int n)
        {
            if((int)example<0||(int)example>2)throw new ArgumentOutOfRangeException("example");
            if(n<0 || n>(example==TLSRecursionExample.Fibonacci?6:10))
                throw new ArgumentOutOfRangeException("n","Use n = 0..10 (sum/factorial), 0..6 (fib), to keep the trace readable.");
            InputError="";Example=example;Argument=n;Frames.Clear();active.Clear();Step=0;PredictionAnswered=false;
            PredictionFeedback="Before stepping, predict the final numeric return value.";
            FinalValue=Eval(n);
            // Keep the call stack hidden until the student advances past initial framing.
            Frames.Insert(0,new TLSRecursionFrame("(no active calls yet)",-1,0,
                "First predict the final result; then step through calls and returns.",""));
        }
        public TLSRecursionLab(){Begin(TLSRecursionExample.SumToN,4);}
        public void Next(){if(Step<Frames.Count-1)Step++;}
        public void Previous(){if(Step>0)Step--;}
        public void Finish(){Step=Frames.Count-1;}
        public bool Predict(string answer)
        {
            if(PredictionAnswered || Step>0) return false;
            int parsed;bool correct=int.TryParse(answer,out parsed)&&parsed==FinalValue;
            PredictionAnswered=true;
            PredictionFeedback=(correct?"Correct. ":"Review the trace. ")+"Final return is "+FinalValue+"; inspect the base case and unwind.";
            return correct;
        }
    }
}
