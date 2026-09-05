using System.Threading;

namespace IIMLib.Dialogue.Authoring
{
    internal static class DialogueAuthoringRevision
    {
        private static long _next = 1;

        public static long Next()
        {
            var value = Interlocked.Increment(ref _next);
            if (value > 0)
                return value;

            Interlocked.Exchange(ref _next, 1);
            return 1;
        }
    }
}
