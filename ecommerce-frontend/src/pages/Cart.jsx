import React, { useEffect } from 'react'
import Title from '../components/Title'
import { useDispatch, useSelector } from 'react-redux';
import { getCartByCustomerId } from '../reduxStore/cartSlice';

const Cart = () => {

  const dispatch = useDispatch();

  const token = useSelector((state) => state.customer.customer.Token);
  const customerId = useSelector((state) => state.customer.customer.CustomerId);

  useEffect(() => {
    console.log(customerId,token)
     dispatch(getCartByCustomerId(customerId,token))
  },[])
  const cart = useSelector((state) => state.carts.cart); 
  return (
    <div>
      <Title text1={"YOUR "} text2={"CART"}/>
      {cart.CartItems.map((item) => 
      <div key={item.Id}>
        <hr/>
        <div className='flex gap-20'>
           <div className='flex gap-2'>
              <div>
                <img src={item.ProductImage} />
              </div>
              <div className='flex-col'>
                <span>{item.ProductName}</span>
                <span>${item.UnitPrice}</span>
              </div>
           </div>
           <div>
            {item.Quantity}
            </div>
        </div>
      </div>
)}
    </div>
  )
}

export default Cart