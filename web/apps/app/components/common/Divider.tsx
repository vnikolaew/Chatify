"use client";
import React from "react";
import { Divider as NextUIDivider, DividerProps } from "@nextui-org/react";

const Divider = (props: DividerProps) => {
   return (
      <NextUIDivider
         className={`w-1/2 shadow-lg h-[1px] rounded-md text-gray-200 mt-3`}
         orientation={"horizontal"}
         {...props}
      />
   );
};

export default Divider;
