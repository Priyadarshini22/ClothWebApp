using Dapper;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.DTOs.AddressesDTOs;

namespace ECommerceApp.Repository
{
    public interface IAddressRepository
    {
        Task<bool> CreateAddress(AddressCreateDTO addressDto);
        Task<AddressResponseDTO> GetAddressByIdAsync(int id);
        Task<bool> UpdateAddressAsync(AddressUpdateDTO addressDto);
        Task<bool> DeleteAddressAsync(AddressDeleteDTO addressDeleteDTO);
        Task<List<AddressResponseDTO>> GetAddressesByCustomerAsync(int customerId);
    }
    public class AddressRepository : IAddressRepository
    {
        private readonly IDapperDbConnection _context;

        public AddressRepository(IDapperDbConnection context)
        {
            _context = context;
        }

        public async Task<bool> CreateAddress(AddressCreateDTO addressDto)
        {
            var dbConnection = _context.CreateConnection();
            try
            {
                var sQuery = $"insert into Addresses values (@CustomerId,@AddressLine1,@AddressLine2,@City,@State,@PostalCode,@Country)";
                return await dbConnection.ExecuteAsync(sQuery, new
                {
                    addressDto.CustomerId,
                    addressDto.AddressLine1,
                    addressDto.AddressLine2,
                    addressDto.City,
                    addressDto.State,
                    addressDto.PostalCode,
                    addressDto.Country
                }) > 1;
            }
            catch
            {
                throw;
            }
        }

        public async Task<AddressResponseDTO> GetAddressByIdAsync(int id)
        {
            var dbConnection = _context.CreateConnection();
            try
            {
                var sQuery = $"select * from Addresses where ID = @id";
                return await dbConnection.QueryFirstAsync<AddressResponseDTO>(sQuery, new
                {
                   id
                }); 
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateAddressAsync(AddressUpdateDTO addressDto)
        {
            var dbConnection = _context.CreateConnection();
            try
            {
                var sQuery = $"update Addresses set AddressLine1 = @AddressLine1,AddressLine2 = @AddressLine2,City = @City,State = @State,PostalCode = @PostalCode,Country = @Country where ID = @ID";
                return await dbConnection.ExecuteAsync(sQuery, new
                {
                    addressDto.ID,
                    addressDto.AddressLine1,
                    addressDto.AddressLine2,
                    addressDto.City,
                    addressDto.State,
                    addressDto.PostalCode,
                    addressDto.Country
                }) > 1;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteAddressAsync(AddressDeleteDTO addressDeleteDTO)
        {
            var dbConnection = _context.CreateConnection();
            try
            {
                var sQuery = $"delete from Addresses where ID = @ID and CustomerId = @CustomerId";
                return await dbConnection.ExecuteAsync(sQuery, new
                {
                    ID = addressDeleteDTO.AddressId,
                    addressDeleteDTO.CustomerId,
                }) > 1;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<AddressResponseDTO>> GetAddressesByCustomerAsync(int customerId)
        {
            var dbConnection = _context.CreateConnection();
            try
            {
                var sQuery = $"select * from Addresses where CustomerId = @customerId";
                var result =  await dbConnection.QueryAsync<AddressResponseDTO>(sQuery, new
                {
                    customerId
                });
                return result.ToList();
            }
            catch
            {
                throw;
            }
        }
    }
}
