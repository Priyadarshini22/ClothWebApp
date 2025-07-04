using Dapper;
using ECommerceApp.Data;
using ECommerceApp.DTOs.CustomerDTOs;
using ECommerceApp.Models;

namespace ECommerceApp.Repository
{
    public interface ICustomerRepository
    {
        Task<bool> CheckCustomerEmailExists(string email);
        Task<bool> CreateNewCustomer(Customer customerRegistrationDTO);
        Task<LoginResponseDTO> GetExistingCustomer(string Email);
        Task<CustomerResponseDTO> GetCustomerDetailsById(int id);
        Task<bool> UpdateCustomerDetails(CustomerUpdateDTO customerDto);
        Task<bool> DeleteCustomerDetails(int id);
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDapperDbConnection _dbContext;

        public CustomerRepository(IDapperDbConnection dbContext) => _dbContext = dbContext;

        public async Task<bool> CheckCustomerEmailExists(string email)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sQuery = $"select * from Customers where Email = @email";
           var result = await  dbConnection.QueryAsync(sQuery, new { email });

            return result.Count()>=1 ? true : false;
        }

        public async Task<bool> CreateNewCustomer(Customer customerDTO)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();

            var sQuery = $"insert into Customers values (@FirstName, @LastName, @Email, @PhoneNumber, @DateOfBirth, @Password, @IsActive, @Role)";
            var result = await dbConnection.ExecuteAsync(sQuery, new 
            { 
                customerDTO.FirstName,
                customerDTO.LastName,
                customerDTO.Email,
                customerDTO.PhoneNumber,
                customerDTO.DateOfBirth,
                customerDTO.Password,
                IsActive = true,
                customerDTO.Role,
            });

            return result>= 1 ? true : false;
        }

        public async Task<LoginResponseDTO> GetExistingCustomer(string Email)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var sQuery = $"select * from Customers where Email = @Email";
            var result = await dbConnection.QueryFirstAsync(sQuery, new { Email });

            var loginResponse = new LoginResponseDTO
            {
                CustomerId = result.Id,
                CustomerName = result.FirstName + " " +result.LastName,
                Token = "",
                Role = result.Role,
                Password = result.Password,
                Message = "",
            };

            return loginResponse;
        }

        public async Task<CustomerResponseDTO> GetCustomerDetailsById(int id)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var sQuery = $"select * from Customers where ID = @id";
            var result = await dbConnection.QueryFirstAsync(sQuery, new { id });
            var customerResponse = new CustomerResponseDTO
            {
                Id = result.Id,
                FirstName = result.FirstName,
                LastName = result.LastName,
                Email = result.Email,
                PhoneNumber = result.PhoneNumber,
                DateOfBirth = result.DateOfBirth
            };
            return customerResponse;

        }

        public async Task<bool> UpdateCustomerDetails(CustomerUpdateDTO customerDto)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var sQuery = $"Update table Customers set FirstName = @FirstName, LastName = @LastName, Email=@Email, DateOfBirth=@DateOfBirth, PhoneNumber=@PhoneNumber";
            var result = await dbConnection.ExecuteAsync(sQuery, 
             new { 
                 customerDto.FirstName,
                 customerDto.LastName,
                 customerDto.Email,
                 customerDto.DateOfBirth,
                 customerDto.PhoneNumber,
            });
            return result >= 1 ? true : false;
        }

        public async Task<bool> DeleteCustomerDetails(int id)
        {
            using var dbConnection = _dbContext.CreateConnection();
            dbConnection.Open();
            var sQuery = $"Delete * from Customers where ID=@id";
            var result = await dbConnection.ExecuteAsync(sQuery, new { id });
            return result >= 1 ? true : false;
        }
    }
}
