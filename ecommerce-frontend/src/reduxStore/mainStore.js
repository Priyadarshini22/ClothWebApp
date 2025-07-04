import { configureStore } from '@reduxjs/toolkit'
import productsReducer from './productsSlice';
import cartReducer from './cartSlice';
import customerReducer from './customerSlice';
export default configureStore({
  reducer: {
    products: productsReducer,
    cart: cartReducer,
    customer: customerReducer,
  }
})