import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Title from '../components/Title';
import { getCustomerDetails } from '../reduxStore/customerSlice';

const Profile = () => {
  const dispatch = useDispatch();
  const customerId = useSelector((state) => state.customer.customer.CustomerId);
  const customer = useSelector((state) => state.customer.profile);

  useEffect(() => {
    if (customerId) {
      dispatch(getCustomerDetails(customerId));
    }
  }, [dispatch, customerId]);

  if (!customer) {
    return <p className="text-center mt-10 text-gray-600">No customer information available.</p>;
  }

  return (
    <div className="max-w-xl mx-auto mt-10 p-6 bg-white border border-gray-200 rounded-md shadow-sm">
      <Title text1="User " text2="Profile" />

      <div className="mt-6 divide-y divide-gray-200">
        {[
          { label: 'First Name', value: customer.FirstName },
          { label: 'Last Name', value: customer.LastName },
          { label: 'Email', value: customer.Email },
          { label: 'Phone', value: customer.PhoneNumber },
          { label: 'Date of Birth', value: customer.DateOfBirth },
        ].map((field, index) => (
          <div key={index} className="py-3 flex justify-between text-gray-700 text-sm sm:text-base">
            <span className="font-medium text-gray-500">{field.label}:</span>
            <span className="text-right font-medium">{field.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Profile;
