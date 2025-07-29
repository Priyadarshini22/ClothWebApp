import { createSlice } from "@reduxjs/toolkit";
import axios from "axios";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const addressSlice = createSlice({
    name: "address",
    initialState: {
       addresses: [],
       address:{},
       loader: false,
    },
    reducers:{
       setAddresses: (state,action) => {
            state.addresses = action.payload 
       },
       setLoader: (state,action) => {
        state.loader = action.payload
       },
       setAddress: (state,action) => {
        state.address = action.payload
       }
    }

})


export const {setAddresses, setLoader, setAddress} = addressSlice.actions

export const addNewAddress = (newAddress,navigate,token) => async () => {

  try {
     await axios.post(`${API_BASE_URL}/api/Addresses/CreateAddress`,newAddress, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
     navigate('/checkout');
  } catch (err) {
    console.error("Error fetching products", err);
  }
};

export const getAddresses = (customerId,token) => async(dispatch) => {
    try{
    await axios.get(`${import.meta.env.VITE_API_BASE_URL}/api/Addresses/GetAddressesByCustomer/${customerId}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(res => dispatch(setAddresses(res.data.Data)) )
        .catch(err => console.error('Failed to fetch addresses:', err));
    }
    catch (err) {
    console.error("Error fetching products", err);
  }
}

export const getAddressById = (addressId,token,navigate) => async(dispatch) => {
  try{
        await axios.get(`${import.meta.env.VITE_API_BASE_URL}/api/Addresses/GetAddressById/${addressId}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(res => {dispatch(setAddress(res.data.Data)); navigate('/add-address');   })
        .catch(err => console.error('Failed to fetch addresses:', err));
  }
  catch(err)
  {
    console.error("Error fetching products", err);
  }
}

export const updateAddress = (address,navigate,token) => async() => {
  try{
        await axios.put(`${import.meta.env.VITE_API_BASE_URL}/api/Addresses/UpdateAddress`,address, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() =>  navigate('/checkout')  )
        .catch(err => console.error('Failed to fetch addresses:', err));
  }
  catch(err)
  {
    console.error("Error fetching products", err);
  }
}



export default addressSlice.reducer



