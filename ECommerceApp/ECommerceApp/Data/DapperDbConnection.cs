using ECommerceApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;
namespace ECommerceApp.Data
{
    // Database context for the application

    public interface IDapperDbConnection
    {
        IDbConnection CreateConnection();

    }
    public class DapperDbConnection : IDapperDbConnection
    {
        private readonly IConfiguration _configuration;

        public DapperDbConnection(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("EFCoreDBConnection"));
        }
    }
}