import React, { useEffect } from 'react'
import { useDispatch, useSelector } from 'react-redux';
import {  useParams } from 'react-router-dom';
import { fetchProductById } from '../reduxStore/productsSlice';
import { assets } from '../assets/assets';
import { addToCart,  } from '../reduxStore/cartSlice';
import { ToastContainer } from 'react-toastify';

const Product = () => {
  const { Id } = useParams();

  // const [size,setSize] = useState();

  const product = useSelector((state) => state.products.product);
  const customer = useSelector((state) => state.customer.customer);
  const dispatch = useDispatch();

  useEffect(() => {
   dispatch(fetchProductById(Id));
   console.log(product)
  },[])

  const handleAddToCart = () => {


    dispatch(addToCart({
        CustomerId: customer.CustomerId,
  ProductId: product.Id,
  Quantity: 1
    }, customer.Token));
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
       <div className='mt-10 flex gap-2'>
         {product.ProductSizes?.map((item) => (
  <button
    key={item.Size}
    className={`border py-2 px-4 ${
      item.Quantity === 0 ? 'bg-gray-300 line-through cursor-not-allowed' : 'bg-gray-100'
    }`}
    disabled={item.Quantity === 0}
  >
    {item.Size}
  </button>
))}
       </div>
       <button className='bg-black text-white font-medium px-5 py-2 mt-10' onClick={handleAddToCart} type='submit'>ADD TO CART</button>
     </div>
     <ToastContainer/>
    </div>
  )
}

export default Product