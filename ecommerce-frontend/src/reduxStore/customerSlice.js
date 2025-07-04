import { createSlice } from "@reduxjs/toolkit";
import axios from "axios";


export const customerSlice = createSlice({
    name: "customer",
    initialState: {
       customer: null,
       loader: false,
    },
    reducers:{
       setCustomer: (state,action) => {
            state.customer = action.payload
       },
       setLoader: (state,action) => {
        state.loader = action.payload
       }
    }

})


export const {setCustomer, setLoader} = customerSlice.actions

export const registerCustomer = (newCustomer,navigate) => async () => {

  try {
     await axios.post("https://localhost:7117/api/Customers/RegisterCustomer",newCustomer);
     navigate('/login');
  } catch (err) {
    console.error("Error fetching products", err);
  }
};

export const loginCustomer = (existCustomer) => async (dispatch) => {

  try {
    const res = await axios.post("https://localhost:7117/api/Customers/Login",existCustomer);
    console.log(res);
    localStorage.setItem("customer", JSON.stringify(res.data));  // Save user data in localStorage
    dispatch(setCustomer(res.data))
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

};



export default customerSlice.reducer



