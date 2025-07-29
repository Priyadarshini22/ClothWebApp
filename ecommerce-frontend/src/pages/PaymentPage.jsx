import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useStripe, useElements, CardElement } from '@stripe/react-stripe-js';
import { useDispatch, useSelector } from 'react-redux';
import { getOrderById } from '../reduxStore/orderSlice';
import { createPayment, processStripePayment, setLoader } from '../reduxStore/paymentSlice';
import PaymentMethodSelector from './PaymentMethodSelector';
import PlaceOrder from './PlaceOrder';
const CARD_OPTIONS = {
  style: {
    base: {
      fontSize: '16px',
      color: '#32325d',
      '::placeholder': { color: '#a0aec0' }
    },
    invalid: { color: '#e53e3e' }
  }
};

const PaymentPage = () => {
  const stripe = useStripe();
  const { orderId } = useParams();
  const dispatch = useDispatch();
  const elements = useElements();
  const navigate = useNavigate();
  
  const [paymentType,setPaymentType] = useState(null);
  const [paymentId,setPaymentId] = useState(null);

  const authToken = useSelector((state) => state.customer.customer.Token);
  const customerId = useSelector((state) => state.customer.customer.CustomerId);
  const order =useSelector((state) => state.order.order);

  const loading = useSelector((state) => state.order.loader);
  const clientSecret = useSelector((state) => state.payment.clientSecret)
  useEffect(() => {
    dispatch(getOrderById(parseInt(orderId), authToken))
  }, []);

  useEffect(() => {
    if(clientSecret !=null) confirmPayment();
  },[clientSecret])

  const confirmPayment = async () => {
    const { error, paymentIntent } =  await stripe.confirmCardPayment(clientSecret, {
    payment_method: paymentId,
    });
      
    if (error) {
      alert(error.message);
      return;
    }
      dispatch(processStripePayment({OrderId: order.Id, CustomerId: customerId, PaymentMethod: "Card", BillingId: order.BillingAddressId, Amount: order.TotalAmount, PaymentId: paymentIntent.id, authToken}, navigate))

  }
  const handlePayment = async () => {
  if (!stripe || !elements) return;
     dispatch(setLoader(true))
     const cardElement = elements.getElement(CardElement);
     const { paymentMethod, error } = await stripe.createPaymentMethod({
     type: 'card',
     card: cardElement, 
     });
   
     setPaymentId(paymentMethod.id);
     if (error) {
       alert(error.message);
       return;
     }
   
     console.log(paymentMethod)
   
     
     dispatch(createPayment({OrderId: order.Id, CustomerId: customerId, PaymentMethod: "Card", BillingId: order.BillingAddressId, Amount: order.TotalAmount, PaymentId: paymentMethod.id, authToken}, navigate));

  };

  return (

    <div>
      <div className="bg-white rounded-md shadow p-4 sm:p-6">
              <h2 className="text-xl font-semibold mb-4">Payment Method</h2>

              <PaymentMethodSelector
                selected={paymentType}
                onChange={setPaymentType}
              />

      </div>

     {paymentType == "Card" ? 
    <div className="max-w-md mx-auto p-6 bg-white shadow rounded mt-10">


      <h2 className="text-xl font-semibold mb-4">Complete Payment</h2>
        <>
          <p className="mb-4 text-gray-600">Order Total: ₹{order.TotalAmount}</p>
          <div className="mb-4 p-2 border border-gray-300 rounded">
            <CardElement options={CARD_OPTIONS} />
          </div>
          <button
            onClick={handlePayment}
            disabled={loading}
            className="w-full bg-black text-white py-2 rounded hover:bg-gray-800"
          >
            {loading ? 'Processing...' : 'Pay Now'}
          </button>
        </>
      
    </div> : paymentType == "COD" ? <PlaceOrder/> : <p></p>}
    </div>
  );
};

export default PaymentPage;
