"use client";
import React, { SVGProps } from "react";

export interface LogoutIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const LogoutIcon = ({ size, fill, ...rest }: LogoutIconProps) => {
   return (
      <div>
         <svg
            width={size}
            height={size}
            // fill={fill ?? "white"}
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 6.35 6.35"
            {...rest}
            id="logout"
         >
            <path
               fillRule="evenodd"
               d="M7.953.998a3.024 3.024 0 0 0-3.006 3.004V20a3.024 3.024 0 0 0 3.006 3.004h3.994A3.022 3.022 0 0 0 14.951 20v-4.002c0-1.334-2-1.334-2 0V20a.983.983 0 0 1-1.004 1.004H7.953A.983.983 0 0 1 6.95 20V4.002a.983.983 0 0 1 1.004-1.004h3.994a.983.983 0 0 1 1.004 1.004v4.002c0 1.334 2 1.334 2 0V4.002A3.022 3.022 0 0 0 11.947.998H7.953zM1.957 4.984a1 1 0 0 0-1.01 1.02v11.994a1 1 0 0 0 2 0V6.004a1 1 0 0 0-.982-1.02 1 1 0 0 0-.008 0zm16.037 2.004a1 1 0 0 0-.096.004 1 1 0 0 0-.6 1.713L19.595 11h-9.588a1.006 1.006 0 0 0-.104 0c-1.333.07-1.23 2.071.104 2.002h9.582l-2.29 2.287a1 1 0 1 0 1.411 1.418l4.002-4.002a1 1 0 0 0 0-1.41l-4.002-4a1 1 0 0 0-.715-.307z"
               color="#000"
               fontFamily="sans-serif"
               fontWeight="400"
               overflow="visible"
               paintOrder="stroke fill markers"
               transform="scale(.26458)"
               // style="line-height:normal;font-variant-ligatures:normal;font-variant-position:normal;font-variant-caps:normal;font-variant-numeric:normal;font-variant-alternates:normal;font-feature-settings:normal;text-indent:0;text-align:start;text-decoration-line:none;text-decoration-style:solid;text-decoration-color:#000;text-transform:none;text-orientation:mixed;shape-padding:0;isolation:auto;mix-blend-mode:normal"
            ></path>
         </svg>
      </div>
   );
};

export default LogoutIcon;
