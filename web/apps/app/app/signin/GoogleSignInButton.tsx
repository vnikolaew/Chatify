"use client";
import React from "react";
import { Button, ButtonProps, Spinner } from "@nextui-org/react";
import { GoogleIcon } from "../../components/icons/GoogleIcon";

const GoogleSignInButton = (props: ButtonProps) => {
   const { className, isLoading, ...rest } = props;

   return (
      <Button
         radius={"sm"}
         className={`bg-white text-medium hover:bg-gray-200 text-black ${className}`}
         startContent={
            isLoading ? (
               <Spinner className={`mr-2`} color={"default"} size={"sm"} />
            ) : (
               <GoogleIcon size={24} />
            )
         }
         {...rest}
      >
         {isLoading ? "Loading" : "Sign in with Google"}
      </Button>
   );
};

export default GoogleSignInButton;
