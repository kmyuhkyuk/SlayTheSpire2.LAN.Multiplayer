using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Logging;

namespace SlayTheSpire2.LAN.Multiplayer.Helpers
{
    internal static class TaskGenericHelper
    {
        public static Task<T> RunSafely<T>(Task<T> task)
        {
            return LogTaskExceptions(task);
        }

        private static async Task<T> LogTaskExceptions<T>(Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                if (ex is not TaskCanceledException)
                {
                    Log.Error(ex.ToString());
                    SentryService.CaptureException(ex);
                }

                throw;
            }
        }
    }
}