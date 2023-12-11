"use client";
import React from "react";
import Link from "next/link";
import { Image } from "@nextui-org/react";
import NextImage from "next/image";

const ImagesPage = () => {
   return (
      <div className={`flex m-8 flex-col items-center gap-8`}>
         <div className={`flex items-center flex-col gap-4`}>
            <Image
               isBlurred
               width={240}
               src="https://nextui-docs-v2.vercel.app/images/album-cover.png"
               alt="NextUI Album Cover"
               className="m-5"
            />
            <Image
               as={NextImage}
               isZoomed
               width={240}
               height={240}
               alt="NextUI Fruit Image with Zoom"
               src="https://nextui-docs-v2.vercel.app/images/fruit-1.jpeg"
            />
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

export default ImagesPage;
