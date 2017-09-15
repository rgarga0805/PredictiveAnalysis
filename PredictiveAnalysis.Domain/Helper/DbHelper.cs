using HL.Domain.Common;
using HL.Domain.Model;
using HL.Domain.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Text;

namespace HL.Domain.Helper
{
    public enum DatabaseSource
    {
        Salesforce,
        Wpf
    }

    public class DbHelper
    {
        private readonly DbContext _dbContext;
        private readonly SqlConnection dbConnection;

        public DbHelper(string connectionString)
        {
            dbConnection = new SqlConnection(connectionString);
            dbConnection.Open();
        }

        public IEnumerable<T> ExecuteProcedure<T>(string name)
        {
            return _dbContext.Database.SqlQuery<T>(name);
        }

        public DbResponse ExecuteProcedure(string name, List<DbParameter> parameters)
        {
            DbResponse response = new DbResponse();
            using (var cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = name;
                if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (var item in parameters)
                {
                    SqlParameter dbParameter = cmd.CreateParameter();
                    dbParameter.ParameterName = item.Name;
                    dbParameter.Value = item.Value;
                    cmd.Parameters.Add(dbParameter);
                    //cmd.Parameters.AddWithValue("@Number", item.Value);
                }
                //var row = new ExpandoObject() as IDictionary<string, object>;
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Record record = new Record();
                        for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                        {
                            record.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);
                        }
                        response.Records.Add(record);
                    }
                }
                return response;
            }
        }

        public DataTable ExecuteQuery(string query)
        {
            DataTable dataTable = new DataTable();
            SqlDataAdapter sqlAdapter = null;
            using (var cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                sqlAdapter = new SqlDataAdapter(cmd);
                sqlAdapter.Fill(dataTable);
            }
            return dataTable;
        }

        public void ExecuteQuery(string query, List<DbParameter> parameters)
        {
            _dbContext.Database.ExecuteSqlCommand(query, parameters);
        }

        private string PrepareQuery(string query, List<DbParameter> parameters)
        {
            StringBuilder text = new StringBuilder();
            text.Append(query);
            text.Append(Characters.Space);
            foreach (var item in parameters)
            {
                text.Append(Characters.At);
                text.Append(item.Name);
                text.Append(Characters.Comma);
                text.Append(Characters.Space);
            }

            text.Remove(text.Length - 1, 1);

            return text.ToString();
        }

        private List<SqlParameter> PrepareParameters(List<DbParameter> parameters)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            foreach (var item in parameters)
            {
                sqlParameters.Add(new SqlParameter() { ParameterName = item.Name, Value = item.Value });
            }

            return sqlParameters;
        }
    }
}