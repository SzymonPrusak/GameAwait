
namespace SimEi.Threading.GameAwait.Execution
{
    public static class AwaitablesExecutor
    {
        public static void Tick<Timing>(float tickDuration)
        {
            Time<Timing>.LastTickDuration = tickDuration;

            GameAwait.YieldHandler<Timing>.Handle();
            GameAwait.DelayHandler<Timing>.Handle();
        }


        public static void SetExceptionHandler(IExceptionHandler handler)
        {
            ExceptionHandler.Handler = handler;
        }
    }
}
