import { configureStore } from '@reduxjs/toolkit'
import productsReducer from './productsSlice';
import cartReducer from './cartSlice';
import customerReducer from './customerSlice';
import addressReducer from './addressSlice';
import orderReducer from './orderSlice';
import paymentReducer from './paymentSlice';

export default configureStore({
  reducer: {
    products: productsReducer,
    cart: cartReducer,
    customer: customerReducer,
    address: addressReducer,
    order: orderReducer,
    payment: paymentReducer,
  }
})  