"use client";
import React from "react";
import { useSignOutMutation } from "@web/api";
import { Button } from "@nextui-org/react";

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
      <Button
         onClick={handleClick}
         color={"danger"}
         radius={"md"}
         size={"lg"}
         variant={"flat"}
         disabled={isLoading}
         className={`hover:underline`}
      >
         {isLoading ? "Loading ..." : "Sign Out"}
      </Button>
   );
};

export default SignOut;
