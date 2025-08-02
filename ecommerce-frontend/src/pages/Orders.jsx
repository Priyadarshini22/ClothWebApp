import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Title from '../components/Title';
import { getOrdersByCustomerId } from '../reduxStore/orderSlice';

const Orders = () => {
  const dispatch = useDispatch();
  const customerId = useSelector((state) => state.customer.customer.CustomerId);
  const token = useSelector((state) => state.customer.customer.Token);
  const { orders, loader: loading } = useSelector((state) => state.order);

  useEffect(() => {
    if (customerId && token) {
      dispatch(getOrdersByCustomerId(customerId, token));
    }
  }, [customerId, token]);

  return (
    <>
      {loading ? (
        <p className="text-center mt-10 text-gray-500">Loading orders...</p>
      ) : orders.length === 0 ? (
        <p className="text-center mt-10 text-gray-500">No orders found.</p>
      ) : (
        <div className="max-w-4xl mx-auto mt-10 p-6 bg-white border border-gray-200 rounded-lg shadow-md">
          <Title text1="Your" text2=" Orders" />
          <div className="space-y-6 mt-8">
            {orders.map((order) => (
              <div
                key={order.Id}
                className="p-5 border border-gray-300 rounded-lg shadow-sm bg-gray-50 hover:bg-white transition duration-200"
              >
                <div className="flex justify-between items-center mb-3">
                  <h3 className="text-xl font-semibold text-gray-800">
                    Order #{order.OrderNumber}
                  </h3>
                  <span className="text-sm text-gray-400">
                    {new Date(order.OrderDate).toLocaleDateString()}
                  </span>
                </div>
                <p className="text-sm text-gray-600 mb-2 font-medium">
                  Status: <span className="capitalize">{order.OrderStatus}</span>
                </p>
                <p className="text-sm text-gray-700 font-semibold mb-4">
                  Total: ₹{order.TotalAmount.toFixed(2)}
                </p>
                <ul className="pl-5 list-disc text-gray-700 text-sm space-y-1">
                  {order.OrderItems?.map((item) => (
                    <li key={item.Id}>
                      {item.ProductName} × {item.Quantity} — ₹{item.TotalPrice.toFixed(2)}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      )}
    </>
  );
};

export default Orders;
