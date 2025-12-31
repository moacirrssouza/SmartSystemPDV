using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace SmartSystemPDV.Data
{
    public class DatabaseConnection
    {
        #region Campos Privados

        private readonly string _connectionString;

        #endregion

        #region Construtores

        public DatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' não encontrada.");
        }

        public DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        #endregion

        #region Conexão

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        #endregion

        #region Testes

        public bool TestarConexao()
        {
            try
            {
                using var connection = CreateConnection();
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}