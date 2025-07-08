import React, { useEffect } from 'react';
import Title from '../components/Title';
import { useDispatch, useSelector } from 'react-redux';
import { getCartByCustomerId } from '../reduxStore/cartSlice';
import { useNavigate } from 'react-router-dom';
import CartItem from '../components/CartItem';

const Cart = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const token = useSelector((state) => state.customer.customer.Token);
  const customerId = useSelector((state) => state.customer.customer.CustomerId);
  const cart = useSelector((state) => state.cart.cart);

  useEffect(() => {
    console.log(customerId,token)
    if (customerId && token) {
      dispatch(getCartByCustomerId(customerId, token));
    }
  }, [customerId, token]);

  if (!cart?.CartItems?.length) return <p>Your cart is empty.</p>;

  const subtotal = cart.CartItems.reduce((sum, item) => sum + item.TotalPrice, 0);
  const tax = +(subtotal * 0.02).toFixed(2);
  const deliveryCharge = 100;
  const total = subtotal + tax + deliveryCharge;

  return (
    <div className="p-4">
      <Title text1="YOUR" text2="CART" />

      {cart.CartItems.map((item) => (
        <CartItem key={item.Id} item={item} />
      ))}

      <hr className="my-4" />
      <div className="text-right space-y-2">
        <p>Subtotal: ₹{subtotal.toFixed(2)}</p>
        <p>Tax (2%): ₹{tax.toFixed(2)}</p>
        <p>Delivery Charges: ₹{deliveryCharge}</p>
        <h2 className="text-lg font-semibold">Total: ₹{total.toFixed(2)}</h2>
        <button 
          className="bg-blue-600 text-white px-4 py-2 rounded mt-2"
          onClick={() => navigate('/checkout')}
        >
          Checkout
        </button>
      </div>
    </div>
  );
};

export default Cart;
