import React from "react";

const OrderSummary = ({ cart }) => {
  const subtotal = cart.CartItems.reduce((sum, item) => sum + item.TotalPrice, 0);
  const tax = +(subtotal * 0.02).toFixed(2);
  const deliveryCharge = 100;
  const total = subtotal + tax + deliveryCharge;

  return (
    <div className="mt-4 mb-4">
      <h2 className="font-semibold">Order Items</h2>
      {cart.CartItems.map(item => (
        <div key={item.Id} className="text-sm">
          {item.ProductName} x {item.Quantity} — ₹{item.TotalPrice}
        </div>
      ))}

      <p className="mt-2">Subtotal: ₹{subtotal.toFixed(2)}</p>
      <p>Tax: ₹{tax.toFixed(2)}</p>
      <p>Delivery: ₹{deliveryCharge}</p>
      <h3 className="font-bold mt-1">Total: ₹{total.toFixed(2)}</h3>
    </div>
  );
};

export default OrderSummary;
