using System.Data;
using System.Dynamic;
using Microsoft.Data.SqlClient;

namespace ChatGPTClone.Infrastructure
{
    public class DatabaseRepository
    {
        private readonly string _connectionString;

        public DatabaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<dynamic> ExecuteCommand(string commandText/, Dictionary<string, object> parameters/)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand(commandText, connection);


                //foreach (var param in parameters)
                //{
                //    command.Parameters.AddWithValue(param.Key, param.Value);
                //}

                connection.Open();
                var dataTable = new DataTable();
                var dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(dataTable);

                return dataTable.Rows.Count > 0 ? ConvertDataRowsToDynamic(dataTable.Rows) : null;
            }
        }

        private List<dynamic> ConvertDataRowsToDynamic(DataRowCollection rows)
        {
            var dynamicList = new List<dynamic>();

            foreach (DataRow row in rows)
            {
                dynamic expando = new ExpandoObject();
                var expandoDict = (IDictionary<string, object>)expando;

                foreach (DataColumn column in row.Table.Columns)
                {
                    expandoDict[column.ColumnName] = row[column];
                }

                dynamicList.Add(expando);
            }

            return dynamicList;
        }
    }
}