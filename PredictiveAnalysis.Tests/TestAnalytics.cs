using Microsoft.VisualStudio.TestTools.UnitTesting;
using PredictiveAnalysis.Api;
using System.Collections.Generic;

namespace PredictiveAnalysis.Tests
{
    [TestClass]
    public class TestAnalytics
    {
        [TestMethod]
        public void GetAnnualChurnRate()
        {
            AnalyticsController test = new AnalyticsController();
            var result = test.GetAnnualChurnRate("Ford Motor");
        }

        [TestMethod]
        public void GetChurnRate()
        {
            AnalyticsController test = new AnalyticsController();
            int[] variables = new int[] { 1001, 1002 };
            //PostData data = new PostData()
            //{
            //    CustomerName = "Ford Motor"
            //};
            //data.Inputs.AddRange(variables);
            //var result = test.GetChurnRate(data);
        }
    }
}