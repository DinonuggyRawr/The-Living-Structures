using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TLS.Core
{
    /// <summary>
    /// Deliberately bounded, side-effect-free C11 *subset* interpreter for three
    /// educational stack functions. NEVER starts processes, uses a host shell,
    /// loads libraries, accesses files/network, or executes compiled user code.
    /// Not a general C compiler: unsupported valid C is reported as unsupported.
    /// </summary>
    public static class StackFunctionEvaluator
    {
        public enum Function { Push, Pop, Peek }
        public sealed class Result
        {
            public bool Accepted;
            public int Passed;
            public int Total;
            public string Message;
            public string FirstFailure;
        }
        private const int MaxCharacters = 5000;
        private const int MaxTokens = 1000;
        private const int MaxSteps = 250;
        private const int MaxDepth = 48;
        private static readonly string[] PushHeader = { "bool", "stack_push", "(", "Stack", "*", "stack", ",", "int", "value", ")" };
        private static readonly string[] PopHeader = { "bool", "stack_pop", "(", "Stack", "*", "stack", ",", "int", "*", "out_value", ")" };
        private static readonly string[] PeekHeader = { "bool", "stack_peek", "(", "const", "Stack", "*", "stack", ",", "int", "*", "out_value", ")" };

        private sealed class SyntaxFault : Exception { public SyntaxFault(string s) : base(s) { } }
        private sealed class ExecutionFault : Exception { public ExecutionFault(string s) : base(s) { } }

        private sealed class Expr
        {
            public string Op, Name;
            public Expr L, R;
            public Expr(string op, Expr l = null, Expr r = null, string name = null)
            { Op = op; L = l; R = r; Name = name; }
        }
        private sealed class Stmt
        {
            public string Op, Name;
            public Expr A, B;
            public Stmt Then, Else;
            public List<Stmt> Children;
        }
        private sealed class Parser
        {
            readonly List<string> tokens;
            int pos, nodes;
            public Parser(List<string> source) { tokens = source; }
            string Peek { get { return pos < tokens.Count ? tokens[pos] : "<end>"; } }
            bool Eat(string token) { if (Peek != token) return false; pos++; return true; }
            string Next() { string t = Peek; if (pos >= tokens.Count) throw new SyntaxFault("Unexpected end of function."); pos++; return t; }
            void Need(string s) { if (!Eat(s)) throw new SyntaxFault("Expected '" + s + "' near '" + Peek + "'."); }
            Expr Make(string op, Expr a = null, Expr b = null, string n = null)
            { if (++nodes > MaxTokens) throw new SyntaxFault("Program is too complex for this learning subset."); return new Expr(op,a,b,n); }
            Stmt Statement(int depth)
            {
                if (depth > MaxDepth) throw new SyntaxFault("Statement nesting exceeds the learning limit.");
                if (++nodes > MaxTokens) throw new SyntaxFault("Program is too complex for this learning subset.");
                if (Eat("{"))
                {
                    Stmt b = new Stmt { Op="block", Children=new List<Stmt>() };
                    while (Peek != "}") { if (Peek == "<end>") throw new SyntaxFault("Missing closing brace."); b.Children.Add(Statement(depth+1)); }
                    Need("}"); return b;
                }
                if (Eat("if"))
                {
                    Need("("); Expr condition = Expression(); Need(")");
                    Stmt yes = Statement(depth+1), no = Eat("else") ? Statement(depth+1) : null;
                    return new Stmt { Op="if", A=condition, Then=yes, Else=no };
                }
                if (Eat("return")) { Expr e=Expression(); Need(";"); return new Stmt { Op="return", A=e }; }
                if (Peek == "int" || Peek == "size_t" || Peek == "bool")
                {
                    Next(); string name = Next();
                    if (!IsIdentifier(name) || Reserved(name)) throw new SyntaxFault("Invalid local variable name.");
                    Expr init = Eat("=") ? Expression() : null;
                    Need(";"); return new Stmt { Op="local", Name=name, A=init };
                }
                Expr target = Expression(); string edit = Next();
                if (edit == "=" || edit == "+=" || edit == "-=")
                { Expr value = Expression(); Need(";"); return new Stmt { Op=edit, A=target, B=value }; }
                if (edit == "++" || edit == "--") { Need(";"); return new Stmt { Op=edit, A=target }; }
                throw new SyntaxFault("Only C assignments, ++/--, if/else, returns, and simple locals are supported; encountered '" + edit + "'.");
            }
            static bool Reserved(string s)
            { return s == "stack" || s == "out_value" || s == "value" || s == "CAPACITY" || s == "NULL" || s == "true" || s == "false"; }
            static bool IsIdentifier(string s)
            { if (s.Length==0 || !(char.IsLetter(s[0]) || s[0]=='_')) return false;
              for(int i=1;i<s.Length;i++) if(!(char.IsLetterOrDigit(s[i]) || s[i]=='_')) return false; return true; }
            public Stmt Parse(Function function)
            {
                string[] header = function == Function.Push ? PushHeader : (function == Function.Pop ? PopHeader : PeekHeader);
                foreach (string expected in header) Need(expected);
                Stmt body = Statement(0);
                if (body.Op != "block") throw new SyntaxFault("A complete function body in braces is required.");
                if (pos != tokens.Count) throw new SyntaxFault("Unexpected text after the function closing brace.");
                return body;
            }
            Expr Expression(int minPrecedence=1)
            {
                Expr left = Prefix();
                while (true)
                {
                    int precedence = Prec(Peek);
                    if (precedence < minPrecedence) return left;
                    string op=Next(); Expr right=Expression(precedence+1);
                    left=Make(op,left,right);
                }
            }
            Expr Prefix()
            {
                if (Eat("!")) return Make("!",Prefix());
                if (Eat("-")) return Make("neg",Prefix());
                if (Eat("*")) return Make("deref",Prefix());
                Expr e;
                if (Eat("(")) { e=Expression(); Need(")"); }
                else
                {
                    string atom=Next();
                    int n;
                    if (int.TryParse(atom,NumberStyles.None,CultureInfo.InvariantCulture,out n)) e=Make("literal",n:atom);
                    else if (IsIdentifier(atom)) e=Make("name",n:atom);
                    else throw new SyntaxFault("Unsupported C expression near '"+atom+"'.");
                }
                while (true)
                {
                    if (Eat("->")) e=Make("member",e,null,Next());
                    else if (Eat("[")) { Expr index=Expression(); Need("]"); e=Make("index",e,index); }
                    else break;
                }
                return e;
            }
            static int Prec(string t)
            {
                if(t=="||") return 1;
                if(t=="&&") return 2;
                if(t=="=="||t=="!=") return 3;
                if(t=="<"||t==">"||t=="<="||t==">=") return 4;
                if(t=="+"||t=="-") return 5;
                if(t=="*"||t=="/"||t=="%") return 6;
                return -1;
            }
        }
        private static List<string> Tokenize(string source)
        {
            if (source == null || source.Length > MaxCharacters) throw new SyntaxFault("C source must be at most " + MaxCharacters + " characters.");
            List<string> result=new List<string>(); int i=0;
            while (i < source.Length)
            {
                char c=source[i]; if(char.IsWhiteSpace(c)) { i++; continue; }
                if(c=='/' && i+1<source.Length && source[i+1]=='/')
                { i+=2; while(i<source.Length && source[i]!='\n') i++; continue; }
                if(c=='/' && i+1<source.Length && source[i+1]=='*')
                { i+=2; int stop=source.IndexOf("*/",i,StringComparison.Ordinal);
                  if(stop<0) throw new SyntaxFault("Unclosed comment."); i=stop+2; continue; }
                if(c=='#' || c=='"' || c=='\'' || c=='\\') throw new SyntaxFault("Directives, literals, and escapes are outside this bounded C subset.");
                if(char.IsLetter(c)||c=='_'||char.IsDigit(c))
                { int start=i++; while(i<source.Length && (char.IsLetterOrDigit(source[i])||source[i]=='_')) i++;
                  result.Add(source.Substring(start,i-start)); }
                else
                {
                    string pair=i+1<source.Length?source.Substring(i,2):"";
                    if(pair=="->"||pair=="=="||pair=="!="||pair==">="||pair=="<="||pair=="&&"||pair=="||"||pair=="++"||pair=="--"||pair=="+="||pair=="-=")
                    {result.Add(pair);i+=2;}
                    else if("(){}[];,=*+-!<>/%".IndexOf(c)>=0) {result.Add(c.ToString());i++;}
                    else throw new SyntaxFault("Unsupported character '"+c+"'.");
                }
                if(result.Count>MaxTokens) throw new SyntaxFault("C subset token limit exceeded.");
            }
            return result;
        }
        private sealed class Machine
        {
            public int[] Data = new int[5];
            public int Count, Out, Input, Steps;
            public bool HasStack = true, HasOutput = true, DidReturn;
            public int ReturnValue;
            public Dictionary<string,int> Locals=new Dictionary<string,int>();
            int Tick() { if(++Steps>MaxSteps) throw new ExecutionFault("Execution exceeded 250 bounded steps."); return 0; }
            int Read(Expr e)
            {
                Tick(); if(e==null) throw new ExecutionFault("Missing expression.");
                if(e.Op=="literal") { int n; if(!int.TryParse(e.Name,NumberStyles.None,CultureInfo.InvariantCulture,out n)) throw new ExecutionFault("Literal exceeds 32-bit signed integer range."); return n; }
                if(e.Op=="name")
                {
                    if(e.Name=="NULL" || e.Name=="false") return 0;
                    if(e.Name=="true") return 1;
                    if(e.Name=="CAPACITY") return Data.Length;
                    if(e.Name=="stack") return HasStack?1:0;
                    if(e.Name=="out_value") return HasOutput?1:0;
                    if(e.Name=="value") return Input;
                    int v; if(Locals.TryGetValue(e.Name,out v)) return v;
                    throw new ExecutionFault("Unknown variable '" + e.Name + "'.");
                }
                if(e.Op=="member")
                {
                    if(e.L.Op!="name" || e.L.Name!="stack" || !HasStack) throw new ExecutionFault("Invalid stack pointer dereference.");
                    if(e.Name=="count") return Count;
                    throw new ExecutionFault("Only stack->count and stack->data[index] are modeled.");
                }
                if(e.Op=="index")
                {
                    if(e.L.Op!="member" || e.L.Name!="data" || e.L.L.Op!="name" || e.L.L.Name!="stack" || !HasStack)
                        throw new ExecutionFault("Only stack->data[index] is modeled.");
                    int idx=Read(e.R);
                    if(idx<0||idx>=Data.Length) throw new ExecutionFault("Array index out of bounds: "+idx+".");
                    return Data[idx];
                }
                if(e.Op=="deref")
                { if(e.L.Op!="name"||e.L.Name!="out_value"||!HasOutput) throw new ExecutionFault("Invalid output pointer dereference."); return Out; }
                if(e.Op=="!") return Read(e.L)==0?1:0;
                if(e.Op=="neg") { long v=-(long)Read(e.L); return Checked(v); }
                if(e.Op=="&&") { int a=Read(e.L); return a==0?0:(Read(e.R)==0?0:1); }
                if(e.Op=="||") { int a=Read(e.L); return a!=0?1:(Read(e.R)==0?0:1); }
                long l=Read(e.L),r=Read(e.R);
                switch(e.Op)
                {
                    case "+": return Checked(l+r); case "-": return Checked(l-r);
                    case "*": return Checked(l*r);
                    case "/": if(r==0) throw new ExecutionFault("Division by zero."); return Checked(l/r);
                    case "%": if(r==0) throw new ExecutionFault("Modulo by zero."); return Checked(l%r);
                    case "==": return l==r?1:0; case "!=": return l!=r?1:0;
                    case "<": return l<r?1:0; case ">": return l>r?1:0;
                    case "<=": return l<=r?1:0; case ">=": return l>=r?1:0;
                }
                throw new ExecutionFault("Unsupported expression operator '"+e.Op+"'.");
            }
            static int Checked(long n)
            { if(n<int.MinValue||n>int.MaxValue) throw new ExecutionFault("Signed integer overflow (undefined in C)."); return (int)n; }
            void Write(Expr dst,int v)
            {
                if(dst.Op=="name" && Locals.ContainsKey(dst.Name)) { Locals[dst.Name]=v; return; }
                if(dst.Op=="member" && dst.L.Op=="name" && dst.L.Name=="stack" && dst.Name=="count" && HasStack)
                { if(v<0||v>5) throw new ExecutionFault("stack->count left valid range 0..5."); Count=v; return; }
                if(dst.Op=="index" && dst.L.Op=="member" && dst.L.Name=="data" && dst.L.L.Op=="name" && dst.L.L.Name=="stack" && HasStack)
                { int idx=Read(dst.R); if(idx<0||idx>=5) throw new ExecutionFault("Array write index out of bounds: "+idx+"."); Data[idx]=v; return; }
                if(dst.Op=="deref" && dst.L.Op=="name" && dst.L.Name=="out_value" && HasOutput)
                { Out=v; return; }
                throw new ExecutionFault("Unsupported write destination or invalid pointer.");
            }
            public void Run(Stmt s,int depth)
            {
                Tick(); if(DidReturn) return;
                if(depth>MaxDepth) throw new ExecutionFault("Execution nesting limit exceeded.");
                if(s.Op=="block") { foreach(Stmt x in s.Children) {Run(x,depth+1);if(DidReturn)break;} return; }
                if(s.Op=="if") { if(Read(s.A)!=0)Run(s.Then,depth+1); else if(s.Else!=null)Run(s.Else,depth+1); return; }
                if(s.Op=="return") {ReturnValue=Read(s.A);DidReturn=true;return;}
                if(s.Op=="local")
                { if(Locals.ContainsKey(s.Name)) throw new ExecutionFault("Duplicate local variable.");
                  if(s.A==null) throw new ExecutionFault("Uninitialized locals are unsupported; initialize the variable first.");
                  Locals[s.Name]=Read(s.A);return; }
                if(s.Op=="="||s.Op=="+="||s.Op=="-=")
                {int n=Read(s.B);if(s.Op!="=") n=Checked((long)Read(s.A)+(s.Op=="+="?n:-n)); Write(s.A,n); return;}
                if(s.Op=="++"||s.Op=="--") { Write(s.A,Checked((long)Read(s.A)+(s.Op=="++"?1:-1)));return; }
                throw new ExecutionFault("Unknown statement.");
            }
        }
        private sealed class Case
        {
            public string Name; public int[] Starting; public int Count, Input, Out=999;
            public bool HasStack=true, HasOutput=true;
            public Case(string name, int[] values,int count,int input=0) {Name=name;Starting=values;Count=count;Input=input;}
        }
        private static List<Case> Cases(Function f)
        {
            List<Case> c=new List<Case>();
            int[] zero=new int[5], one={7,0,0,0,0}, two={-3,21,0,0,0}, full={10,-2,30,0,50};
            if(f==Function.Push)
            {
                c.Add(new Case("empty stack",zero,0,12));c.Add(new Case("one element",one,1,-9));
                c.Add(new Case("partly full",two,2,0));c.Add(new Case("last free slot",new[]{1,2,3,4,0},4,99));
                c.Add(new Case("full stack",full,5,20));c.Add(new Case("negative boundary",zero,0,int.MinValue));
                c.Add(new Case("positive boundary",two,2,int.MaxValue));
                Case nil=new Case("null stack pointer",zero,0,1);nil.HasStack=false;c.Add(nil);
            }
            else
            {
                c.Add(new Case("empty stack",zero,0));c.Add(new Case("one element",one,1));
                c.Add(new Case("two elements",two,2));c.Add(new Case("full stack",full,5));
                c.Add(new Case("inactive slot preserved",new[]{-6,8,77,0,0},2));
                Case nil=new Case("null stack pointer",zero,0);nil.HasStack=false;c.Add(nil);
                Case outNil=new Case("null output pointer",two,2);outNil.HasOutput=false;c.Add(outNil);
                Case both=new Case("null pointers",one,1);both.HasStack=false;both.HasOutput=false;c.Add(both);
            }
            return c;
        }
        public static Result Check(Function function,string source)
        {
            Result result=new Result { Total=8, Message="No tests run." };
            Stmt ast;
            try { ast=new Parser(Tokenize(source)).Parse(function); }
            catch(SyntaxFault e) { result.Message="Subset syntax limitation: " + e.Message; result.FirstFailure=result.Message;return result; }
            List<Case> cases=Cases(function);result.Total=cases.Count;
            foreach(Case test in cases)
            {
                Machine m=new Machine { Count=test.Count,Input=test.Input,Out=test.Out,HasStack=test.HasStack,HasOutput=test.HasOutput };
                Array.Copy(test.Starting,m.Data,5);
                bool expectedSuccess=test.HasStack && (function==Function.Push?test.Count<5:test.HasOutput && test.Count>0);
                int expectedCount=expectedSuccess?(test.Count+(function==Function.Push?1:(function==Function.Pop?-1:0))):test.Count;
                int expectedOut=expectedSuccess && function!=Function.Push ? test.Starting[test.Count-1] : test.Out;
                int[] expectedData=(int[])test.Starting.Clone();
                if(expectedSuccess && function==Function.Push) expectedData[test.Count]=test.Input;
                try
                {
                    m.Run(ast,0);
                    if(!m.DidReturn) throw new ExecutionFault("Function ended without a return value.");
                    if(m.ReturnValue!=0 && m.ReturnValue!=1) throw new ExecutionFault("Expected a boolean return value (0 or 1).");
                    if((m.ReturnValue==1)!=expectedSuccess || m.Count!=expectedCount || m.Out!=expectedOut)
                        throw new ExecutionFault("Expected return="+(expectedSuccess?"true":"false")+", count="+expectedCount+
                            ", out="+expectedOut+"; got return="+(m.ReturnValue==1?"true":"false")+", count="+m.Count+", out="+m.Out+".");
                    for(int i=0;i<5;i++) if(m.Data[i]!=expectedData[i])
                        throw new ExecutionFault("Backing array differs at data["+i+"]: expected "+expectedData[i]+", got "+m.Data[i]+".");
                    result.Passed++;
                }
                catch(ExecutionFault e)
                {result.FirstFailure=test.Name+": "+e.Message; result.Message="Passed "+result.Passed+"/"+result.Total+"; first failure: "+result.FirstFailure; return result;}
                catch(OverflowException)
                {result.FirstFailure=test.Name+": numeric overflow.";result.Message="Passed "+result.Passed+"/"+result.Total+"; first failure: "+result.FirstFailure;return result;}
            }
            result.Accepted=true;
            result.Message="All "+result.Total+" modeled tests passed for the supported C subset. This is not native C compilation or certified mastery.";
            return result;
        }
        public static string Reference(Function f)
        {
            if(f==Function.Push) return string.Join("\n",StackSimulation.PushCode);
            if(f==Function.Pop) return string.Join("\n",StackSimulation.PopCode);
            return string.Join("\n",StackSimulation.PeekCode);
        }
        public static string Starter(Function f)
        {
            string[] header=f==Function.Push?PushHeader:(f==Function.Pop?PopHeader:PeekHeader);
            StringBuilder b=new StringBuilder(); b.Append(string.Join(" ",header)).Append(" {\n");
            b.Append("    /* TODO: validate pointers and stack bounds. */\n");
            b.Append("    /* TODO: implement the stack operation. */\n");
            b.Append("    return false;\n}");return b.ToString();
        }
        public static string Objective(Function f)
        {
            if(f==Function.Push) return "Write the complete C push function. Avoid writes when full or NULL, and update count only on success.";
            if(f==Function.Pop) return "Write a complete C pop function. Check NULL/empty, decrease count, and store the popped value through out_value.";
            return "Write a complete C peek function. Check NULL/empty, read top without changing count, and write to out_value.";
        }
        public static string Hint(Function f)
        {
            if(f==Function.Push) return "Guard against NULL and count >= CAPACITY before data[count] = value; then increment count.";
            if(f==Function.Pop) return "Guard against a NULL stack, NULL output, or count == 0. Decrement count before reading data[count].";
            return "Guard against NULL stack, NULL output, and empty stack. Read data[count - 1] without changing count.";
        }
    }
}
