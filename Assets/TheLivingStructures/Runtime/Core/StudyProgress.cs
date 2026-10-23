using UnityEngine;

namespace TLS.Core
{
    public static class StudyProgress
    {
        // Separate namespace from The Living Internet and from future C coding mastery.
        private const string PracticeKey = "tls.phase1.stack.trace_practice_completed.v1";
        private const string IndependentKey = "tls.phase1.stack.independent_trace_pass.v1";

        public static bool TracePracticeComplete { get { return PlayerPrefs.GetInt(PracticeKey, 0) == 1; } }
        public static bool IndependentTracePass { get { return PlayerPrefs.GetInt(IndependentKey, 0) == 1; } }
        public static void RecordTracePractice(bool allFirstAttemptCorrect)
        {
            PlayerPrefs.SetInt(PracticeKey, 1);
            if (allFirstAttemptCorrect) PlayerPrefs.SetInt(IndependentKey, 1);
            PlayerPrefs.Save();
        }
    }
}
