using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PredictiveAnalysis.Controllers;

namespace PredictiveAnalysis.Tests
{
    [TestClass]
    public class NlpTest
    {
        [TestMethod]
        public void TestGetScoreEx()
        {
            NlpController nlp = new NlpController();
            nlp.GetScoreEx();
        }

        [TestMethod]
        public void TestGetScore()
        {
            NlpController nlp = new NlpController();
            nlp.GetScore();
        }
    }
}
