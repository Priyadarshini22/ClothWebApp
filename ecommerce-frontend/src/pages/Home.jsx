import React, { useEffect } from 'react'
import Hero from '../components/Hero'
import LatestCollection from '../components/LatestCollection'
import { useDispatch } from 'react-redux'
import { fetchProductsFromAPI, getProductCategories } from '../reduxStore/productsSlice'


const Home = () => {


  const dispatch = useDispatch();
  useEffect(() => {
         dispatch(fetchProductsFromAPI());
         dispatch(getProductCategories());

  },[])
  return (
    <div>
    <Hero/>
    <LatestCollection/>
    </div>
  )
}

export default Home