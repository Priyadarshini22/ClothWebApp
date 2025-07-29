import React, { useState,useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux';
import {  useNavigate, useParams } from 'react-router-dom';
import { fetchProductById } from '../reduxStore/productsSlice';
import { assets } from '../assets/assets';
import { addToCart,  } from '../reduxStore/cartSlice';
import { ToastContainer } from 'react-toastify';

const Product = () => {
  const { Id } = useParams();
  const navigate = useNavigate();

  const [addToCartbtn,setAddToCartBtn] = useState(false);

  const product = useSelector((state) => state.products.product);
  const cart = useSelector((state) => state.cart.cart);
  const customer = useSelector((state) => state.customer.customer);
  const [size,setSize] = useState(0);
  const dispatch = useDispatch();
  console.log('Add',customer)
  console.log(customer.Role)

  useEffect(() => {
   dispatch(fetchProductById(Id));
   console.log(cart)
   cart?.CartItems?.filter(item => item.ProductId == Id).length>0  ? setAddToCartBtn(true) : setAddToCartBtn(false);
   console.log(product)
  },[])

  const handleAddToCart = () => {
    console.log(cart)
    var discount = product.DiscountPercentage > 0 ? product.Price * product.DiscountPercentage / 100 : 0;
    dispatch(addToCart({
        CustomerId: customer.CustomerId,
        ProductId: product.Id,
        Quantity: 1,
        UnitPrice : product.Price,
        Discount: discount,
        Id: cart?.Id ?? 0,
        TotalPrice: (product.Price - discount) * 1, 
        SizeId: size
  }, customer.Token, navigate));
  }

  return (
    <div className='flex gap-5 sm:gap-12 flex-col sm:flex-row'>
     <div className="w-full sm:w-[20%]">
       <img 
          src={product.Image2 == null ? product.Image1 : product.Image2} 
          className="w-full h-auto max-h-[300px] object-contain rounded-md" 
          alt="Image 2"
        />
     </div>
     <div className="w-full sm:w-[50%]">
       <img 
         src={product.Image1} 
         className="w-full h-auto max-h-[700px] object-contain rounded-md" 
         alt="Image 1"
       />
     </div>
     <div>
       <p className='text-2xl'>{product.Name}</p>
       <div className='flex w-4 mt-2 gap-1'>
     <img src={assets.star_icon}/>
     <img src={assets.star_icon} />
     <img src={assets.star_icon} />
     <img src={assets.star_dull_icon} />

     </div>
     <div>
       <p className='text-2xl mt-4 font-bold'>${product.Price}</p>
       <p className='text-zinc-500  mt-5 font-medium text-xl'>${product.Description}</p>
      </div>
       <p className='mt-5'>Select Size</p>
       <div className="mt-10 flex gap-2">
  {product.ProductSizes?.map((item) => (
    <button
      key={item.Size}
      className={`border py-2 px-4 rounded transition ${
        item.Quantity === 0
          ? 'bg-gray-300 text-gray-500 line-through cursor-not-allowed'
          : `bg-gray-100 hover:bg-gray-200 ${
              size === item.Id ? 'border-green-500' : 'border-gray-300'
            }`
      }`}
      disabled={item.Quantity === 0}
      onClick={() => setSize(item.Id)}
    >
      {item.Size}
    </button>
  ))}
</div>
     
       <button className={`${addToCartbtn ||  customer.Role == 'admin' ? 'bg-gray-100 text-black' : 'bg-black text-white' }  font-medium px-5 py-2 mt-10`} onClick={handleAddToCart} type='submit' disabled={addToCartbtn || customer.Role == 'admin'}>ADD TO CART</button>
     </div>
     <ToastContainer/>
    </div>
  )
}

export default Product