"use client";
import React from "react";
import { Button, ButtonProps } from "@nextui-org/react";
import { FacebookIcon } from "../../components/icons/FacebookIcon";

const FacebookSignInButton = (props: ButtonProps) => {
   const { className, ...rest } = props;
   return (
      <Button
         radius={"sm"}
         color={"primary"}
         className={`text-white hover:opacity-80 ${className}`}
         startContent={<FacebookIcon fill={"white"} size={24} />}
         {...rest}
      >
         Sign in with Facebook
      </Button>
   );
};

export default FacebookSignInButton;
