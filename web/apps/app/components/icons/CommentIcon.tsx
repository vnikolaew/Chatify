"use client";
import React, { SVGProps } from "react";

export interface CommentIconProps extends SVGProps<SVGSVGElement> {
   size: string | number;
}

const CommentIcon = ({ size, fill, ...rest }: CommentIconProps) => {
   return (
      <svg
         width={size}
         height={size}
         // fill={fill ?? "white"}
         xmlns="http://www.w3.org/2000/svg"
         enableBackground="new 0 0 512 512"
         viewBox="0 0 512 512"
         id="comment"
         {...rest}
      >
         <path d="M91 189.934v90h90v-90H91zM151 249.934h-30v-30h30V249.934zM211 189.934v90h90v-90H211zM271 249.934h-30v-30h30V249.934zM331 189.934v90h90v-90H331zM391 249.934h-30v-30h30V249.934z"></path>
         <path
            d="M256,9.934c-141.159,0-256,100.935-256,225c0,47.88,18.323,95.46,51.748,134.776L25.276,502.066l124.785-62.393
			c33.299,13.448,68.901,20.26,105.939,20.26c141.159,0,256-100.935,256-225S397.159,9.934,256,9.934z M256,429.934
			c-35.335,0-69.155-6.902-100.522-20.514l-6.42-2.786l-82.334,41.167l17.379-86.893l-5.082-5.603
			C46.951,319.952,30,278.328,30,234.934c0-107.523,101.383-195,226-195s226,87.477,226,195S380.617,429.934,256,429.934z"
         ></path>
      </svg>
   );
};

export default CommentIcon;
