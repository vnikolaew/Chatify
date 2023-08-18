"use client";
import React from "react";
import { useSignOutMutation } from "@web/api";
import { Button, Spinner } from "@nextui-org/react";

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
         isLoading={isLoading}
         spinner={<Spinner className={`mr-2`} color={"default"} size={"sm"} />}
         variant={"flat"}
         disabled={isLoading}
         className={`hover:underline`}
      >
         {isLoading ? "Loading ..." : "Sign Out"}
      </Button>
   );
};

export default SignOut;
