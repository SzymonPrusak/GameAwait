using System;

namespace SimEi.Threading.GameAwait.Execution
{
    public interface IExceptionHandler
    {
        void OnException(Exception e);
    }
}
