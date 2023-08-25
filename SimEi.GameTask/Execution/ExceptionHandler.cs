using System;

namespace SimEi.Threading.GameTask.Execution
{
    internal static class ExceptionHandler
    {
        public static IExceptionHandler? Handler { get; set; }

        public static void OnException(Exception e)
        {
            if (Handler == null)
                throw new InvalidOperationException($"{nameof(IExceptionHandler)} not initialized");

            Handler.OnException(e);
        }
    }
}
