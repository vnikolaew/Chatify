"use client";
import React from "react";
import Link from "next/link";
import { User } from "@nextui-org/react";

const UserPage = () => {
   return (
      <div className={`flex m-8 w-1/3 flex-col gap-8`}>
         <div className={`flex flex-col items-center gap-4`}>
            <User
               isFocusable
               avatarProps={{
                  src: "https://robohash.org/hicveldicta.png",
               }}
               description={"Software Engineer at Twitter"}
               name={"John Doe"}
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

export default UserPage;
