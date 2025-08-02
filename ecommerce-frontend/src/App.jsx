import React, { useEffect } from 'react'
import { Route, Routes } from 'react-router-dom'
import Home from './pages/Home'
import Navbar from './components/Navbar'
import Collection from './pages/Collection'
import About from './pages/About'
import Contact from './pages/Contact'
import Product from './pages/Product'
import Cart from './pages/Cart'
import Login from './pages/Login'
import PlaceOrder from './pages/PlaceOrder'
import Orders from './pages/Orders'
import { ToastContainer } from 'react-toastify'
import { useDispatch } from 'react-redux'
import Register from './pages/Register'
import { setCustomer } from './reduxStore/customerSlice'
import CreateProduct from './pages/CreateProduct'
import { getCartByCustomerId } from './reduxStore/cartSlice'
import Checkout from './pages/Checkout'
import AddAddress from './pages/AddAddress'
import PaymentPage from './pages/PaymentPage'
import { Elements } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';
import { fetchProductsFromAPI } from './reduxStore/productsSlice'
import Profile from './pages/Profile'

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLIC_KEY);


const App = () => {


  const dispatch = useDispatch();
  
    useEffect(() => {
               dispatch(fetchProductsFromAPI());
    const customerData = localStorage.getItem("customer");
    if (customerData != null && customerData != undefined && customerData != "undefined") {
      var customerDataObject = JSON.parse(customerData);
      dispatch(setCustomer(customerDataObject));
      console.log(customerDataObject.CustomerId)
      dispatch(getCartByCustomerId(customerDataObject.CustomerId))
    }
    
  }, []);
  
  return (
    <div className='px-4 sm:px-[5vw] md:px-[7vw] lg:px-[9vw]'>
      <Navbar/>
    <Routes>
      <Route path="/" element={<Home/>} />
      <Route path="/collection" element={<Collection/>}/>
      <Route path='/about' element={<About/>}/>
      <Route path='/contact' element={<Contact/>}/>
      <Route path='/product/:Id' element={<Product/>}/>
      <Route path='/cart' element={<Cart/>}/>
      <Route path='/create-product' element={<CreateProduct/>}/>
      <Route path='/register' element={<Register/>}/>
      <Route path='/login' element={<Login/>} />
      <Route path='/place-order' element={<PlaceOrder/>} />
      <Route path='/orders' element={<Orders/>} />
      <Route path='/checkout' element={<Checkout/>}/>
      <Route path="/add-address" element={<AddAddress />} />
      <Route path="/payment/:orderId"   element={<Elements stripe={stripePromise}><PaymentPage /></Elements>} />
      <Route path="/profile" element={<Profile />} />
      <Route path="/orders" element={<Orders />} />

    </Routes>
    <ToastContainer/>
    </div>
  )
}

export default App