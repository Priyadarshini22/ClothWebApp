import React from 'react';
import Title from '../components/Title';

const ContactUs = () => {
  return (
    <div className="max-w-4xl mx-auto px-4 py-10 bg-white text-gray-700">
      <Title text1="Contact" text2=" Us" />

      <div className="mt-8 space-y-6 text-base leading-relaxed">
        <h2 className="text-xl font-semibold text-gray-800">Our Store</h2>
        <p>
          54709 Willms Station  
          <br />
          Suite 350, Washington, USA
        </p>

        <p>
          <strong>Tel:</strong> (415) 555-0132  
          <br />
          <strong>Email:</strong> <a href="mailto:admin@forever.com" className="text-blue-600 hover:underline">admin@forever.com</a>
        </p>

        <h2 className="text-xl font-semibold text-gray-800">Careers at Forever</h2>
        <p>
          Interested in joining our team? Learn more about our departments, culture, and current job openings.
        </p>
        <a
          href="/careers"
          className="inline-block mt-2 text-white bg-gray-800 hover:bg-black px-5 py-2 rounded transition"
        >
          View Careers
        </a>
      </div>
    </div>
  );
};

export default ContactUs;
