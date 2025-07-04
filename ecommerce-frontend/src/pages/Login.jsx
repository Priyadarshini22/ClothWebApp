import React, { useState } from 'react'
import { useDispatch, useSelector } from 'react-redux';
import { loginCustomer, setLoader } from '../reduxStore/customerSlice';
import Title from '../components/Title';
import { useNavigate } from 'react-router-dom';

const Login = () => {
  	

  const navigate = useNavigate();
// Response body
// Download
// {
//   "StatusCode": 200,
//   "Success": true,
//   "Data": {
//     "Id": 2,
//     "FirstName": "Priya",
//     "LastName": "Darshini",
//     "Email": "priya@example.com",
//     "PhoneNumber": "8825662728",
//     "DateOfBirth": "2025-05-22T11:12:08.683Z"
//   },

  const dispatch = useDispatch();
  const [formData, setFormData] = useState({
    Email: "",
    Password: "",
    Role: "",
  });
  const isLoading = useSelector((state) => state.customer.loader);
  const handleChange = async(e) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    dispatch(setLoader(true));
    if(await dispatch(loginCustomer(formData))) navigate('/')
  };

  return  (
    <div className="max-w-md mx-auto mt-10 p-6 border rounded-md shadow-md bg-white text-center py-8">
      <Title text1="Login your " text2="Account" />

      <form onSubmit={handleSubmit} className="space-y-4">
        <input
          name="Email"
          type="email"
          placeholder="Email"
          className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400"
          value={formData.Email}
          onChange={handleChange}
          required
        />
        <input
          name="Password"
          type="password"
          placeholder="Password"
          className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400"
          value={formData.Password}
          onChange={handleChange}
          required
        />
        <select id="role" name="Role" value={formData.Role} onChange={handleChange} className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400">
          <option value="">-- Select Role --</option>
          <option value="user" className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400">User</option>
          <option value="admin">Admin</option>
        </select>
        <button type="submit" className="w-full bg-gray-800 text-white py-2 rounded hover:bg-gray-900" load>
          {isLoading && (
        <span className="animate-spin rounded-full h-4 w-4 border-t-2 border-white border-solid"></span>
          )}
          Login
        </button>
      </form>
    </div>
  
  )
}

export default Login