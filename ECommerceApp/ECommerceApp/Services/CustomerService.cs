using Microsoft.EntityFrameworkCore;
using ECommerceApp.Data;
using ECommerceApp.DTOs.CustomerDTOs;
using ECommerceApp.Models;
using ECommerceApp.DTOs;
using ECommerceApp.Repository;

namespace ECommerceApp.Services
{
    public class CustomerService
    {
        private readonly IDapperDbConnection _context;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITokenService _tokenService;
        public CustomerService(IDapperDbConnection context, ICustomerRepository customerRepository, ITokenService tokenService)
        {
            _context = context;
            _customerRepository = customerRepository;
            _tokenService = tokenService;
        }

        public async Task<ApiResponse<bool>> RegisterCustomerAsync(CustomerRegistrationDTO customerDto)
        {
            try
            {
                // Check if email already exists
                if (await _customerRepository.CheckCustomerEmailExists(customerDto.Email ?? ""))
                {
                    return new ApiResponse<bool>(400, "Email is already in use.");
                }

                // Manual mapping from DTO to Model
                var customer = new Customer
                {
                    FirstName = customerDto.FirstName,
                    LastName = customerDto.LastName,
                    Email = customerDto.Email,
                    PhoneNumber = customerDto.PhoneNumber,
                    DateOfBirth = customerDto.DateOfBirth,
                    IsActive = true,
                    Role = customerDto.Role,
                    Password = BCrypt.Net.BCrypt.HashPassword(customerDto.Password)
                };

                var response = await _customerRepository.CreateNewCustomer(customer);

                return new ApiResponse<bool>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginAsync(LoginDTO loginDto)
        {
            try
            {
                var customer = await _customerRepository.GetExistingCustomer(loginDto.Email ?? "");

                if (customer == null)
                {
                    return new ApiResponse<LoginResponseDTO>(401, "Invalid email or password.");
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, customer.Password);
                if (!isPasswordValid)
                {
                    return new ApiResponse<LoginResponseDTO>(401, "Invalid email or password.");
                }

                var loginResponse = new LoginResponseDTO
                {
                    Message = "Login successful.",
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    Token = _tokenService.GenerateToken(loginDto),
                };

                return new ApiResponse<LoginResponseDTO>(200, loginResponse);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<LoginResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CustomerResponseDTO>> GetCustomerByIdAsync(int id)
        {
            try
            {
                var customer = await _customerRepository.GetCustomerDetailsById(id);

                if (customer == null)
                {
                    return new ApiResponse<CustomerResponseDTO>(404, "Customer not found.");
                }

                // Map to CustomerResponseDTO
                var customerResponse = new CustomerResponseDTO
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    DateOfBirth = customer.DateOfBirth
                };

                return new ApiResponse<CustomerResponseDTO>(200, customerResponse);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<CustomerResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateCustomerAsync(CustomerUpdateDTO customerDto)
        {
            try
            {
                var customer = await _customerRepository.GetCustomerDetailsById(customerDto.CustomerId);
                if (customer == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Customer not found.");
                }

                // Check if email is being updated to an existing one
                if (customer.Email != customerDto.Email && await _customerRepository.CheckCustomerEmailExists(customerDto.Email ?? ""))
                {
                    return new ApiResponse<ConfirmationResponseDTO>(400, "Email is already in use.");
                }

                // Update customer properties manually
                customer.FirstName = customerDto.FirstName;
                customer.LastName = customerDto.LastName;
                customer.Email = customerDto.Email;
                customer.PhoneNumber = customerDto.PhoneNumber;
                customer.DateOfBirth = customerDto.DateOfBirth;


                var result = await _customerRepository.UpdateCustomerDetails(customerDto);
                // Prepare confirmation message
                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Customer with Id {customerDto.CustomerId} updated successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await _customerRepository.DeleteCustomerDetails(id);

                // Prepare confirmation message
                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Customer with Id {id} deleted successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        // Changes the password for an existing customer.
        public async Task<ApiResponse<ConfirmationResponseDTO>> ChangePasswordAsync(ChangePasswordDTO changePasswordDto)
        {
            try
            {
                var customer = await _customerRepository.GetCustomerDetailsById(changePasswordDto.CustomerId);
                if (customer == null || !customer.IsActive)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Customer not found or inactive.");
                }

                bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, customer.Password);
                if (!isCurrentPasswordValid)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(401, "Current password is incorrect.");
                }

                customer.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = "Password changed successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }
    }
}