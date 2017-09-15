using System.Collections.Generic;

namespace PredictiveAnalysis
{
    public sealed class Response<T> 
    {
        public int Status { get; set; }
        public T Data { get; set; }
    }
}