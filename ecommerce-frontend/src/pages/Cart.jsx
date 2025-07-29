import React, { useEffect } from 'react';
import Title from '../components/Title';
import { useDispatch, useSelector } from 'react-redux';
import { getCartByCustomerId } from '../reduxStore/cartSlice';
import CartItem from '../components/CartItem';
import { useNavigate } from 'react-router-dom';

const Cart = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const token = useSelector((state) => state.customer.customer.Token);
  const customerId = useSelector((state) => state.customer.customer.CustomerId);
  const cart = useSelector((state) => state.cart.cart);

  useEffect(() => {
    if (customerId && token) {
      dispatch(getCartByCustomerId(customerId, token));
    }
  }, [customerId, token]);

  if (!cart?.CartItems?.length || cart.CartItems[0].Id == 0) return (
    <div className="p-6 text-center text-xl font-semibold text-gray-600">
      Your cart is empty.
    </div>
  );

  const subtotal = cart.CartItems.reduce((sum, item) => sum + item.TotalPrice, 0);
  const tax = +(subtotal * 0.02).toFixed(2);
  const deliveryCharge = 100;
  const total = subtotal + tax + deliveryCharge;

  return (
    <div className="p-4 sm:p-8 max-w-4xl mx-auto">
      <Title text1="YOUR" text2="CART" />

      <div className="space-y-6 mt-6">
        {cart.CartItems.map((item) => (
          <CartItem key={item.Id} item={item} />
        ))}
      </div>

      <hr className="my-8 border-gray-300" />

      <div className="bg-white rounded-md shadow-md p-6 w-full sm:w-1/2 ml-auto space-y-3 text-right">
        <div className="text-lg">
          <p className="flex justify-between">
            <span className="text-gray-700">Subtotal:</span>
            <span className="font-medium">₹{subtotal.toFixed(2)}</span>
          </p>
          <p className="flex justify-between">
            <span className="text-gray-700">Tax (2%):</span>
            <span className="font-medium">₹{tax.toFixed(2)}</span>
          </p>
          <p className="flex justify-between">
            <span className="text-gray-700">Delivery Charges:</span>
            <span className="font-medium">₹{deliveryCharge}</span>
          </p>
        </div>

        <h2 className="flex justify-between text-xl font-bold text-gray-800 border-t pt-4">
          <span>Total:</span>
          <span>₹{total.toFixed(2)}</span>
        </h2>

        <button
          className="mt-4 w-full bg-black hover:bg-gray-800 text-white py-2 px-6 rounded-md transition font-semibold"
          onClick={() => navigate('/checkout')}
        >
          Proceed to Checkout
        </button>
      </div>
    </div>
  );
};

export default Cart;
