"use client";
import React from "react";
import { Button, ButtonProps } from "@nextui-org/react";
import Link from "next/link";

const VARIANTS: ButtonProps["variant"][] = [
   "solid",
   "faded",
   "flat",
   "light",
   "ghost",
   "bordered",
   "shadow",
];

const COLORS: ButtonProps["color"][] = [
   "default",
   "primary",
   "danger",
   "success",
   "secondary",
   "warning",
];

const ButtonsPage = () => {
   return (
      <div className={`flex w-min flex-col gap-8`}>
         <div className={`flex mt-8 items-center gap-8`}>
            {Array.from({ length: COLORS.length })
               .map((_, i) => i)
               .map((i) => (
                  <Button
                     key={i}
                     className={`py-1 px-6`}
                     variant={VARIANTS[i]}
                     color={COLORS[i]}
                     radius={"sm"}
                  >
                     Button
                  </Button>
               ))}
         </div>
         <Link
            className={`hover:underline ml-auto self-end text-xl text-blue-500`}
            href={`/_playgrounds`}
         >
            Go Back
         </Link>
      </div>
   );
};

export default ButtonsPage;
