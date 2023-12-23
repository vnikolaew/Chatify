import React from "react";

export interface FooterProps {
}

const Footer = ({}: FooterProps) => {
   return (
      <footer
         className={`w-full text-center mt-20 p-12 text-large border-t border-t-gray-700`}
      >
         <h2 className={`text-foreground font-medium text-2xl`}>
            Footer Area
         </h2>
      </footer>
   );
};

export default Footer;
