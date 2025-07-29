import React from 'react';
import Title from '../components/Title';

const About = () => {
  return (
    <div className="max-w-4xl mx-auto px-4 py-10 text-gray-700 bg-white">
      <Title text1="About" text2=" Us" />

      <section className="mt-6 space-y-6 text-base leading-relaxed">
        <p>
          <strong>Forever</strong> was born out of a passion for innovation and a desire to revolutionize the way people shop online. 
          Our journey began with a simple idea: to provide a platform where customers can easily discover, explore, and purchase 
          a wide range of products from the comfort of their homes.
        </p>

        <p>
          Since our inception, we've worked tirelessly to curate a diverse selection of high-quality products that cater to every 
          taste and preference. From fashion and beauty to electronics and home essentials, we offer an extensive collection 
          sourced from trusted brands and suppliers.
        </p>

        <h2 className="text-xl font-semibold mt-8 text-gray-800">Our Mission</h2>
        <p>
          Our mission at <strong>Forever</strong> is to empower customers with choice, convenience, and confidence. We're dedicated to 
          providing a seamless shopping experience that exceeds expectations — from browsing and ordering to delivery and beyond.
        </p>

        <h2 className="text-xl font-semibold mt-8 text-gray-800">Why Choose Us</h2>
        <ul className="list-disc pl-6 space-y-2">
          <li>
            <strong>Quality Assurance:</strong> We meticulously select and vet each product to ensure it meets our stringent quality standards.
          </li>
          <li>
            <strong>Convenience:</strong> With our user-friendly interface and hassle-free ordering process, shopping has never been easier.
          </li>
          <li>
            <strong>Exceptional Customer Service:</strong> Our team of dedicated professionals is here to assist you every step of the way, 
            ensuring your satisfaction is our top priority.
          </li>
        </ul>
      </section>
    </div>
  );
};

export default About;
