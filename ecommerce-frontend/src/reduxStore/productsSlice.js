import { createSlice } from "@reduxjs/toolkit";
// import { products } from "../assets/assets";
import axios from "axios";


export const productsSlice = createSlice({
    name: "products",
    initialState: {
       products: [],
       product:{},
       categories: [],
    },
    reducers:{
       setProducts: (state,action) => {
            state.products = action.payload
       },
       setProduct: (state,action) =>{
         state.product = action.payload
       },
       setCategories: (state,action) => {
        state.categories = action.payload
       }
    }

})


export const {setProducts,setProduct,setCategories} = productsSlice.actions


export const fetchProductsFromAPI = () => async (dispatch) => {

  try {
    const res = await axios.get("https://localhost:7117/api/Products/GetAllProducts");
    console.log('fetch',res.data.Data)
    setProducts(res.data.Data)
    dispatch(setProducts(res.data.Data));
  } catch (err) {
    console.error("Error fetching products", err);
  }
};


export const fetchProductById = (id) => async(dispatch) => {

   try{
      const res = await axios.get(`https://localhost:7117/api/Products/GetProductById/${id}`);
      dispatch(setProduct(res.data.Data));
   }
   catch (err) {
    console.error("Error fetching products", err);
  }
}


export const createProduct = (product, token, navigate) => async (dispatch) => {
  console.log(token)
  console.log(product);
  try{
      const res = await axios.post("https://localhost:7117/api/Products/CreateProduct",product, {
      headers: {
         Authorization: `Bearer ${token}`,
      }});
      dispatch(setProduct(res.data.Data))
      console.log(res);
      return res;
  }

  catch(err)
  {
        if (err.status === 401) {
      navigate('/login'); // navigate to login page
    }
    console.log(err);
        console.error("Error fetching products", err);
  }
}


export const getProductCategories = () =>  async (dispatch)  => {
  try{
      const res = await axios.get("https://localhost:7117/api/Categories/GetAllCategories");
      // dispatch(setProduct(res.data.Data));
      dispatch(setCategories(res.data.Data));
      return res;
  }
  catch(err)
  {      
        console.error("Error fetching products", err);
  }
}
export default productsSlice.reducer



