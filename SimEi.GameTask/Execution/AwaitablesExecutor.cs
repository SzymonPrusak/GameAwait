
namespace SimEi.Threading.GameTask.Execution
{
    public static class AwaitablesExecutor
    {
        public static void Tick<Timing>(float tickDuration)
        {
            Time<Timing>.LastTickDuration = tickDuration;

            GameTask.YieldHandler<Timing>.Handle();
            GameTask.DelayHandler<Timing>.Handle();
        }


        public static void SetExceptionHandler(IExceptionHandler handler)
        {
            ExceptionHandler.Handler = handler;
        }
    }
}
