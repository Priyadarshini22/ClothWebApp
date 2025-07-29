import { createSlice } from "@reduxjs/toolkit";
// import { products } from "../assets/assets";
import axios from "axios";
import toast from "react-hot-toast";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const productsSlice = createSlice({
    name: "products",
    initialState: {
       products: [],
       product:{},
       categories: [],
       loader:false,
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
       },
       setProductsLoader: (state,action) => {
        state.loader = action.payload
       }
    }

})


export const {setProducts,setProduct,setCategories, setProductsLoader} = productsSlice.actions


export const fetchProductsFromAPI = () => async (dispatch) => {
  dispatch(setProductsLoader(true))
  try {
    const res = await axios.get(`${API_BASE_URL}/api/Products/GetAllProducts`);
    console.log('fetch',res.data.Data)
    setProducts(res.data.Data)
    dispatch(setProducts(res.data.Data));
  } catch (err) {
    console.error("Error fetching products", err);
  }
  finally {
        dispatch(setProductsLoader(false))
  }
};


export const fetchProductById = (id) => async(dispatch) => {

   try{
      const res = await axios.get(`${API_BASE_URL}/api/Products/GetProductById/${id}`);
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
      const res = await axios.post(`${API_BASE_URL}/api/Products/CreateProduct`,product, {
      headers: {
         Authorization: `Bearer ${token}`,
      }});
      dispatch(setProduct(res.data.Data))
      dispatch(fetchProductsFromAPI())
      console.log(res);
      toast.success("New Product created");
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
  finally{
     dispatch(setProductsLoader(false))
  }
}


export const getProductCategories = () =>  async (dispatch)  => {
  try{
      const res = await axios.get(`${API_BASE_URL}/api/Categories/GetAllCategories`);
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



