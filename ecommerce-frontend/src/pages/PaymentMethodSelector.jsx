import React from "react";

const PaymentMethodSelector = ({ selected, onChange }) => {
  return (
    <div className="mt-4">
      <h2 className="text-lg font-medium text-gray-800 mb-3">Select Payment Method</h2>
      <div className="space-y-3">
        <label
          className={`flex items-center p-3 border rounded-md cursor-pointer transition hover:shadow-sm ${
            selected === 'COD' ? 'border-blue-500 bg-blue-50' : 'border-gray-300'
          }`}
        >
          <input
            type="radio"
            name="payment"
            value="COD"
            checked={selected === 'COD'}
            onChange={() => onChange('COD')}
            className="form-radio text-blue-600 mr-3"
          />
          <span className="text-gray-700">Cash on Delivery</span>
        </label>

        <label
          className={`flex items-center p-3 border rounded-md cursor-pointer transition hover:shadow-sm ${
            selected === 'Card' ? 'border-blue-500 bg-blue-50' : 'border-gray-300'
          }`}
        >
          <input
            type="radio"
            name="payment"
            value="Card"
            checked={selected === 'Card'}
            onChange={() => onChange('Card')}
            className="form-radio text-blue-600 mr-3"
          />
          <span className="text-gray-700">Pay with Card (Stripe)</span>
        </label>
      </div>
    </div>
  );
};

export default PaymentMethodSelector;
