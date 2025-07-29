import React, { useState } from "react";
import Title from "../components/Title";
import { useDispatch } from "react-redux";
import { registerCustomer } from "../reduxStore/customerSlice";
import { useNavigate } from "react-router-dom";

const Register = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    FirstName: "",
    LastName: "",
    Email: "",
    PhoneNumber: "",
    DateOfBirth: "",
    Password: "",
    Role: "",
  });

  const [errors, setErrors] = useState({});

  const handleChange = (e) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
    setErrors((prev) => ({ ...prev, [e.target.name]: "" })); // Clear error on change
  };

  const validate = () => {
    const newErrors = {};
    if (!formData.FirstName.trim()) newErrors.FirstName = "First name is required";
    if (!formData.LastName.trim()) newErrors.LastName = "Last name is required";
    if (!formData.Email) newErrors.Email = "Email is required";
    if (!formData.PhoneNumber) newErrors.PhoneNumber = "Phone number is required";
    if (!formData.DateOfBirth) newErrors.DateOfBirth = "Date of birth is required";
    if (!formData.Password || formData.Password.length < 8) {
      newErrors.Password = "Password must be at least 8 characters";
    }
    if (!formData.Role) newErrors.Role = "Please select a role";
    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    dispatch(registerCustomer(formData, navigate));
  };

  return (
    <div className="max-w-md mx-auto mt-10 p-6 border rounded-md shadow-md bg-white text-center py-8">
      <Title text1="Create Your " text2="Account" />

      <form onSubmit={handleSubmit} className="space-y-4 text-left">
        <div className="flex gap-3">
          <div className="w-1/2">
            <input
              name="FirstName"
              type="text"
              placeholder="First Name"
              className="w-full  border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400"
              value={formData.FirstName}
              onChange={handleChange}
            />
            {errors.FirstName && <p className="text-red-500 text-sm">{errors.FirstName}</p>}
          </div>
          <div className="w-1/2">
            <input
              name="LastName"
              type="text"
              placeholder="Last Name"
              className="w-full  border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400"
              value={formData.LastName}
              onChange={handleChange}
            />
            {errors.LastName && <p className="text-red-500 text-sm">{errors.LastName}</p>}
          </div>
        </div>

        <div>
          <input
            name="Email"
            type="email"
            placeholder="Email"
            className="w-full  border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400"
            value={formData.Email}
            onChange={handleChange}
          />
          {errors.Email && <p className="text-red-500 text-sm">{errors.Email}</p>}
        </div>

        <div>
          <input
            name="PhoneNumber"
            type="tel"
            placeholder="Phone Number"
            className="w-full  border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400"
            value={formData.PhoneNumber}
            onChange={handleChange}
          />
          {errors.PhoneNumber && <p className="text-red-500 text-sm">{errors.PhoneNumber}</p>}
        </div>

        <div>
          <input
            name="DateOfBirth"
            type="date"
            className="w-full  border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400"
            value={formData.DateOfBirth}
            onChange={handleChange}
          />
          {errors.DateOfBirth && <p className="text-red-500 text-sm">{errors.DateOfBirth}</p>}
        </div>

        <div>
          <input
            name="Password"
            type="password"
            placeholder="Password"
            className="w-full  border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400"
            value={formData.Password}
            onChange={handleChange}
          />
          {errors.Password && <p className="text-red-500 text-sm">{errors.Password}</p>}
        </div>

        <div>
          <select
            id="role"
            name="Role"
            value={formData.Role}
            onChange={handleChange}
            className="w-full border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-1 focus:ring-gray-400"
          >
            <option value="">-- Select Role --</option>
            <option value="user">User</option>
            <option value="admin">Admin</option>
          </select>
          {errors.Role && <p className="text-red-500 text-sm">{errors.Role}</p>}
        </div>

        <button
          type="submit"
          className="w-full bg-gray-800 text-white py-2 rounded hover:bg-gray-900"
        >
          Register
        </button>
      </form>
    </div>
  );
};

export default Register;
