import React from "react";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import { getAddressById } from "../reduxStore/addressSlice";

const AddressSelector = ({ addresses, onBillingChange, onShippingChange }) => {
  const navigate = useNavigate();
  const dispatch = useDispatch();

    const token = useSelector(state => state.customer.customer.Token);


  return (
    <div className="mb-6 space-y-6">

      <div>
        <h2 className="font-semibold mb-2">Billing Address</h2>
        {Array.isArray(addresses) && addresses.length > 0 ? (
          addresses.map((addr) => (
            <label
              key={`billing-${addr.Id}`}
              className="flex items-start gap-2 mb-2 cursor-pointer"
            >
              <input
                type="radio"
                name="billingAddress"
                value={addr.Id}
                onChange={() => onBillingChange(addr.Id)}
              />
              <div className="flex justify-between gap-4">
                <div>
                <div>{addr.AddressLine1}, {addr.AddressLine2}</div>
                <div>{addr.City}, {addr.State}, {addr.PostalCode}</div>
                </div>
                <button
                  className="text-blue-600 text-sm mt-1"
                  onClick={(e) => {
                    e.stopPropagation();
                    dispatch(getAddressById(addr.Id,token, navigate));
                  }}
                >
                  ✏️ 
                </button>
              </div>
            </label>
          ))
        ) : (
          <div className="text-gray-500">No addresses available.</div>
        )}
      </div>

      <div>
        <h2 className="font-semibold mb-2">Shipping Address</h2>
        {Array.isArray(addresses) && addresses.length > 0 ? (
          addresses.map((addr) => (
            <label
              key={`shipping-${addr.Id}`}
              className="flex items-start gap-2 mb-2 cursor-pointer"
            >
              <input
                type="radio"
                name="shippingAddress"
                value={addr.Id}
                onChange={() => onShippingChange(addr.Id)}
              />
              <div className="flex">
                <div>
                <div>{addr.AddressLine1}, {addr.AddressLine2}</div>
                <div>{addr.City}, {addr.State}, {addr.PostalCode}</div>
                </div>
                <button
                  className="text-blue-600 text-sm mt-1"
                  onClick={(e) => {
                    e.stopPropagation();
                    dispatch(getAddressById(addr.Id,token, navigate));
                  }}
                >
                  ✏️
                </button>
              </div>
            </label>
          ))
        ) : (
          <div className="text-gray-500">No addresses available.</div>
        )}
      </div>

      <div>
        <button
          onClick={() => navigate('/add-address')}
          className="flex items-center gap-2 text-gray-700 bg-gray-100 hover:bg-gray-200 font-medium py-2 px-4 rounded shadow-sm transition duration-200"
        >
          ➕ Add New Address
        </button>
      </div>
    </div>
  );
};

export default AddressSelector;
