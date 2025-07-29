import React, { useState } from 'react'
import { useDispatch, useSelector } from 'react-redux';
import { loginCustomer, setLoader } from '../reduxStore/customerSlice';
import Title from '../components/Title';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';

const Login = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    Email: "",
    Password: "",
    Role: "",
  });

  const isLoading = useSelector((state) => state.customer.loader);

  const handleChange = (e) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  const validateForm = () => {
    const { Email, Password, Role } = formData;

    if (!Email.trim()) {
      toast.error("Email is required.");
      return false;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(Email)) {
      toast.error("Invalid email format.");
      return false;
    }
    if (!Password || Password.length < 6) {
      toast.error("Password must be at least 6 characters.");
      return false;
    }
    if (!Role) {
      toast.error("Please select a role.");
      return false;
    }

    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) return;

    dispatch(setLoader(true));

    dispatch(loginCustomer(formData,navigate));
  };

  return (
    <div className="max-w-md mx-auto mt-10 p-6 border rounded-md shadow-md bg-white text-center py-8">
      <Title text1="Login your " text2="Account" />

      <form onSubmit={handleSubmit} className="space-y-4">
        <input
          name="Email"
          type="email"
          placeholder="Email"
          className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400 disabled:bg-gray-100"
          value={formData.Email}
          onChange={handleChange}
          disabled={isLoading}
          required
        />
        <input
          name="Password"
          type="password"
          placeholder="Password"
          className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400 disabled:bg-gray-100"
          value={formData.Password}
          onChange={handleChange}
          disabled={isLoading}
          required
        />
        <select
          id="role"
          name="Role"
          value={formData.Role}
          onChange={handleChange}
          disabled={isLoading}
          className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400 disabled:bg-gray-100"
        >
          <option value="">-- Select Role --</option>
          <option value="user">User</option>
          <option value="admin">Admin</option>
        </select>
        <button
          type="submit"
          disabled={isLoading}
          className={`w-full bg-gray-800 text-white py-2 rounded flex justify-center items-center gap-2 transition ${
            isLoading ? 'opacity-50 cursor-not-allowed' : 'hover:bg-gray-900'
          }`}
        >
          {isLoading && (
            <span className="animate-spin rounded-full h-4 w-4 border-t-2 border-white border-solid"></span>
          )}
          Login
        </button>
      </form>
    </div>
  );
};

export default Login;
