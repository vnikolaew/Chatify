"use client";
import React, { SVGProps } from "react";

export interface NotSentIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

export const NotSentIcon = ({ size, fill, ...rest }: NotSentIconProps) => {
   return (
      <svg
         width={size}
         height={size}
         fill={fill}
         xmlns="http://www.w3.org/2000/svg"
         {...rest}
         viewBox="0 0 24 24"
         id="not-sent"
      >
         <path d="M15.5 5h-7c-1.7 0-3.4.7-4.6 1.9C2.7 8.1 2 9.8 2 11.5 2 15.1 4.9 18 8.5 18h5.3l5.8 2.9c.1.1.2.1.4.1s.4 0 .5-.1c.3-.2.5-.5.5-.9v-4.8c.3-.6 1-2.4 1-3.7C22 7.9 19.1 5 15.5 5zm3.6 9.6c-.1.1-.1.3-.1.4v3.4l-4.6-2.3c-.1-.1-.2-.1-.4-.1H8.5C6 16 4 14 4 11.5c0-1.2.5-2.3 1.3-3.2.9-.8 2-1.3 3.2-1.3h7C18 7 20 9 20 11.5c0 .8-.6 2.3-.9 3.1z"></path>
         <path d="M14.8 9.2c-.4-.4-1-.4-1.4 0L12 10.6l-1.4-1.4c-.4-.4-1-.4-1.4 0s-.4 1 0 1.4l1.4 1.4-1.4 1.4c-.4.4-.4 1 0 1.4.2.2.5.3.7.3s.5-.1.7-.3l1.4-1.4 1.4 1.4c.2.2.5.3.7.3s.5-.1.7-.3c.4-.4.4-1 0-1.4L13.4 12l1.4-1.4c.4-.4.4-1 0-1.4z"></path>
      </svg>
   );
};
