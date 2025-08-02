import { createSlice } from "@reduxjs/toolkit";
import axios from "axios";
import toast from "react-hot-toast";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const orderSlice = createSlice({
    name: "order",
    initialState: {
       order: null,
       loader: false,
       orderNumber: 0,
       orders: []
    },
    reducers:{
       setOrder: (state,action) => {
            state.order = action.payload 
       },
       setLoader: (state,action) => {
        state.loader = action.payload
       },
       setOrderNumber: (state,action) => {
            state.order = action.payload 
       },
       setOrders: (state,action) => {
        state.orders = action.payload
       }
    }

})


export const {setOrder, setLoader, setOrderNumber, setOrders} = orderSlice.actions

export const createOrder = (newOrder,navigate,token) => async (dispatch) => {

      try {
      const res = await axios.post(`${import.meta.env.VITE_API_BASE_URL}/api/Orders/CreateOrder`, newOrder, {
        headers: { Authorization: `Bearer ${token}` }
      });
      
      console.log('Order success:', res.data);
      toast.success(`Order placed successfully`);

      dispatch(setOrderNumber(res.data.Data))
      if (newOrder.PaymentMethod !== "COD") {
  navigate(`/payment/${res.data.Data}`);
}
      // Optionally redirect or show order summary
    } catch (err) {
      console.error('Order failed:', err);
      alert('Order failed. Please try again.');
    }
};

export const getOrderById = (orderId,token) => async(dispatch) => {
  console.log(orderId)
  try{
      axios
      .get(`${import.meta.env.VITE_API_BASE_URL}/api/Orders/GetOrderById/${orderId}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      .then((res) => dispatch(setOrder(res.data.Data)))
      .catch(() => alert('Failed to retrieve order details'));
    }
    catch(err){
        console.log("Order failed",err)
    }
}


export const getOrdersByCustomerId = (customerId, token) => async(dispatch) => {
    try {
      const res = await axios.get(
        `${import.meta.env.VITE_API_BASE_URL}/api/Orders/GetOrdersByCustomer/${customerId}`,
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      );
      console.log(res)
      dispatch(setOrders(res.data?.Data ?? []));
    } catch (err) {
        console.log("Order failed",err)
    }
  }


export default orderSlice.reducer




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