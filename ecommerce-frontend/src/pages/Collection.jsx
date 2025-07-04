import React, { useEffect, useState } from 'react'
import {  useSelector } from 'react-redux'
import Title from '../components/Title';
import Products from '../components/Products';

const Collection = () => {
  const products = useSelector((state) => state.products.products);
  const [collections,setCollections] = useState(products);
  const categories = useSelector((state) => state.products.categories);
  const [checkedCategory,setCheckedCategory] = useState(0);

  useEffect(() => {
    checkedCategory == 0 ? setCollections(products) : 
    setCollections(
      products.filter((item) => item.CategoryId == checkedCategory)
    )
  }, [checkedCategory])
  
  return (
    <div className='flex gap-40'>
      <div className='text-2xl'>
      <p>FILTERS</p>
      <div className=''>
        <div className='text-sm border-2 pr-20 p-2 border-gray-300 border-t-2'>
        <p>CATEGORIES</p>
          {categories.map((item,index) => 
           
            <div className='flex gap-2' key={index}>
             <input type='checkbox' value={item.Id} id={item.Id} onChange={() => setCheckedCategory(item.Id)} checked={item.Id == checkedCategory}/>
             <label for={item.Id} className={item.Id}>{item.Name}</label>
             </div>
           )}
        </div>
        <div className='text-sm border-2 pr-20 p-2 border-gray-300 border-t-2'>
        <p>TYPE</p>
          {categories.map((item,index) => 
           
            <div className='flex gap-2' key={index}>
             <input type='checkbox' value={1} id={item.Id}/>
             <label for={item.Id} className=''>{item.Name}</label>
             </div>
           )}
        </div>
      </div>
      </div>
      <div className='text-2xl flex flex-col'><Title text1={"ALL "} text2={"COLLECTIONS"}/>
       <Products products={collections}/>
      </div>
    </div>
    
  )
}

export default Collection