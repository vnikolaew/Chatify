"use client";
import React, { SVGProps } from "react";

export interface AddUserIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const AddUserIcon = ({ size, fill, ...rest }: AddUserIconProps) => {
   return (
      <div>
         <svg
            xmlns="http://www.w3.org/2000/svg"
            width={size}
            height={size}
            fill={fill}
            id="add-user"
            {...rest}
         >
            <path d="M42 42h-3v3c0 1.659-1.341 3-3 3s-3-1.341-3-3v-3h-3c-1.659 0-3-1.341-3-3s1.341-3 3-3h3v-3c0-1.659 1.341-3 3-3s3 1.341 3 3v3h3c1.659 0 3 1.341 3 3s-1.341 3-3 3zM27.423 30.423C23.715 31.536 21 34.935 21 39c0 3.915 2.511 7.209 6 8.448V48H6a3 3 0 0 1-3-3c0-6 5.799-11.598 11.727-13.812C11.304 29.073 9 25.317 9 21v-3c0-6.627 5.373-12 12-12s12 5.373 12 12v3c0 1.398-.312 2.697-.756 3.939-2.283 1.086-4.083 3.021-4.821 5.484z"></path>
         </svg>
      </div>
   );
};

export default AddUserIcon;
