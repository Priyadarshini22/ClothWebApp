import { createSlice } from "@reduxjs/toolkit";
import axios from "axios";
import toast from "react-hot-toast";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

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

export const addToCart = (cart, token, navigate) => async () => {
  console.log(cart,token);
  try {
    const res = await axios.post(`${API_BASE_URL}/api/Carts/AddToCart`,cart, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    console.log('cart',res)
    toast.success("Added to Cart");
    
  } catch (err) {
    if(err.status == 401) navigate('/login');
    console.error("Error fetching products", err);
  }
};

export const updateCartItem = (cartItem, token) => async (dispatch) => {
  console.log(cartItem,token);
  try {
    const res = await axios.put(`${API_BASE_URL}/api/Carts/UpdateCartItem`,cartItem, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    console.log('updateCart',res)
    dispatch(getCartByCustomerId(cartItem.CustomerId))
  } catch (err) {
    console.error("Error fetching products", err);
  }
};


export const getCartByCustomerId = (customerId,token ) => async (dispatch) => 
{
    try {
    const res = await axios.get(`${API_BASE_URL}/api/Carts/GetCart/${customerId}`, {
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

export const removeItemFromCart = (customerId, Id, token) => async (dispatch) => {
  try {
    const res = await axios.delete(`${API_BASE_URL}/api/Carts/RemoveCartItem`, {
      headers: {
        Authorization: `Bearer ${token}`
      },
      data: {
        CustomerId: customerId,
        CartItemId: Id
      }
    });

    console.log('cart', res);
    dispatch(getCartByCustomerId(customerId, token));

  } catch (err) {
    console.error("Error removing item from cart", err);
  }
};

export default cartSlice.reducer



