using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PredictiveAnalysis.Api
{
    public class PostData
    {
        public string Timeline { get; set; }
        public string ValueFilter { get; set; }
        public string Value { get; set; }
        public string CustomerName { get; set; }
        public List<int> Inputs { get; set; }

        public PostData()
        {
            Inputs = new List<int>();
        }
    }
}