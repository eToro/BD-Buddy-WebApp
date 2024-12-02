using System.Data;
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

        public DataRow ExecuteCommand(string commandText/*, Dictionary<string, object> parameters*/)
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

                return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
            }
        }
    }
}
