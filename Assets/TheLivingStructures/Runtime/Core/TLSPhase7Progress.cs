using System;
using System.Collections.Generic;
using UnityEngine;

namespace TLS.Core
{
    /// <summary>
    /// Local, separate Phase 7 learning record. These are OBSERVED ACTION categories,
    /// not certified C mastery. No prior Academy/Stack/Phase 1-6 save is migrated or edited.
    /// </summary>
    [Serializable] public sealed class TLSPhase7TopicEvidence
    {
        public string id;
        public int views, guided, independent, compilerCases, misconceptions, testDesign;
        public long lastPracticeUtcTicks, nextReviewUtcTicks;
    }
    [Serializable] public sealed class TLSPhase7Progress
    {
        public int version=1;
        public TLSPhase7TopicEvidence[] topics;
        public string[] attemptedIds=new string[0];
        public string[] independentIds=new string[0];
        public int diagnosticCorrect=-1, finalCorrect=-1;
        public int diagnosticAnswered, finalAnswered;
    }
    public sealed class TLSPhase7ProgressBook
    {
        public const string SaveKey="tls_phase7_learning_evidence_v1";
        public readonly TLSPhase7Progress Data;
        private readonly HashSet<string> attempted;
        private readonly HashSet<string> independent;
        public TLSPhase7ProgressBook(TLSPhase7Progress existing=null)
        {
            Data=existing??Fresh();
            if(!Valid(Data))throw new ArgumentException("Invalid or incompatible Phase 7 learning record.");
            attempted=new HashSet<string>(Data.attemptedIds,StringComparer.Ordinal);
            independent=new HashSet<string>(Data.independentIds,StringComparer.Ordinal);
        }
        public static TLSPhase7Progress Fresh()
        {
            var p=new TLSPhase7Progress();
            p.topics=new TLSPhase7TopicEvidence[TLSPhase7LearningCatalog.Topics.Length];
            for(int i=0;i<p.topics.Length;i++)p.topics[i]=new TLSPhase7TopicEvidence{id=TLSPhase7LearningCatalog.Topics[i].Id};
            return p;
        }
        public static bool Valid(TLSPhase7Progress p)
        {
            if(p==null||p.version!=1||p.topics==null||p.topics.Length!=TLSPhase7LearningCatalog.Topics.Length||p.attemptedIds==null||p.independentIds==null||
               p.attemptedIds.Length>1024||p.independentIds.Length>1024||p.diagnosticCorrect< -1||p.finalCorrect< -1||
               p.diagnosticCorrect>13||p.finalCorrect>13||p.diagnosticAnswered<0||p.diagnosticAnswered>13||p.finalAnswered<0||p.finalAnswered>13)return false;
            for(int i=0;i<p.topics.Length;i++)
            {
                var t=p.topics[i];if(t==null||t.id!=TLSPhase7LearningCatalog.Topics[i].Id||
                  t.views<0||t.guided<0||t.independent<0||t.compilerCases<0||t.misconceptions<0||t.testDesign<0||
                  t.views>10000||t.guided>10000||t.independent>10000||t.compilerCases>10000||t.testDesign>10000||t.misconceptions>10000||
                  t.lastPracticeUtcTicks<0||t.nextReviewUtcTicks<0)return false;
            }
            var a=new HashSet<string>(StringComparer.Ordinal);
            foreach(string id in p.attemptedIds)if(string.IsNullOrEmpty(id)||id.Length>96||!a.Add(id))return false;
            var allAttempted=new HashSet<string>(a,StringComparer.Ordinal);
            a.Clear();foreach(string id in p.independentIds)
                if(string.IsNullOrEmpty(id)||id.Length>96||!a.Add(id)||!allAttempted.Contains(id))return false;
            return true;
        }
        public static bool TryParse(string json,out TLSPhase7ProgressBook book)
        {
            book=null;
            if(string.IsNullOrWhiteSpace(json)||json.Length>250000)return false;
            try {var p=JsonUtility.FromJson<TLSPhase7Progress>(json);if(!Valid(p))return false;book=new TLSPhase7ProgressBook(p);return true;}
            catch {return false;}
        }
        public string ExportJson()
        {
            Data.attemptedIds=Sorted(attempted);
            Data.independentIds=Sorted(independent);
            return JsonUtility.ToJson(Data);
        }
        private static string[] Sorted(HashSet<string> items){var a=new List<string>(items);a.Sort(StringComparer.Ordinal);return a.ToArray();}
        public TLSPhase7TopicEvidence Topic(string topicId)
        {int at=TLSPhase7LearningCatalog.Find(topicId);if(at<0)throw new ArgumentOutOfRangeException("topicId");return Data.topics[at];}
        public bool Seen(string id){return attempted.Contains(id);}
        public bool Independent(string id){return independent.Contains(id);}
        private static long Ticks(DateTime utc)
        {if(utc.Kind!=DateTimeKind.Utc)throw new ArgumentException("UTC timestamp required.");return utc.Ticks;}
        private static void CountEvidence(TLSPhase7TopicEvidence topic,DateTime now,bool guided)
        {
            if(guided)topic.guided++;else topic.independent++;
            topic.lastPracticeUtcTicks=Ticks(now);
            // Review prompts are surfaced on return to the Academy, not scheduled notifications.
            topic.nextReviewUtcTicks=now.AddDays(guided?1:3).Ticks;
        }
        public void MarkViewed(string topicId)
        {TLSPhase7TopicEvidence t=Topic(topicId);if(t.views==0)t.views=1;}
        public bool RecordAttempt(string id,string topicId,bool correct,bool guidedOrExposed,DateTime now)
        {
            if(string.IsNullOrWhiteSpace(id)||id.Length>96)throw new ArgumentException("Bad assessment key.");
            TLSPhase7TopicEvidence t=Topic(topicId);
            bool first=attempted.Add(id);
            if(!correct){if(first)t.misconceptions++;return false;}
            if(first)
            {
                bool clean=!guidedOrExposed;
                CountEvidence(t,now,!clean);
                if(clean)independent.Add(id);
                return clean;
            }
            return false;
        }
        public bool RecordTestDesign(string id,string topicId,bool identifiesBug,bool guidedOrExposed,DateTime now)
        {
            bool independentPass=RecordAttempt("test:"+id,topicId,identifiesBug,guidedOrExposed,now);
            if(identifiesBug&&independentPass)Topic(topicId).testDesign++;
            return independentPass;
        }
        public bool RecordCompilerCase(string id,string topicId,bool compiled,bool complete,bool allPassed,bool guidedOrExposed,bool previouslyAttempted,DateTime now)
        {
            if(!compiled||!complete||!allPassed)return false;
            bool clean=RecordAttempt("wasm:"+id,topicId,true,guidedOrExposed||previouslyAttempted,now);
            if(clean)Topic(topicId).compilerCases++;
            return clean;
        }
        public void RecordAssessment(bool final,int correct,int answered)
        {
            if(answered<0||answered>TLSPhase7QuestionBank.Baseline.Length||correct<0||correct>answered)throw new ArgumentOutOfRangeException("correct");
            if(final){if(Data.finalAnswered==0){Data.finalCorrect=correct;Data.finalAnswered=answered;}}
            else {if(Data.diagnosticAnswered==0){Data.diagnosticCorrect=correct;Data.diagnosticAnswered=answered;}}
        }
        public bool ReviewDue(string topicId,DateTime now)
        {long next=Topic(topicId).nextReviewUtcTicks;return next>0&&next<=Ticks(now);}
        public int DueCount(DateTime now)
        {int count=0;foreach(var t in Data.topics)if(t.nextReviewUtcTicks>0&&t.nextReviewUtcTicks<=Ticks(now))count++;return count;}
        public string Recommendation(DateTime now)
        {
            foreach(var t in Data.topics)if(t.nextReviewUtcTicks>0&&t.nextReviewUtcTicks<=Ticks(now))return t.id;
            foreach(var t in Data.topics)if(t.misconceptions>0&&t.independent==0)return t.id;
            foreach(var t in Data.topics)if(t.views==0)return t.id;
            return "testing";
        }
        public void CompleteReview(string topicId,DateTime now)
        {var t=Topic(topicId);t.lastPracticeUtcTicks=Ticks(now);t.nextReviewUtcTicks=now.AddDays(7).Ticks;}
    }
}
