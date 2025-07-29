import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { addNewAddress, setAddress, updateAddress } from '../reduxStore/addressSlice';

const AddAddress = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const token = useSelector(state => state.customer.customer.Token);
  const customerId = useSelector(state => state.customer.customer.CustomerId);
  const address = useSelector(state => state.address.address);

  const [form, setForm] = useState({
    Id:0,
    AddressLine1: '',
    AddressLine2: '',
    City: '',
    State: '',
    PostalCode: '',
    Country: ''
  });
  
  useEffect(() => {
   if(address != null && address.Id > 0)
   {
    setForm(address);
   }
  },[])
  const handleChange = (e) => {
    console.log([e.target.name],e.target.value)
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    const payload = {
      CustomerId: customerId,
      ...form
    };
    
    dispatch(form.Id == 0 ? addNewAddress(payload, navigate, token) : updateAddress(payload,navigate,token));
    dispatch(setAddress(null));
  };

  return (
    <div className="p-6 sm:p-8 max-w-lg mx-auto bg-white rounded-md shadow-md">
      <h1 className="text-2xl font-bold mb-6 text-center">Add New Address</h1>

      <form onSubmit={handleSubmit} className="space-y-4">
        <input
          name="AddressLine1"
          value={form.AddressLine1}
          onChange={handleChange}
          placeholder="Address Line 1"
          required
          className="w-full border border-gray-300 px-4 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-black"
        />
        <input
          name="AddressLine2"
          value={form.AddressLine2}
          onChange={handleChange}
          placeholder="Address Line 2"
          required
          className="w-full border border-gray-300 px-4 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-black"
        />
        <input
          name="City"
          value={form.City}
          onChange={handleChange}
          placeholder="City"
          required
          className="w-full border border-gray-300 px-4 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-black"
        />
        <input
          name="State"
          value={form.State}
          onChange={handleChange}
          placeholder="State"
          required
          className="w-full border border-gray-300 px-4 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-black"
        />
        <input
          name="PostalCode"
          value={form.PostalCode}
          onChange={handleChange}
          placeholder="Postal Code"
          required
          className="w-full border border-gray-300 px-4 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-black"
        />
        <input
          name="Country"
          value={form.Country}
          onChange={handleChange}
          placeholder="Country"
          required
          className="w-full border border-gray-300 px-4 py-2 rounded-md focus:outline-none focus:ring-2 focus:ring-black"
        />

        <button
          type="submit"
          className="w-full bg-black hover:bg-gray-800 text-white font-medium py-3 px-6 rounded-md transition"
        >
          Save Address
        </button>
      </form>
    </div>
  );
};

export default AddAddress;
