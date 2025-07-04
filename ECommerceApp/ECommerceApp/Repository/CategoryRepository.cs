using Dapper;
using ECommerceApp.Data;
using ECommerceApp.DTOs.CategoryDTOs;
using ECommerceApp.DTOs.CustomerDTOs;

namespace ECommerceApp.Repository
{

    public interface ICategoryRepository
    {
        Task<List<CategoryResponseDTO>> GetAllCategories();
    }
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDapperDbConnection _dbContext;

        public CategoryRepository(IDapperDbConnection dbContext) => _dbContext = dbContext;

        public async Task<List<CategoryResponseDTO>> GetAllCategories()
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var sQuery = $"select * from Categories";
            var result = await dbConnection.QueryAsync<CategoryResponseDTO>(sQuery);
            return result.ToList();
        }

    }
}
