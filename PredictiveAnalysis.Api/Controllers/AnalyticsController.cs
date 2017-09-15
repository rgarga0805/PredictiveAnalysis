using Accord.Controls;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression;
using Flurl;
using Flurl.Http;
using NCalc;
using Newtonsoft.Json;
using PredictiveAnalysis.Api.Models;
using PredictiveAnalysis.Common;
using PredictiveAnalysis.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using ZedGraph;

namespace PredictiveAnalysis.Api
{
    public class AnalyticsController : ApiController
    {
        private readonly PredictiveAnalysisEntities _dbContext;
        public static Object NPLResult;

        public AnalyticsController()
        {
            _dbContext = new PredictiveAnalysisEntities();
        }

        //[STAThread]
        [HttpGet]
        public IHttpActionResult GetAnnualChurnRate(string customerName)
        {
            dynamic customerAnnulInfo = new ExpandoObject();
            double[][] inputs;
            double[] outputs;
            string[] inputVariableList = CacheManager.GetItem(CacheKeys.IndependantVariables).ConvertTo<string>().Split(Literals.Comma.ToCharArray());
            string outputVariableName = CacheManager.GetItem(CacheKeys.OutputVariable).ConvertTo<string>();
            List<Variable> inputVariables = _dbContext.Variables.Where(v => v.VariableType == 0 && inputVariableList.Contains(v.DisplayName)).ToList();
            Variable outputVariable = _dbContext.Variables.Where(v => v.VariableType == 1 && v.DisplayName == outputVariableName).Distinct().SingleOrDefault();
            List<PredictiveAnalysis.Domain.Dataset> data = _dbContext.Datasets.
                                                            Where(ds => inputVariableList.Contains(ds.Variable.DisplayName)
                                                                 && ds.CustomerName.ToLower() == customerName.ToLower())
                                                            .ToList();
            DataTable dataSets = CreateDataSets(inputVariables, outputVariable, data);
            //PopulateData(ref dataSets, )
            string[] independantVariables = inputVariables.Select(v => v.DisplayName).ToArray();
            string dependantVariable = outputVariable.DisplayName;

            DataTable independent = dataSets.DefaultView.ToTable(false, independantVariables);
            DataTable dependent = dataSets.DefaultView.ToTable(false, dependantVariable);
            inputs = independent.ToJagged();
            outputs = (dependent.Columns[dependantVariable].ToArray());
            LogisticRegressionAnalysis lra = new LogisticRegressionAnalysis()
            {
                Inputs = independantVariables,
                Output = dependantVariable
            };
            LogisticRegression lr = lra.Learn(inputs, outputs);
            //var test22 = lra.Result;
            //Code for the graph

            // List<DataRow> list = dataSets.AsEnumerable().ToList();

            List<DataRow> list = new List<DataRow>();
            foreach (DataRow dr in dataSets.Rows)
            {
                list.Add(dr);
            }

           // var graphImage = ImageMethod(lra.Result);
            customerAnnulInfo.churnRate = lra.ChiSquare.PValue.ToString("N5"); ;

            string json = JsonConvert.SerializeObject(dataSets, Formatting.Indented);
            // customerAnnulInfo.dataSet = JsonConvert.DeserializeObject(json);
            customerAnnulInfo.testData = list;
            //customerAnnulInfo.graphImage = graphImage;
            customerAnnulInfo.xAxisDataPoints = lra.Result;

            return Ok(customerAnnulInfo);
        }

        //Get the January Month Churn Rate for all the Customers
        [HttpGet]
        public IHttpActionResult GetJanCustomerChurnRate()
        {
            dynamic customerAnnulInfo = new ExpandoObject();
            double[][] inputs;
            double[] outputs;
            string[] inputVariableList = CacheManager.GetItem(CacheKeys.IndependantVariables).ConvertTo<string>().Split(Literals.Comma.ToCharArray());
            string outputVariableName = CacheManager.GetItem(CacheKeys.OutputVariable).ConvertTo<string>();
            List<Variable> inputVariables = _dbContext.Variables.Where(v => v.VariableType == 0 && inputVariableList.Contains(v.DisplayName)).ToList();
            Variable outputVariable = _dbContext.Variables.Where(v => v.VariableType == 1 && v.DisplayName == outputVariableName).Distinct().SingleOrDefault();

            var months = _dbContext.Datasets.Select(x => x.Month).Distinct().ToList();
            List<double> averageResultPerMonth = new List<double>();
            List<DataRow> list = new List<DataRow>();
            foreach (var item in months)
            {
                List<PredictiveAnalysis.Domain.Dataset> data = _dbContext.Datasets
                                                               .Where(ds => inputVariableList.Contains(ds.Variable.DisplayName) && ds.Year == "2013" && ds.Month == item).ToList();
                DataTable dataSets = CreateDataSets(inputVariables, outputVariable, data);
                string[] independantVariables = inputVariables.Select(v => v.DisplayName).ToArray();
                string dependantVariable = outputVariable.DisplayName;

                DataTable independent = dataSets.DefaultView.ToTable(false, independantVariables);
                DataTable dependent = dataSets.DefaultView.ToTable(false, dependantVariable);
                inputs = independent.ToJagged();
                outputs = (dependent.Columns[dependantVariable].ToArray());
                LogisticRegressionAnalysis lra = new LogisticRegressionAnalysis()
                {
                    Inputs = independantVariables,
                    Output = dependantVariable
                };
                LogisticRegression lr = lra.Learn(inputs, outputs);
                var probabilities = lr.Probability(inputs);
                var result = probabilities.Average();
                averageResultPerMonth.Add(result);
                string json = JsonConvert.SerializeObject(dataSets, Formatting.Indented);

                foreach (DataRow dr in dataSets.Rows)
                {
                    list.Add(dr);
                }
                customerAnnulInfo.testData = list;
                // customerAnnulInfo.graphImage = graphImage;
                //customerAnnulInfo.xAxisDataPoints = lra.Result;
            }
            var finalResult = averageResultPerMonth.Average();
            customerAnnulInfo.churnRate = finalResult;
            customerAnnulInfo.xAxisDataPoints = averageResultPerMonth;

            return Ok(customerAnnulInfo);
        }

        [Route("api/Analytics/GetCustomerInfo")]
        [HttpGet]
        public IHttpActionResult GetCustomerInfo()
        {
            dynamic customerData = new ExpandoObject();
            customerData = _dbContext.Customers.Select(x => new { x.DisplayName, x.Id }).ToList();
            return Ok(customerData);
        }

        [Route("api/Analytics/GetKPIs")]
        [HttpGet]
        public IHttpActionResult GetKPIs()
        {
            dynamic KPIS = new ExpandoObject();
            var data = _dbContext.Variables.Where(v => v.VariableType == 0).Select(x => new { x.Id, x.DisplayName }).ToList();
            KPIS = data;
            return Ok(KPIS);
        }

        private DataTable CreateDataSets(List<Variable> inputVariables, Variable outputVariable, List<PredictiveAnalysis.Domain.Dataset> data)
        {
            DataTable table = new DataTable();
            DataColumn primaryKeyColumn = table.Columns.Add("DataSetId");
            string ruleId = string.Empty;
            table.PrimaryKey = new DataColumn[] { primaryKeyColumn };
            foreach (var item in inputVariables)
            {
                table.Columns.Add(item.DisplayName);
                 ruleId = ConfigurationManager.AppSettings[item.Id.ToString()];
                
                
            }
            table.Columns.Add(outputVariable.DisplayName);

            foreach (PredictiveAnalysis.Domain.Dataset item in data)
            {
                string rowId = String.Format("{0}{1}{2}{3}", item.Customer.DisplayName, item.Country.DisplayName, item.Year.ConvertTo<string>(), item.Month.ConvertTo<string>());
                DataRow row = table.Rows.Find(rowId);
                if (row == null)
                {
                    row = table.NewRow();
                    row[primaryKeyColumn] = rowId;
                    table.Rows.Add(row);
                }
                row[item.Variable.DisplayName] = item.DataValue.ConvertTo<float>(0);
            }

            //PopulateOutputAssumptions(ref table);
           // CacheManager.GetItem(CacheKeys.Rule).ConvertTo<string>()
            DetermineOutputValues(ref table,ruleId , inputVariables, outputVariable.DisplayName);
            return table;
        }

        //private DataTable CreateDataSetsForStock()
        //{
        //    DataTable table = new DataTable();
        //    DataColumn primaryKeyColumn = table.Columns.Add("StockId");
        //    string ruleId = string.Empty;
        //    table.PrimaryKey = new DataColumn[] { primaryKeyColumn };
        //    foreach (var item in inputVariables)
        //    {
        //        table.Columns.Add(item.DisplayName);
        //        ruleId = ConfigurationManager.AppSettings[item.Id.ToString()];
        //    }
        //    table.Columns.Add(outputVariable.DisplayName);
        //    foreach (PredictiveAnalysis.Domain.Dataset item in data)
        //    {
        //        string rowId = String.Format("{0}{1}{2}{3}", item.Customer.DisplayName, item.Country.DisplayName, item.Year.ConvertTo<string>(), item.Month.ConvertTo<string>());
        //        DataRow row = table.Rows.Find(rowId);
        //        if (row == null)
        //        {
        //            row = table.NewRow();
        //            row[primaryKeyColumn] = rowId;
        //            table.Rows.Add(row);
        //        }
        //        row[item.Variable.DisplayName] = item.DataValue.ConvertTo<float>(0);
        //    }
        //    DetermineOutputValues(ref table, ruleId, inputVariables, outputVariable.DisplayName);
        //    return table;
        //}
        [HttpPost]
        [Route("api/Analytics/GetChurnRate")]
        public IHttpActionResult GetChurnRate([FromBody] SaveModal data)
        {
            List<int> variableIds = new List<int>();
            PostData serializedData = JsonConvert.DeserializeObject<PostData>(data.DATA);
            dynamic result = ComputeChurnRate(serializedData);
            return Ok(result.Content);
        }

        private object ComputeChurnRate(PostData data)
        {
            dynamic customerAnnualInfo = new ExpandoObject();
            List<double> averageResultPerMonth = new List<double>();
            List<DataRow> list = new List<DataRow>();
            double[][] inputs;
            double[] outputs;
            string outputVariableName = CacheManager.GetItem(CacheKeys.OutputVariable).ConvertTo<string>();
            List<Variable> inputVariables = _dbContext.Variables.Where(v => v.VariableType == 0 && data.Inputs.Contains(v.Id)).ToList();
            Variable outputVariable = _dbContext.Variables.Where(v => v.VariableType == 1 && v.DisplayName == outputVariableName).Distinct().SingleOrDefault();
            SortedDictionary<string, double> monthWiseInfo = new SortedDictionary<string, double>();
            var months = _dbContext.Datasets.Select(x => x.Month).Distinct().ToList();
            List<PredictiveAnalysis.Domain.Dataset> datasets = GetDatasets(data);

            List<double> actualOutput = new List<double>();
            List<double> expectedOutput = new List<double>(); 

            if (data.Timeline == "Monthly")
            {
                foreach (var item in months)
                {
                    var month = ConfigurationManager.AppSettings[item];
                    List<PredictiveAnalysis.Domain.Dataset> monthDataset = datasets.Where(ds => ds.Month == item).ToList();
                    if (monthDataset.Count > 0)
                    {
                        DataTable table = CreateDataSets(inputVariables, outputVariable, monthDataset);
                        string[] independantVariables = inputVariables.Select(v => v.DisplayName).ToArray();
                        string dependantVariable = outputVariable.DisplayName;

                        DataTable independent = table.DefaultView.ToTable(false, independantVariables);
                        DataTable dependent = table.DefaultView.ToTable(false, dependantVariable);
                        inputs = independent.ToJagged();
                        outputs = (dependent.Columns[dependantVariable].ToArray());
                        LogisticRegressionAnalysis lra = new LogisticRegressionAnalysis()
                        {
                            Inputs = independantVariables,
                            Output = dependantVariable
                        };
                        LogisticRegression lr = lra.Learn(inputs, outputs);
                        var probabilities = lr.Probability(inputs);
                        actualOutput.AddRange(probabilities);
                        expectedOutput.AddRange(outputs);
                        var result = probabilities.Average();
                        averageResultPerMonth.Add(result);
                        string json = JsonConvert.SerializeObject(table, Formatting.Indented);

                        foreach (DataRow dr in table.Rows)
                        {
                            list.Add(dr);
                        }

                        monthWiseInfo.Add(month, result);
                        customerAnnualInfo.testData = list;
                    }
                }
            }
            else
            {
                DataTable table = CreateDataSets(inputVariables, outputVariable, datasets);
                string[] independantVariables = inputVariables.Select(v => v.DisplayName).ToArray();
                string dependantVariable = outputVariable.DisplayName;

                DataTable independent = table.DefaultView.ToTable(false, independantVariables);
                DataTable dependent = table.DefaultView.ToTable(false, dependantVariable);
                inputs = independent.ToJagged();
                outputs = (dependent.Columns[dependantVariable].ToArray());
                LogisticRegressionAnalysis lra = new LogisticRegressionAnalysis()
                {
                    Inputs = independantVariables,
                    Output = dependantVariable
                };
                LogisticRegression lr = lra.Learn(inputs, outputs);
                var probabilities = lr.Probability(inputs);
                var result = probabilities.Average();
                averageResultPerMonth.Add(result);
                string json = JsonConvert.SerializeObject(table, Formatting.Indented);

                foreach (DataRow dr in table.Rows)
                {
                    list.Add(dr);
                }
                customerAnnualInfo.testData = list;
            }

            var finalResult = averageResultPerMonth.Average();
            customerAnnualInfo.churnRate = finalResult;
            customerAnnualInfo.xAxisDataPoints = averageResultPerMonth;
            customerAnnualInfo.monthWiseData = monthWiseInfo;
            dynamic stateData = ComputeStateWiseChurnRate(data);
            customerAnnualInfo.stateData = stateData.Content;
            customerAnnualInfo.validateModel = confusionMatrix(expectedOutput.ToArray(), actualOutput.ToArray());
            //dynamic NPLInfo = NPLResults();
            //customerAnnualInfo.CustomerNPLInfo = NPLInfo;
            return Ok(customerAnnualInfo);
        }

        //private object ComputeChurnRatePerStock(PostData data)
        //{
        //    dynamic customerAnnualInfo = new ExpandoObject();
        //    List<double> averageResultPerMonth = new List<double>();
        //    List<DataRow> list = new List<DataRow>();
        //    double[][] inputs;
        //    double[] outputs;
        //    string outputVariableName = CacheManager.GetItem(CacheKeys.OutputVariable).ConvertTo<string>();
        //    List<Variable> inputVariables = _dbContext.Variables.Where(v => v.VariableType == 0 && data.Inputs.Contains(v.Id)).ToList();
        //    Variable outputVariable = _dbContext.Variables.Where(v => v.VariableType == 1 && v.DisplayName == outputVariableName).Distinct().SingleOrDefault();
        //    SortedDictionary<string, double> monthWiseInfo = new SortedDictionary<string, double>();
        //    var months = _dbContext.Datasets.Select(x => x.Month).Distinct().ToList();
        //    List<PredictiveAnalysis.Domain.StockData> datasets = GetStockDatasets(data);

        //    List<double> actualOutput = new List<double>();
        //    List<double> expectedOutput = new List<double>();

        //    if (data.Timeline == "Monthly")
        //    {
        //        foreach (var item in months)
        //        {
        //            var month = ConfigurationManager.AppSettings[item];
        //            List<PredictiveAnalysis.Domain.StockData> monthDataset = datasets.Where(ds => ds.Month == item).ToList();
        //            if (monthDataset.Count > 0)
        //            {
        //                DataTable table = CreateDataSets(inputVariables, outputVariable, monthDataset);
        //                string[] independantVariables = inputVariables.Select(v => v.DisplayName).ToArray();
        //                string dependantVariable = outputVariable.DisplayName;

        //                DataTable independent = table.DefaultView.ToTable(false, independantVariables);
        //                DataTable dependent = table.DefaultView.ToTable(false, dependantVariable);
        //                inputs = independent.ToJagged();
        //                outputs = (dependent.Columns[dependantVariable].ToArray());
        //                LogisticRegressionAnalysis lra = new LogisticRegressionAnalysis()
        //                {
        //                    Inputs = independantVariables,
        //                    Output = dependantVariable
        //                };
        //                LogisticRegression lr = lra.Learn(inputs, outputs);
        //                var probabilities = lr.Probability(inputs);
        //                actualOutput.AddRange(probabilities);
        //                expectedOutput.AddRange(outputs);
        //                var result = probabilities.Average();
        //                averageResultPerMonth.Add(result);
        //                string json = JsonConvert.SerializeObject(table, Formatting.Indented);

        //                foreach (DataRow dr in table.Rows)
        //                {
        //                    list.Add(dr);
        //                }

        //                monthWiseInfo.Add(month, result);
        //                customerAnnualInfo.testData = list;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        DataTable table = CreateDataSets(inputVariables, outputVariable, datasets);
        //        string[] independantVariables = inputVariables.Select(v => v.DisplayName).ToArray();
        //        string dependantVariable = outputVariable.DisplayName;

        //        DataTable independent = table.DefaultView.ToTable(false, independantVariables);
        //        DataTable dependent = table.DefaultView.ToTable(false, dependantVariable);
        //        inputs = independent.ToJagged();
        //        outputs = (dependent.Columns[dependantVariable].ToArray());
        //        LogisticRegressionAnalysis lra = new LogisticRegressionAnalysis()
        //        {
        //            Inputs = independantVariables,
        //            Output = dependantVariable
        //        };
        //        LogisticRegression lr = lra.Learn(inputs, outputs);
        //        var probabilities = lr.Probability(inputs);
        //        var result = probabilities.Average();
        //        averageResultPerMonth.Add(result);
        //        string json = JsonConvert.SerializeObject(table, Formatting.Indented);

        //        foreach (DataRow dr in table.Rows)
        //        {
        //            list.Add(dr);
        //        }
        //        customerAnnualInfo.testData = list;
        //    }

        //    var finalResult = averageResultPerMonth.Average();
        //    customerAnnualInfo.churnRate = finalResult;
        //    customerAnnualInfo.xAxisDataPoints = averageResultPerMonth;
        //    customerAnnualInfo.monthWiseData = monthWiseInfo;
        //    dynamic stateData = ComputeStateWiseChurnRate(data);
        //    customerAnnualInfo.stateData = stateData.Content;
        //    customerAnnualInfo.validateModel = confusionMatrix(expectedOutput.ToArray(), actualOutput.ToArray());
        //    //dynamic NPLInfo = NPLResults();
        //    //customerAnnualInfo.CustomerNPLInfo = NPLInfo;
        //    return Ok(customerAnnualInfo);
        //}

        private object ComputeStateWiseChurnRate(PostData data)
        {
            dynamic customerAnnualInfo = new ExpandoObject();
            List<double> averageResultPerState = new List<double>();
            List<DataRow> list = new List<DataRow>();
            double[][] inputs;
            double[] outputs;
            //string[] inputVariableList = data.Inputs.ConvertTo<string>().Split(Literals.Comma.ToCharArray());
            //string[] inputVariableList = CacheManager.GetItem(CacheKeys.IndependantVariables).ConvertTo<string>().Split(Literals.Comma.ToCharArray());
            string outputVariableName = CacheManager.GetItem(CacheKeys.OutputVariable).ConvertTo<string>();
            List<Variable> inputVariables = _dbContext.Variables.Where(v => v.VariableType == 0 && data.Inputs.Contains(v.Id)).ToList();
            Variable outputVariable = _dbContext.Variables.Where(v => v.VariableType == 1 && v.DisplayName == outputVariableName).Distinct().SingleOrDefault();
            var countryIds = _dbContext.Countries.Select(x => x.Id).Distinct().ToList();
            Dictionary<int, double> stateWiseInfo = new Dictionary<int, double>();
            foreach (var item in countryIds)
            {
                List<PredictiveAnalysis.Domain.Dataset> datasets = new List<PredictiveAnalysis.Domain.Dataset>();
                if (data.CustomerName.Length>0)
                datasets = _dbContext.Datasets.Where(ds => data.Inputs.Contains(ds.VariableId) && ds.Customer.DisplayName.ToLower() == data.CustomerName.ToLower() && ds.Year == data.Value && ds.CountryId == item).ToList();
                else
                    datasets = _dbContext.Datasets.Where(ds => data.Inputs.Contains(ds.VariableId) && ds.Year == data.Value && ds.CountryId == item).ToList();
                if (datasets.Count > 0)
                {
                    DataTable dataSets = CreateDataSets(inputVariables, outputVariable, datasets);
                    string[] independantVariables = inputVariables.Select(v => v.DisplayName).ToArray();
                    string dependantVariable = outputVariable.DisplayName;

                    DataTable independent = dataSets.DefaultView.ToTable(false, independantVariables);
                    DataTable dependent = dataSets.DefaultView.ToTable(false, dependantVariable);
                    inputs = independent.ToJagged();
                    outputs = (dependent.Columns[dependantVariable].ToArray());
                    LogisticRegressionAnalysis lra = new LogisticRegressionAnalysis()
                    {
                        Inputs = independantVariables,
                        Output = dependantVariable
                    };
                    LogisticRegression lr = lra.Learn(inputs, outputs);
                    var probabilities = lr.Probability(inputs);
                    var result = probabilities.Average();
                    averageResultPerState.Add(result);
                    string json = JsonConvert.SerializeObject(dataSets, Formatting.Indented);

                    foreach (DataRow dr in dataSets.Rows)
                    {
                        list.Add(dr);
                    }
                    stateWiseInfo.Add(item, result);

                }
                //  customerAnnualInfo.testData = list;
            }

            var finalResult = averageResultPerState.Average();
            //customerAnnualInfo.churnRate = finalResult;
            //customerAnnualInfo.xAxisDataPoints = averageResultPerState;
            customerAnnualInfo.StateWiseChurn = stateWiseInfo;
            return Ok(customerAnnualInfo);
        }

        private List<Dataset> GetDatasets(PostData data)
        {
            List<Dataset> datasets = null;
            if (!String.IsNullOrWhiteSpace(data.CustomerName) && (!String.IsNullOrWhiteSpace(data.Value) && data.ValueFilter == "Yearly"))
                //datasets = _dbContext.Datasets.Where(ds => data.Inputs.Contains(ds.VariableId) && ds.Customer.DisplayName.ToLower() == data.CustomerName.ToLower() && ds.Year == data.Value).ToList();
                datasets = _dbContext.Datasets.Where(ds => data.Inputs.Contains(ds.VariableId) && ds.Customer.DisplayName.ToLower() == data.CustomerName.ToLower() && ds.Year == data.Value).ToList();
            else if (!String.IsNullOrWhiteSpace(data.CustomerName) && (!String.IsNullOrWhiteSpace(data.Value) && data.ValueFilter == "Monthly"))
                datasets = _dbContext.Datasets.Where(ds => data.Inputs.Contains(ds.VariableId) && ds.Customer.DisplayName.ToLower() == data.CustomerName.ToLower() && ds.Month == data.Value).ToList();
            else if (!String.IsNullOrWhiteSpace(data.CustomerName))
                datasets = _dbContext.Datasets.Where(ds => data.Inputs.Contains(ds.VariableId) && ds.Customer.DisplayName.ToLower() == data.CustomerName.ToLower()).ToList();
            else if (String.IsNullOrWhiteSpace(data.CustomerName))
                datasets = _dbContext.Datasets.Where(ds => data.Inputs.Contains(ds.VariableId) && ds.Year == data.Value).ToList();
            return datasets;
        }

        private List<StockData> GetStockDatasets(PostData data)
        {
            List<StockData> datasets = null;
            datasets = _dbContext.StockDatas.Where(ds => ds.Year == "2012" && ds.Month == data.Value).ToList();
            return datasets;
        }



        private Object confusionMatrix(double[] expected, double[] predicted)
        {
            // The correct and expected output values (as confirmed by a Gold
            //  standard rule, actual experiment or true verification)
            int[] expectedOld = { 0, 0, 1, 0, 1, 0, 0, 0, 0, 0 };

            //// The values as predicted by the decision system or
            ////  the test whose performance is being measured.
            int[] predictedOld = { 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 };
            int[] expectedIntValues = new int[expected.Length];
            int[] predictedIntValues = new int[predicted.Length];

            for (int i = 0; i < expected.Length; i++)
            {
                var expectedItem = Math.Round(expected[i], 0);
                expectedIntValues[i] = Convert.ToInt16(expectedItem);
            }
            for (int i = 0; i < predicted.Length; i++)
            {
                var predictedIntItem = Math.Round(predicted[i], 0);
                predictedIntValues[i] = Convert.ToInt16(predictedIntItem);

            }
            // In this test, 1 means positive, 0 means negative
            int positiveValue = 1;
            int negativeValue = 0;

            // Create a new confusion matrix using the given parameters
           // ConfusionMatrix matrix = new ConfusionMatrix(predictedOld, expectedOld,   positiveValue, negativeValue);
             ConfusionMatrix matrix = new ConfusionMatrix(predictedIntValues, expectedIntValues, positiveValue, negativeValue);
            // ConfusionMatrixView m = new ConfusionMatrixView(matrix);
            return matrix;

        }

        private void DetermineOutputValues(ref DataTable table, string ruleId, List<Variable> inputVariables, string outputVariableName)
        {
            DatasetRule rule = _dbContext.DatasetRules.Where(dr => dr.RuleId == ruleId).SingleOrDefault();
            Expression expression = new Expression(rule.Expression);
            foreach (DataRow row in table.Rows)
            {
                expression.Parameters.Clear();
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (!inputVariables.Any(iv => iv.DisplayName.ToLower() == column.ColumnName.ToLower()))
                        continue;
                    string dataVariableName = inputVariables.Where(iv => iv.DisplayName.ToLower() == column.ColumnName.ToLower()).SingleOrDefault().DataVariable;
                    double variableValue = Math.Round(row[column.ColumnName].ConvertTo<float>(0), 2);
                    row[column.ColumnName] = variableValue;
                    expression.Parameters.Add(dataVariableName, variableValue);
                }
                row[outputVariableName] = expression.Evaluate();
            }
        }

        private void PopulateOutputAssumptions(ref DataTable data)
        {
            foreach (DataRow row in data.Rows)
            {
                float data1 = row["On Time Delivery"] == DBNull.Value ? 0 : row["On Time Delivery"].ConvertTo<float>();
                float data2 = row["Total Net Revenue"] == DBNull.Value ? 0 : row["Total Net Revenue"].ConvertTo<float>();
                row["On Time Delivery"] = data1;
                row["Total Net Revenue"] = data2;
                double onTimeDelivery = Math.Round(data1, 2);
                double revenue = Math.Round(data2, 2);
                if (onTimeDelivery >= 95 && revenue >= 1000000)
                    row["Churn Rate"] = 0;
                else
                    row["Churn Rate"] = 1;
            }
        }

        [Route("api/Analytics/NPLResults")]
        [HttpGet]
        public async Task<IHttpActionResult> NPLResults()
        {
            //TalkToWatson().Wait();
            await TalkToWatson();
            return Ok(AnalyticsController.NPLResult);
            //return "test";
        }

        private async Task TalkToWatson()
        {
            var baseurl = "https://gateway.watsonplatform.net/natural-language-understanding/api/v1/analyze?version=2017-02-27";
            var username = "5a0eb094-fde8-4d0f-89cf-cb5d35f2719b";
            var password = "OJ8pW2FgDhql";
            var context = null as object;
            // var input = Console.ReadLine();
            //input = new { text = input },

            var message = new
            {
                //text = "I still have a dream, a dream deeply rooted in the American dream – one day this nation will rise up and live up to its creed, We hold these truths to be self evident: that all men are created equal",
                url = "https://www.forbes.com/2005/01/17/0117mondaymatchup.html",
               // url= "https://uk.trustpilot.com/review/www.dhl.co.uk",
                features = "sentiment,keywords,emotion"
            };
            var test1 = new
            {
                keywords = new { sentiment = true }
            };
            var test = JsonConvert.SerializeObject(message);

            var resp = await baseurl
                //  .AppendPathSegments("v1", "workspaces", workspace, "message")
                //.SetQueryParam("text", "I still have a dream, a dream deeply rooted in the American dream – one day this nation will rise up and live up to its creed, We hold these truths to be self evident: that all men are created equal")
                .SetQueryParams(message)
                .WithBasicAuth(username, password)
                .AllowAnyHttpStatus()
                .PostJsonAsync(test1);

            var json = await resp.Content.ReadAsStringAsync();

            var answer = new
            {
                intents = default(object),
                entities = default(object),
                input = default(object),
                output = new
                {
                    text = default(string[])
                },
                context = default(object)
            };

            answer = JsonConvert.DeserializeAnonymousType(json, answer);

            var output = answer?.output?.text?.Aggregate(
                new StringBuilder(),
                (sb, l) => sb.AppendLine(l),
                sb => sb.ToString());
            AnalyticsController.NPLResult = JsonConvert.DeserializeObject(json);

            //return output;
        }

    }
}