import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AddressSelector from './AddressSelector';
import OrderSummary from './OrderSummary';
import { getAddresses } from '../reduxStore/addressSlice';
import { createOrder } from '../reduxStore/orderSlice';
import { useNavigate } from 'react-router-dom';

const Checkout = () => {

  const dispatch = useDispatch();
  const navigate = useNavigate();

  const [billingAddressId, setBillingAddressId] = useState(null);
  const [shippingAddressId, setShippingAddressId] = useState(null);

  const customerId = useSelector(state => state.customer.customer.CustomerId);
  const token = useSelector(state => state.customer.customer.Token);
  const cart = useSelector(state => state.cart.cart);
  const addresses = useSelector(state => state.address.addresses);


  useEffect(() => {
     if (customerId && token) {
       dispatch(getAddresses(customerId, token));
     }
  }, [customerId, token, dispatch]);

  const handlePlaceOrder = async () => {
    if (!billingAddressId || !shippingAddressId) {
      alert('Please select both billing and shipping addresses.');
      return;
    }

    const orderItems = cart.CartItems.map(item => ({
      ProductId: item.ProductId,
      Quantity: item.Quantity,
      Price: item.UnitPrice,
    }));

    var newOrder = {
    CustomerId: customerId,
        BillingAddressId: billingAddressId,
        ShippingAddressId: shippingAddressId,
        OrderItems: orderItems
    }
    dispatch(createOrder(newOrder,navigate,token))
  };

  return (
    <div className="p-4 sm:p-8 max-w-4xl mx-auto space-y-6">
      <h1 className="text-2xl font-bold text-center mb-6">Checkout</h1>

      <div className="bg-white rounded-md shadow p-4 sm:p-6">
        <h2 className="text-xl font-semibold mb-4">Select Address</h2>
        <AddressSelector
          addresses={addresses}
          onBillingChange={setBillingAddressId}
          onShippingChange={setShippingAddressId}
        />
      </div>

      <div className="bg-white rounded-md shadow p-4 sm:p-6">
        <h2 className="text-xl font-semibold mb-4">Order Summary</h2>
        <OrderSummary cart={cart} />
      </div>

      <button
        onClick={handlePlaceOrder}
        className="w-full bg-black hover:bg-gray-800 text-white font-medium py-3 px-6 rounded-md transition"
      >
        Place Order
      </button>
    </div>
  );
};

export default Checkout;
