import React,{ useState } from "react";
import Title from "../components/Title";
import { useDispatch } from "react-redux";
import { registerCustomer } from "../reduxStore/customerSlice";
import { useNavigate } from "react-router-dom";

const Register = () => {

  const dispatch = useDispatch();
  const [formData, setFormData] = useState({
    FirstName: "",
    LastName: "",
    Email: "",
    PhoneNumber: "",
    DateOfBirth: "",
    Password: "",
    Role: "",
  });
  const navigate = useNavigate();


  const handleChange = (e) => {
    setFormData(() => ({
      ...formData,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    dispatch(registerCustomer(formData,navigate))

  };

  return (
    <div className="max-w-md mx-auto mt-10 p-6 border rounded-md shadow-md bg-white text-center py-8">
      <Title text1="Create Your " text2="Account" />

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="flex gap-3">
          <input
            name="FirstName"
            type="text"
            placeholder="First Name"
            className="w-1/2 border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400"
            value={formData.FirstName}
            onChange={handleChange}
            required
          />
          <input
            name="LastName"
            type="text"
            placeholder="Last Name"
            className="w-1/2 border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400"
            value={formData.LastName}
            onChange={handleChange}
            required
          />
        </div>

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
          name="PhoneNumber"
          type="tel"
          placeholder="Phone Number"
          className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400"
          value={formData.PhoneNumber}
          onChange={handleChange}
          required
        />

        <input
          name="DateOfBirth"
          type="date"
          className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400 focus:border-gray-400"
          value={formData.DateOfBirth}
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
        <button type="submit" className="w-full bg-gray-800 text-white py-2 rounded hover:bg-gray-900">
          Register
        </button>
      </form>
    </div>
  );
};

export default Register;
