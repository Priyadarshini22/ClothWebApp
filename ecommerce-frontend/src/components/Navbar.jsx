import React, { useEffect } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import {assets} from '../assets/assets';
import { useDispatch, useSelector } from 'react-redux';
import { setCustomer } from '../reduxStore/customerSlice';
const Navbar = () => {

  const navigate = useNavigate();
  const cart = useSelector((state) => state.cart.cart);
  const customer = useSelector((state) => state.customer.customer);
      const dispatch = useDispatch();
    useEffect(() => {
      console.log(customer)
          // dispatch(fetchProductsFromAPI());
    }, [customer])


  const handleLogOut = () => {
      localStorage.removeItem("customer");
      dispatch(setCustomer(null));
      navigate('/');
  }
  return (
    <div className='mb-10'>
    <div className='flex items-center justify-between py-5 font-medium'>
        <img src={assets.logo} className='w-36' alt=''/>
        <ul className='hidden sm:flex gap-5 text-sm text-gray-700'>
        <NavLink to='/' className='flex flex-col items-center gap-1'>
          <p>HOME</p>
          <hr className='w-2/4 border-none h-[1.5px] bg-gray-700 hidden'/>
        </NavLink>
        <NavLink to='/collection' className='flex flex-col items-center gap-1'>
          <p>COLLECTION</p>
          <hr className='w-2/4 border-none h-[1.5px] bg-gray-700 hidden'/>
        </NavLink>
        <NavLink to='/about' className='flex flex-col items-center gap-1'>
          <p>ABOUT</p>
          <hr className='w-2/4 border-none h-[1.5px] bg-gray-700 hidden'/>
        </NavLink>
        <NavLink to='/contact' className='flex flex-col items-center gap-1'>
          <p>CONTACT</p>
          <hr className='w-2/4 border-none h-[1.5px] bg-gray-700 hidden'/>
        </NavLink>
        </ul>
        <div className='flex items-center gap-5'>
          <img src={assets.search_icon} className='w-5 cursor-pointer'/>
          {customer != null ? <div className='flex gap-5'>
             <div className='group relative'>
             <img src={assets.profile_icon} className='w-5 cursor-pointer' />
                 <div className='group-hover:block hidden absolute dropdown-menu right-0 pt-4'>
                   <div className='flex flex-col  gap-2 w-36 py-3 px-5 bg-slate-100 text-gray-500'>
                     <p className=' cursor-pointer hover:text-black'>My Profile</p>
                     <p className=' cursor-pointer hover:text-black'>My Orders</p>
                     <p className=' cursor-pointer hover:text-black' onClick={handleLogOut}>Logout</p>
                   </div>
                 </div>
             </div>
             <div>          
                 <Link to={customer.Role == "user" ? '/cart' : '/create-product' } className='relative'>
                 <img src={assets.cart_icon} className='w-5 min-w-5'/>
                 <p className='absolute right-[-5px] bottom-[-5px] w-4 text-white text-[8px] text-center  rounded-full bg-black '>{cart.length}</p>
                 </Link>
             </div>
          </div>
          :<div className='flex gap-5'>
                         <div className='group relative'>
                          <Link to='/register'>Register</Link>
                         </div>
                           <div className='group relative'>
                          <Link to='/login'>Login</Link>
                         </div>
          </div>}
        </div>
    </div>  
    <hr className='text-gray-300 font-medium'/>
    </div>

  )
}

export default Navbar