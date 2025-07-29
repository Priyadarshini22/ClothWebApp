import React, { useEffect } from 'react'
import Hero from '../components/Hero'
import LatestCollection from '../components/LatestCollection'
import { useDispatch } from 'react-redux'
import { getProductCategories } from '../reduxStore/productsSlice'
import { FaSpinner } from 'react-icons/fa';

const Home = () => {
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(getProductCategories());
  }, [dispatch]);

  return (
    <div>
      <Hero />
      <LatestCollection />
    </div>
  );
}

export default Home;
