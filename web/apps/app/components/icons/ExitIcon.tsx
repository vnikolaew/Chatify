"use client";
import React, { SVGProps } from "react";

export interface ExitIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

export const ExitIcon = (props: ExitIconProps) => {
   const { size, fill, ...rest } = props;

   return (
      <svg
         xmlns="http://www.w3.org/2000/svg"
         width={size}
         height={size}
         // fill={fill ?? "white"}
         id="exit"
         {...rest}
      >
         <path
            // fill={fill}
            d="M6 2a4 4 0 0 0-4 4v3h2V6a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2v-3H2v3a4 4 0 0 0 4 4h12a4 4 0 0 0 4-4V6a4 4 0 0 0-4-4H6Z"
         ></path>
         <path
            // fill={fill}
            d="M3 11a1 1 0 1 0 0 2h9.582l-2.535 2.536a1 1 0 0 0 1.414 1.414l4.196-4.196a.998.998 0 0 0 0-1.508L11.46 7.05a1 1 0 1 0-1.414 1.414L12.582 11H3Z"
         ></path>
      </svg>
   );
};
