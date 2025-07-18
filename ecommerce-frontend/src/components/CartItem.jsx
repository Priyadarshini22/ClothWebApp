import React from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { updateCartItem } from '../reduxStore/cartSlice';
// import { removeItemFromCart, updateCartItemQuantity } from '../reduxStore/cartSlice';

const CartItem = ({ item }) => {
  const dispatch = useDispatch();

  const cart = useSelector((state) => state.cart.cart);

  const handleQuantityChange = (delta) => {
    const newQuantity = item.Quantity + delta;
    if (newQuantity < 1) return;
    dispatch(updateCartItem({...item,CustomerId : cart.CustomerId, CartItemId: item.Id, Quantity: newQuantity, TotalPrice : (item.UnitPrice - item.Discount)*newQuantity}))
  };

  const handleRemove = () => {
    // dispatch(removeItemFromCart(item.Id));
  };

  return (
    <div className="flex justify-between items-center my-4 p-4 border rounded shadow-sm">
      <div className="flex gap-4 items-center">
        <img src={item.ProductImage} alt={item.ProductName} className="w-20 h-20 object-cover" />
        <div>
          <h3 className="font-semibold">{item.ProductName}</h3>
          <div className="text-sm">
            <span className="line-through text-gray-400">₹{item.UnitPrice.toFixed(2)}</span>{' '}
            <span className="text-green-600 font-bold">
              ₹{(item.UnitPrice - item.Discount).toFixed(2)}
            </span>
          </div>
        </div>
      </div>

      <div className="flex items-center gap-3">
        <button onClick={() => handleQuantityChange(-1)} className="px-2 border rounded">-</button>
        <span>{item.Quantity}</span>
        <button onClick={() => handleQuantityChange(1)} className="px-2 border rounded">+</button>
      </div>

      <div className="flex flex-col items-end">
        <span>Total: ₹{item.TotalPrice.toFixed(2)}</span>
        <button onClick={handleRemove} className="text-red-500 text-sm underline">
          Remove
        </button>
      </div>
    </div>
  );
};

export default CartItem;
