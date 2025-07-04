import React from 'react'
import { useNavigate } from 'react-router-dom'

const Products = ({products}) => {
    const navigate = useNavigate();
  return (
    <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4 gap-y-6">
    {products?.map((item,index) => 
       <div className='w-full text-sm py-5' key={index} onClick={()=>navigate(`/product/${item.Id}`)}>
       <div className='w-full h-100 overflow-hidden rounded'>
       <img src={item.Image1} className='hover:scale-95 transition ease-in-out w-full h-full object-cover'/>
       </div>
       <p>{item.Name}</p>
       <p className='font-bold'>${item.Price}</p>
     </div>
    )}
 </div>
  )
}

export default Products