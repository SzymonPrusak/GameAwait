using System;

namespace SimEi.Threading.GameTask.Execution
{
    public interface IExceptionHandler
    {
        void OnException(Exception e);
    }
}
