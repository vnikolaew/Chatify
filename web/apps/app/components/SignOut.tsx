"use client";
import React from "react";
import { useSignOutMutation } from "@web/api";

const SignOut = () => {
   const {
      mutateAsync: signOut,
      data,
      isLoading,
      error,
   } = useSignOutMutation();

   const handleClick = async () => {
      try {
         await signOut(null!);
         window.location.reload();
      } catch (e) {}
   };

   return (
      <div>
         <button
            onClick={handleClick}
            disabled={isLoading}
            className={`hover:underline text-blue-500`}
         >
            {isLoading ? "Loading ..." : "Sign Out"}
         </button>
      </div>
   );
};

export default SignOut;
