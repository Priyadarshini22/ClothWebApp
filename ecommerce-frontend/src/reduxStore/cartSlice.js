import { createSlice } from "@reduxjs/toolkit";
import axios from "axios";


export const cartSlice = createSlice({
    name: "cart",
    initialState: {
       cart: {},
    },
    reducers:{
       setCarts: (state,action) => {
            state.cart = action.payload
       },
    }

})


export const {setCarts} = cartSlice.actions

export const addToCart = (cart, token) => async () => {
  console.log(cart,token);
  try {
    const res = await axios.post("https://localhost:7117/api/Carts/AddToCart",cart, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    console.log('cart',res)
  } catch (err) {
    console.error("Error fetching products", err);
  }
};

export const getCartByCustomerId = (customerId,token ) => async (dispatch) => 
{
    try {
    const res = await axios.get(`https://localhost:7117/api/Carts/GetCart/${customerId}`, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    console.log('cart',res)
    dispatch(setCarts(res.data.Data));
  } catch (err) {
    console.error("Error fetching products", err);
  }
}


export default cartSlice.reducer



