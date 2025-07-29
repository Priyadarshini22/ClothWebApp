using Microsoft.EntityFrameworkCore;
using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.DTOs;
using ECommerceApp.DTOs.AddressesDTOs;
using ECommerceApp.Repository;

namespace ECommerceApp.Services
{
    public class AddressService
    {
        private readonly IAddressRepository _addressRepository;

        public AddressService(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<ApiResponse<bool>> CreateAddressAsync(AddressCreateDTO addressDto)
        {
           return new ApiResponse<bool>(200, _addressRepository.CreateAddress(addressDto).Result);
        }

        public async Task<ApiResponse<AddressResponseDTO>> GetAddressByIdAsync(int id)
        { 
           return new ApiResponse<AddressResponseDTO>(200, await _addressRepository.GetAddressByIdAsync(id));
        }

        public async Task<ApiResponse<bool>> UpdateAddressAsync(AddressUpdateDTO addressDto)
        {
           return new ApiResponse<bool>(200, await _addressRepository.UpdateAddressAsync(addressDto));
        }

        public async Task<ApiResponse<bool>> DeleteAddressAsync(AddressDeleteDTO addressDeleteDTO)
        {
           return new ApiResponse<bool>(200, await _addressRepository.DeleteAddressAsync(addressDeleteDTO));
        }

        public async Task<ApiResponse<List<AddressResponseDTO>>> GetAddressesByCustomerAsync(int customerId)
        {  
                return new ApiResponse<List<AddressResponseDTO>>(200, await _addressRepository.GetAddressesByCustomerAsync(customerId));
        }
    }
}