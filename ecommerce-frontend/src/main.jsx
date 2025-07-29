import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import { BrowserRouter } from 'react-router-dom'
import React from 'react'
import ShopContextProvider from './context/ShopContext.jsx'
import { Toaster } from 'react-hot-toast'

createRoot(document.getElementById('root')).render(
  <BrowserRouter>
  <ShopContextProvider>
    <App />
    <Toaster position="top-center" reverseOrder={false} />
    </ShopContextProvider>
  </BrowserRouter>,
)
