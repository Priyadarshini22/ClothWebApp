import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import Title from '../components/Title';
import Products from '../components/Products';

const Collection = () => {
  const products = useSelector((state) => state.products.products);
  const categories = useSelector((state) => state.products.categories);

  const [collections, setCollections] = useState(products);
  const [checkedCategory, setCheckedCategory] = useState(0); // 0 means 'All'

  useEffect(() => {
    if (checkedCategory === 0) {
      setCollections(products);
    } else {
      setCollections(products.filter(item => item.CategoryId === checkedCategory));
    }
  }, [checkedCategory, products]);

  const handleCategoryChange = (id) => {
    setCheckedCategory(prev => (prev === id ? 0 : id)); // toggle
  };

  return (
    <div className="flex gap-10 flex-col lg:flex-row">
      <div className="text-xl lg:w-1/4">
        <p className="font-bold mb-4">FILTERS</p>
        <div className="border p-4 border-gray-300 rounded">
          <p className="text-sm font-semibold mb-2">CATEGORIES</p>
          {categories.map((item) => (
            <div className="flex items-center gap-2 mb-2" key={item.Id}>
              <input
                type="checkbox"
                id={item.Id}
                checked={item.Id === checkedCategory}
                onChange={() => handleCategoryChange(item.Id)}
              />
              <label htmlFor={item.Id} className="text-sm cursor-pointer">
                {item.Name}
              </label>
            </div>
          ))}
        </div>
      </div>

      <div className="lg:w-3/4">
        <Title text1="ALL " text2="COLLECTIONS" />
        <Products products={collections} />
      </div>
    </div>
  );
};

export default Collection;
