// src/pages/CreateProduct.jsx
import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Title from '../components/Title';
import { createProduct } from '../reduxStore/productsSlice';
import { useNavigate } from 'react-router-dom';



const CreateProduct = () => {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const token = useSelector((state) => state.customer.customer.Token);
  const categories = useSelector((state) => state.products.categories);

  const [newSizeName, setNewSizeName] = useState("");
  const [newSizeQuantity, setNewSizeQuantity] = useState("");
  const [formData, setFormData] = useState({
    Name: "",
    Description: "",
    Price: "",
    DiscountPercentage: "",
    CategoryId: "",
    StockBySize: {},
    Images: [],
  });

  const [errors, setErrors] = useState({});

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleImageChange = (e) => {
    const files = Array.from(e.target.files);
    if (files.length < 1 || files.length > 2) {
      alert("Please upload at least 1 and at most 2 images.");
      return;
    }
    setFormData((prev) => ({ ...prev, Images: files }));
  };

  const validate = () => {
    const newErrors = {};
    const { Name, Description, Price, StockQuantity, DiscountPercentage, CategoryId } = formData;

    if (!Name || Name.length < 3 || Name.length > 100) {
      newErrors.Name = "Product Name must be between 3 and 100 characters.";
    }
    if (!Description || Description.length < 10) {
      newErrors.Description = "Description must be at least 10 characters.";
    }
    if (!Price || Price < 0.01 || Price > 10000) {
      newErrors.Price = "Price must be between $0.01 and $10,000.00.";
    }

    if (DiscountPercentage < 0 || DiscountPercentage > 100) {
      newErrors.DiscountPercentage = "Discount Percentage must be between 0 and 100.";
    }
    if (!CategoryId) {
      newErrors.CategoryId = "Please select a category.";
    }

    console.log(newErrors)
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSizeStockChange = (e) => {
  const { name, value } = e.target;
  console.log(name,value)
  setFormData((prev) => ({
    ...prev,
    StockBySize: {
      ...prev.StockBySize,
      [name]: Number(value),
    }
  }));
};
  const handleSubmit = (e) => {
    e.preventDefault();
    console.log('Submit')
        console.log(formData)
    console.log(token)
    if (!validate()) return;

    const multipartForm = new FormData();
    console.log(formData)
    console.log(token)
    Object.entries(formData).forEach(([key, value]) => {
      if (key === "Images") {
        value.forEach((file) => multipartForm.append("Images", file));
      }  else if (key != "StockBySize") {
        multipartForm.append(key, value);
      }
    });

    multipartForm.append("StockBySize", JSON.stringify(formData.StockBySize));

    console.log('multi',multipartForm);
    dispatch(createProduct(multipartForm, token, navigate));
    setFormData({
    Name: "",
    Description: "",
    Price: "",
    DiscountPercentage: "",
    CategoryId: "",
    StockBySize: {},
    Images: [],
    });
  };


  return (
    <div className="max-w-2xl mx-auto mt-10 p-6 bg-white rounded-md shadow-md">
      <Title text1="New " text2="Product" />
      <form onSubmit={handleSubmit} className="space-y-4">
        
        {/* Name */}
        <div>
          <input
            type="text"
            name="Name"
            placeholder="Product Name"
            value={formData.Name}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-gray-300 rounded"
          />
          {errors.Name && <p className="text-red-500 text-sm mt-1">{errors.Name}</p>}
        </div>

        {/* Description */}
        <div>
          <textarea
            name="Description"
            placeholder="Description"
            value={formData.Description}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-gray-300 rounded"
            rows={4}
          />
          {errors.Description && <p className="text-red-500 text-sm mt-1">{errors.Description}</p>}
        </div>

        {/* Price */}
        <div>
          <input
            type="number"
            name="Price"
            placeholder="Price"
            value={formData.Price}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-gray-300 rounded"
          />
          {errors.Price && <p className="text-red-500 text-sm mt-1">{errors.Price}</p>}
        </div>

        {/* Stock Quantity */}

  <div>
  <p className="mb-2 font-normal">Stock by Size</p>

  {/* Input fields for adding new size */}
  <div className="flex gap-4 mb-4">
    <input
      type="text"
      placeholder="Size (e.g. XXXL)"
      value={newSizeName}
      onChange={(e) => setNewSizeName(e.target.value)}
      className="flex-1 px-3 py-2 border border-gray-300 rounded"
    />
    <input
      type="number"
      placeholder="Stock"
      value={newSizeQuantity}
      onChange={(e) => setNewSizeQuantity(e.target.value)}
      className="w-28 px-3 py-2 border border-gray-300 rounded"
      min={0}
    />
    <button
      type="button"
      onClick={() => {
        const size = newSizeName.trim();
        const qty = parseInt(newSizeQuantity, 10);
        if (!size) {
          alert("Please enter a size name.");
          return;
        }
        if (isNaN(qty) || qty < 0) {
          alert("Please enter a valid stock count.");
          return;
        }
        if (formData.StockBySize[size]) {
          alert("Size already exists!");
          return;
        }
        setFormData((prev) => ({
          ...prev,
          StockBySize: {
            ...prev.StockBySize,
            [size]: qty,
          },
        }));
        setNewSizeName("");
        setNewSizeQuantity("");
      }}
      className="w-full bg-gray-800 text-white py-2 px-4 rounded hover:bg-gray-900"

    >
      Add
    </button>
  </div>

  {/* Render existing sizes */}
  {Object.entries(formData.StockBySize).map(([key, value]) => (
    <div key={key} className="flex items-center gap-4 mb-2">
      <label htmlFor={key} className="w-12">{key}</label>
      <input
        type="number"
        name={key}
        id={key}
        value={value}
        onChange={handleSizeStockChange}
        className="w-full px-3 py-2 border border-gray-300 rounded"
        min={0}
      />
      <button
        type="button"
        onClick={() => {
          const updatedSizes = { ...formData.StockBySize };
          delete updatedSizes[key];
          setFormData((prev) => ({ ...prev, StockBySize: updatedSizes }));
        }}
        className="text-red-500 text-sm"
      >
        Remove
      </button>
    </div>
  ))}

</div>


        {/* Discount Percentage */}
        <div>
          <input
            type="number"
            name="DiscountPercentage"
            placeholder="Discount Percentage"
            value={formData.DiscountPercentage}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-gray-300 rounded"
          />
          {errors.DiscountPercentage && <p className="text-red-500 text-sm mt-1">{errors.DiscountPercentage}</p>}
        </div>

        {/* Category */}
        <div>
          <select
            name="CategoryId"
            value={formData.CategoryId}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-gray-300 rounded"
          >
            <option value="" className="text-gray-400">Select Category</option>
            {categories.map((cat) => (
              <option key={cat.Id} value={cat.Id}>{cat.Name}</option>
            ))}
          </select>
          {errors.CategoryId && <p className="text-red-500 text-sm mt-1">{errors.CategoryId}</p>}
        </div>

        {/* Images Upload */}
        <div>
          <input
            type="file"
            name="Images"
            accept="image/*"
            multiple
            onChange={handleImageChange}
            className="w-full px-3 py-2 border border-gray-300 rounded"
          />
        </div>

        {/* Submit */}
        <button
          type="submit"
          className="w-full bg-gray-800 text-white py-2 rounded hover:bg-gray-900"
        >
          Create Product
        </button>
      </form>
    </div>
  );
};

export default CreateProduct;
