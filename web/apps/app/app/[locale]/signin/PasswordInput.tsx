"use client";
import React, { useState } from "react";
import { Input, InputProps } from "@nextui-org/react";
import { EyeFilledIcon, EyeSlashFilledIcon } from "@web/components";

const PasswordInput = (props: InputProps) => {
   const [isVisible, setIsVisible] = useState(false);

   const toggleVisibility = () => setIsVisible(!isVisible);

   // @ts-ignore
   return (
      <Input
         label="Password"
         variant="bordered"
         placeholder="Enter your password"
         endContent={
            <button
               className="focus:outline-none"
               type="button"
               onClick={toggleVisibility}
            >
               {isVisible ? (
                  <EyeSlashFilledIcon className="text-2xl text-default-400 pointer-events-none" />
               ) : (
                  <EyeFilledIcon className="text-2xl text-default-400 pointer-events-none" />
               )}
            </button>
         }
         type={isVisible ? "text" : "password"}
         {...props}
      />
   );
};

export default PasswordInput;
