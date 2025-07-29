import { createSlice } from "@reduxjs/toolkit";
import axios from "axios";
import toast from "react-hot-toast";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const customerSlice = createSlice({
    name: "customer",
    initialState: {
       customer: null,
       loader: false,
       profile:{},
    },
    reducers:{
       setCustomer: (state,action) => {
            state.customer = action.payload 
       },
       setLoader: (state,action) => {
        state.loader = action.payload
       },
       setprofile: (state,action) => {
        state.profile = action.payload
       }
    }

})


export const {setCustomer, setLoader, setprofile} = customerSlice.actions

export const registerCustomer = (newCustomer,navigate) => async () => {

  try {
     await axios.post(`${API_BASE_URL}/api/Customers/RegisterCustomer`,newCustomer);
     toast.success("Registered successful!");

     navigate('/login');
  } catch (err) {
    console.error("Error fetching products", err);
  }
};

export const loginCustomer = (existCustomer, navigate) => async (dispatch) => {

  try {
    const res = await axios.post(`${API_BASE_URL}/api/Customers/Login`,existCustomer);
    console.log(res);
    localStorage.setItem("customer", JSON.stringify(res.data));  // Save user data in localStorage
    toast.success("Login successful!");

    dispatch(setCustomer(res.data))
    dispatch(setLoader(false));
    navigate('/')
     return true;
  } catch (err) {
    if(err.status == 401)
    {
      console.error("Login again");
    }
    console.error("Error fetching products", err);
    return false;
  }

};


export const getCustomerDetails = (id) => async (dispatch) => {
    try {
    const res = await axios.get(`${API_BASE_URL}/api/Customers/GetCustomerById/${id}`);
    console.log(res); // Save user data in localStorage

    dispatch(setprofile(res.data.Data))
    dispatch(setLoader(false));
     return true;
  } catch (err) {
    if(err.status == 401)
    {
      console.error("Login again");
    }
    console.error("Error fetching products", err);
    return false;
  }
}


export default customerSlice.reducer



