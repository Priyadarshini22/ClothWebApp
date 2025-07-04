import React, { useEffect, useState } from 'react'
import { ShopContext } from '../context/ShopContext'
import Title from './Title';
import { useSelector } from 'react-redux';
import Products from './Products';

const LatestCollection = () => {
  const state = useSelector((state) => state);
  const products = useSelector((state) => state.products.products);
  const [latestCollections,setLatestCollections] = useState([]);
  const [bestSellers,setBestSellers] = useState([]);
  useEffect(() => {
    console.log(state)
    console.log('Latest',products)
    console.log(bestSellers)
    setLatestCollections(products.slice(products.length-11,products.length));
    setBestSellers(products.filter((item) => item.bestseller));
  },[products])
  return (
    <div className='my-10'>
      <div className='text-center py-8 text-3xl'>
        <Title text1={"LATEST "} text2={"COLLECTIONS"}/>
        <p className='w-3/4 m-auto text-xs sm:text-sm md:text-base text-gray-600'>Lorem Ipsum</p>
      </div>
      <Products products={latestCollections}/>
      <div className='text-center py-8 text-3xl'><Title text1={"BEST "} text2={"SELLERS"}/>
             <Products products={bestSellers}/>
      </div>
    </div>
  )
}

export default LatestCollection