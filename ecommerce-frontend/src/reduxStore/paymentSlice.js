import { createSlice } from "@reduxjs/toolkit";
import axios from "axios";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const paymentSlice = createSlice({
    name: "payment",
    initialState: {
       payment: null,
       loader: false,
       clientSecret: null,
    },
    reducers:{
       setPayment: (state,action) => {
            state.payment = action.payload 
       },
       setLoader: (state,action) => {
        state.loader = action.payload
       },
       setClientSecret: (state,action) => {
        state.clientSecret = action.payload
       }
    }

})


export const {setPayment, setLoader, setClientSecret} = paymentSlice.actions



export const createPayment = (payload,navigate) => async (dispatch) => {
  debugger

  try{
     const res =  await axios.post(`${API_BASE_URL}/api/Payments/CreatePayment`,payload, {
      headers: { Authorization: `Bearer ${payload.authToken}` },
      })

      dispatch(setClientSecret(res.data.clientSecret));
    }
  catch(err){
    console.log(err)
  }
      
}
export const processStripePayment = (payload, navigate) => async (dispatch) => {
  
  console.log(payload)
  
  try {
    dispatch(setLoader(true));
    const res = await axios.post(`${API_BASE_URL}/api/Payments/ProcessPayment`, payload, {
      headers: { Authorization: `Bearer ${payload.authToken}` },
    });


    if (res.data?.StatusCode === 200) {
      dispatch(setPayment(res.data.Data));
      alert("Payment successful!");
      // navigate(`/order-confirmation/${orderId}`);
    } else {
      alert("Payment failed: " + res.data?.Message);
    }
  } catch (err) {
    console.error("Payment Error:", err);
    alert("Payment failed. Please try again.");
  } finally {
    dispatch(setLoader(false));
  }
};





export default paymentSlice.reducer




    // try {
    //   const { token: stripeToken, error } = await stripe.createToken(cardElement);

    //   if (error || !stripeToken) {
    //     alert(error.message || 'Stripe token creation failed.');
    //     return;
    //   }

    //   const payload = {
    //     OrderId: parseInt(orderId),
    //     CustomerId: customerId,
    //     Amount: amount,
    //     PaymentMethod: 'Stripe',
    //     StripeToken: stripeToken.id // e.g. "tok_visa"
    //   };

    //   const res = await axios.post(
    //     `${import.meta.env.VITE_API_BASE_URL}/api/Payments/ProcessPayment`,
    //     payload,
    //     { headers: { Authorization: `Bearer ${token}` } }
    //   );

    //   if (res.data?.StatusCode === 200) {
    //     alert('Payment successful!');
    //     navigate(`/order-confirmation/${orderId}`);
    //   } else {
    //     alert('Payment failed: ' + res.data.Message);
    //   }
    // } catch (err) {
    //   console.error(err);
    //   alert('Error processing payment.');
    // } finally {
    //   setLoading(false);
    // }