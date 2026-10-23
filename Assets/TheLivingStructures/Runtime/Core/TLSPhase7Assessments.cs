using System;
using System.Collections.Generic;
using System.Globalization;

namespace TLS.Core
{
    public sealed class TLSPhase7Assessment
    {
        public bool Final {get;private set;}
        public int Index {get;private set;}
        public int CorrectCount {get;private set;}
        public bool Answered {get;private set;}
        public bool Complete {get;private set;}
        public string Feedback {get;private set;}="Choose the answer before receiving feedback.";
        public TLSPhase7Question Current {get{return TLSPhase7QuestionBank.Get(Final,Index);}}
        public TLSPhase7Assessment(bool final){Final=final;}
        public bool Answer(string choice,TLSPhase7ProgressBook book,DateTime now)
        {
            if(Complete||Answered||book==null)return false;
            if(choice!="A"&&choice!="B"&&choice!="C")return false;
            var q=Current;
            Answered=true;
            bool correct=q.Correct==choice;
            if(correct)CorrectCount++;
            Feedback=(correct?"Correct. ":"Incorrect — "+q.Misconception+". ")+q.Explanation;
            // These are checkups, not demonstrations of C implementation mastery.
            book.RecordAttempt((Final?"transfer":"diagnostic")+":"+q.TopicId,q.TopicId,correct,false,now);
            return true;
        }
        public bool Next(TLSPhase7ProgressBook book)
        {
            if(!Answered||Complete||book==null)return false;
            if(Index==TLSPhase7QuestionBank.Baseline.Length-1)
            {
                Complete=true;
                book.RecordAssessment(Final,CorrectCount,Index+1);
                Feedback="Assessment complete. This is a finite conceptual check, not C coding mastery.";
                return true;
            }
            Index++;Answered=false;Feedback="Choose the answer before receiving feedback.";return true;
        }
        public string ResultLabel()
        {return CorrectCount+"/"+(Complete?Index+1:Index+(Answered?1:0))+" correctly answered";}
    }

    public sealed class TLSPhase7Reconstruction
    {
        public readonly string Id,Topic,Prompt,Hint,Why;
        public readonly string[] Shuffled;
        public readonly int[] CorrectOrder;
        public TLSPhase7Reconstruction(string id,string topic,string prompt,string[] shuffled,int[] order,string hint,string why)
        {Id=id;Topic=topic;Prompt=prompt;Shuffled=shuffled;CorrectOrder=order;Hint=hint;Why=why;}
        public string CorrectText {get{return string.Join(",",CorrectOrder);}}
    }
    public static class TLSPhase7ReconstructionCatalog
    {
        public static readonly TLSPhase7Reconstruction[] Cards={
            new TLSPhase7Reconstruction("safe-pop","stack","Arrange steps for a guarded pop, returning success.",
                new[]{"return true;","*out = s->data[--s->count];","if (!s || !out || s->count == 0) return false;"},new[]{3,2,1},
                "Guards precede any read or write.","The guard prevents invalid access, then count changes before data is read."),
            new TLSPhase7Reconstruction("array-shift","array","Arrange indexed insertion steps after validating capacity/index.",
                new[]{"a->data[index] = value;","a->count++;","for (size_t j = a->count; j > index; --j) a->data[j] = a->data[j-1];"},new[]{3,1,2},
                "Move cells right from the end, then write value.","Right-to-left shifting preserves all old elements before the inserted value is stored."),
            new TLSPhase7Reconstruction("list-delete","list","Arrange safe removal of the head node.",
                new[]{"free(old);","Node *old = *head;","*head = old->next;"},new[]{2,3,1},
                "Keep an alias to the old allocation until free.","Save old, publish the successor, and free only after the last required dereference."),
            new TLSPhase7Reconstruction("bst-guard","bst","Arrange recursive lookup steps for a BST.",
                new[]{"return key < root->data ? search(root->left, key) : search(root->right, key);",
                      "if (root->data == key) return true;","if (root == NULL) return false;"},new[]{3,2,1},
                "Null guard first, equality second, recurse last.","The search excludes null before dereferencing and chooses exactly one ordered subtree."),
            new TLSPhase7Reconstruction("queue-tail","queue","Arrange queue enqueue steps after checking the queue and capacity.",
                new[]{"q->data[tail] = value;","size_t tail = (q->head + q->count) % CAPACITY;","q->count++;"},new[]{2,1,3},
                "Compute modulo tail before the store.","The raw slot is populated before increasing the logical queue count.")
        };
    }
    public sealed class TLSPhase7ReconstructionSession
    {
        public int Index {get;private set;}
        public bool Guided {get;private set;}=true;
        public bool Exposed {get;private set;}
        private readonly HashSet<int> attempted=new HashSet<int>();
        public string Feedback {get;private set;}="Enter the comma-separated statement numbers in execution order.";
        public TLSPhase7Reconstruction Current {get{return TLSPhase7ReconstructionCatalog.Cards[Index];}}
        public void Select(int index){if(index<0||index>=TLSPhase7ReconstructionCatalog.Cards.Length)throw new ArgumentOutOfRangeException("index");Index=index;Feedback="Arrange the statements without referring to the solution.";Exposed=false;}
        public void SetGuided(bool guided){Guided=guided;}
        public void Hint(){if(Guided){Exposed=true;Feedback="HINT: "+Current.Hint;}}
        public void Reveal(){Exposed=true;Feedback="Reference order: "+Current.CorrectText+". "+Current.Why;}
        public bool Submit(string raw,TLSPhase7ProgressBook book,DateTime now)
        {
            if(book==null)return false;
            int[] sequence=Parse(raw,Current.Shuffled.Length);
            if(sequence==null){Feedback="Enter each statement number exactly once (e.g., 3,1,2).";return false;}
            bool match=true;for(int i=0;i<sequence.Length;i++)if(sequence[i]!=Current.CorrectOrder[i])match=false;
            bool first=attempted.Add(Index);
            book.RecordAttempt("parsons:"+Current.Id,Current.Topic,match,Guided||Exposed||!first,now);
            Feedback=match?"Correct order. "+Current.Why:"Incorrect order. "+Current.Hint;
            return match;
        }
        public static int[] Parse(string raw,int length)
        {
            if(raw==null||raw.Length>64||length<=0||length>10)return null;
            string[] parts=raw.Split(',');if(parts.Length!=length)return null;
            var seen=new HashSet<int>();int[] result=new int[length];
            for(int i=0;i<length;i++){int at;if(!int.TryParse(parts[i].Trim(),NumberStyles.Integer,CultureInfo.InvariantCulture,out at)||at<1||at>length||!seen.Add(at))return null;result[i]=at;}
            return result;
        }
    }

    public sealed class TLSPhase7ReviewSession
    {
        public int Index {get;private set;}
        public bool Answered {get;private set;}
        public string Feedback {get;private set;}="Choose an answer, then inspect the explanation.";
        public TLSPhase7Question Current{get{return TLSPhase7QuestionBank.GetReview(Index);}}
        public bool Choose(string answer,TLSPhase7ProgressBook book,DateTime utc)
        {
            if(Answered||book==null||(answer!="A"&&answer!="B"&&answer!="C"))return false;
            var q=Current;bool correct=answer==q.Correct;Answered=true;
            book.RecordAttempt("review:"+q.TopicId,q.TopicId,correct,true,utc);
            if(correct)book.CompleteReview(q.TopicId,utc);
            Feedback=correct?"Correct. "+q.Explanation:"Review recommended: "+q.Misconception+". "+q.Explanation;
            return true;
        }
        public bool Next()
        {if(!Answered)return false;Index=(Index+1)%TLSPhase7LearningCatalog.Topics.Length;Answered=false;Feedback="Choose an answer, then inspect the explanation.";return true;}
        public void Pick(int at){if(at<0||at>=TLSPhase7LearningCatalog.Topics.Length)throw new ArgumentOutOfRangeException("at");Index=at;Answered=false;Feedback="Choose an answer, then inspect the explanation.";}
    }
    public sealed class TLSPhase7TestDesignSession
    {
        public int Selected {get;private set;}
        public bool Guided {get;private set;}=true;
        public string Feedback {get;private set;}="Create a case and predict what the CORRECT function should return.";
        public TLSPhase7TestResult Latest {get;private set;}
        private readonly HashSet<int> exposed=new HashSet<int>();
        private readonly HashSet<int> tried=new HashSet<int>();
        public bool HintVisible {get;private set;}
        public void Select(int value)
        {if(value<0||value>=TLSPhase7TestDesign.Cases.Length)throw new ArgumentOutOfRangeException("value");Selected=value;Latest=null;HintVisible=false;Feedback="Create a case and predict the reference output.";}
        public void SetGuided(bool value){Guided=value;HintVisible=false;}
        public void Hint(){if(Guided){exposed.Add(Selected);HintVisible=true;Feedback="HINT: "+TLSPhase7TestDesign.Cases[Selected].Hint;}}
        public void InputError(){Latest=null;Feedback="Enter a whole-number target/head and expected result; invalid text does not count as an attempt.";}
        public bool Submit(string csv,int arg,int expected,TLSPhase7ProgressBook book,DateTime utc)
        {
            Latest=TLSPhase7TestDesign.Evaluate(Selected,csv,arg,expected);
            Feedback=Latest.Explanation;
            if(!Latest.Valid)return false;
            bool first=tried.Add(Selected);
            bool solves=Latest.CorrectOracle&&Latest.ExposesBug;
            bool qualified=book.RecordTestDesign(TLSPhase7TestDesign.Cases[Selected].Id,
                TLSPhase7TestDesign.Cases[Selected].Topic,solves,Guided||exposed.Contains(Selected)||!first,utc);
            return qualified;
        }
    }

    public sealed class TLSPhase7TestScenario
    {
        public readonly string Id,Topic,Title,Prompt,Hint,Why;
        public TLSPhase7TestScenario(string id,string topic,string title,string prompt,string hint,string why)
        {Id=id;Topic=topic;Title=title;Prompt=prompt;Hint=hint;Why=why;}
    }
    public sealed class TLSPhase7TestResult
    {
        public bool Valid,CorrectOracle,ExposesBug;
        public string Reference,Faulty,Explanation;
    }
    public static class TLSPhase7TestDesign
    {
        public static readonly TLSPhase7TestScenario[] Cases={
            new TLSPhase7TestScenario("last-cell","testing","Linear search: missed last cell",
                "Supply comma-separated values and a target. Predict the reference return INDEX, or -1 when absent.",
                "Try a target that appears only in the final occupied cell.",
                "The faulty loop ends at i+1 < count, so the final element is never examined."),
            new TLSPhase7TestScenario("skip-mid","analysis","Binary search: incorrect low update",
                "Supply a sorted ascending array and target; predict the reference INDEX or -1.",
                "Find an input where the wrong low=mid+2 skips a valid element.",
                "Skipping two positions violates the binary-search invariant."),
            new TLSPhase7TestScenario("wrap-tail","queue","Circular queue: missing tail modulo",
                "Supply initial logical queue values, start head (0–4), and value to enqueue. Predict the physical tail INDEX in a capacity-5 ring.",
                "Try a head near the physical end with at least one queued item.",
                "The buggy tail uses head+count without modulo and can address outside the ring.")
        };
        public static TLSPhase7TestResult Evaluate(int scenario,string csv,int arg,int expected)
        {
            var result=new TLSPhase7TestResult{Valid=false,Reference="—",Faulty="—",Explanation="Enter 1–6 integers between -1000 and 1000."};
            if(scenario<0||scenario>=Cases.Length)return result;
            int[] data=ParseValues(csv);
            if(data==null)return result;
            if(scenario==0)
            {
                int a=IndexOf(data,arg,data.Length),b=IndexOf(data,arg,Math.Max(0,data.Length-1));
                result.Reference=a.ToString(CultureInfo.InvariantCulture);result.Faulty=b.ToString(CultureInfo.InvariantCulture);result.ExposesBug=a!=b;
                result.CorrectOracle=expected==a;
            }
            else if(scenario==1)
            {
                for(int i=1;i<data.Length;i++)if(data[i]<data[i-1]){result.Explanation="Binary search requires sorted ascending input.";return result;}
                int a=Binary(data,arg,false),b=Binary(data,arg,true);
                result.Reference=a.ToString(CultureInfo.InvariantCulture);result.Faulty=b.ToString(CultureInfo.InvariantCulture);
                result.ExposesBug=(a!=b);result.CorrectOracle=expected==a;
            }
            else
            {
                if(arg<0||arg>=5||data.Length>=5){result.Explanation="Use head 0–4 and 1–4 initial queue elements.";return result;}
                int refTail=(arg+data.Length)%5,wrong=arg+data.Length;
                result.Reference=refTail.ToString(CultureInfo.InvariantCulture);
                result.Faulty=wrong>=5?"OUT OF BOUNDS ("+wrong+")":wrong.ToString(CultureInfo.InvariantCulture);
                result.ExposesBug=refTail!=wrong;result.CorrectOracle=expected==refTail;
            }
            result.Valid=true;
            result.Explanation=(result.CorrectOracle?"Correct expected result. ":"Your expected result does not match the reference. ")+
               (result.ExposesBug?"This test distinguishes the bug. ":"This case does not distinguish the faulty implementation. ")+Cases[scenario].Why;
            return result;
        }
        public static int[] ParseValues(string raw)
        {
            if(string.IsNullOrWhiteSpace(raw)||raw.Length>96)return null;
            string[] parts=raw.Split(',');if(parts.Length<1||parts.Length>6)return null;
            int[] values=new int[parts.Length];
            for(int i=0;i<parts.Length;i++)if(!int.TryParse(parts[i].Trim(),NumberStyles.Integer,CultureInfo.InvariantCulture,out values[i])||values[i]<-1000||values[i]>1000)return null;
            return values;
        }
        private static int IndexOf(int[] a,int value,int limit)
        {for(int i=0;i<limit;i++)if(a[i]==value)return i;return -1;}
        private static int Binary(int[] a,int value,bool wrong)
        {
            int low=0,high=a.Length-1;
            for(int k=0;k<16&&low<=high;k++)
            {int mid=low+(high-low)/2;if(a[mid]==value)return mid;if(a[mid]<value)low=mid+(wrong?2:1);else high=mid-1;}
            return -1;
        }
    }
}
