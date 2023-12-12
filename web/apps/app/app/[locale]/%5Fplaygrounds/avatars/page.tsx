"use client";
import React from "react";
import { Avatar, AvatarGroup } from "@nextui-org/react";
import Link from "next/link";
//
const AvatarsPage = () => {
   return (
      <div className={`flex m-4 w-max flex-col gap-8`}>
         <div className={`flex mt-4 items-center gap-8`}>
            <Avatar
               isDisabled
               radius={"md"}
               isBordered
               size={"lg"}
               src="https://i.pravatar.cc/150?u=a042581f4e29026024d"
            />
            <Avatar
               showFallback
               size={"lg"}
               name="Jane"
               src="https://images.unsplash.com/broken"
            />
            <AvatarGroup
               renderCount={(count) => (
                  <p className="text-small text-foreground font-medium ml-2">
                     +{count} others
                  </p>
               )}
               max={2}
               total={20}
               isBordered
            >
               <Avatar src="https://i.pravatar.cc/150?u=a042581f4e29026024d" />
               <Avatar src="https://i.pravatar.cc/150?u=a04258a2462d826712d" />
               <Avatar src="https://i.pravatar.cc/150?u=a042581f4e29026704d" />
               <Avatar src="https://i.pravatar.cc/150?u=a04258114e29026302d" />
               <Avatar src="https://i.pravatar.cc/150?u=a04258114e29026702d" />
               <Avatar src="https://i.pravatar.cc/150?u=a04258114e29026708c" />
            </AvatarGroup>
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

export default AvatarsPage;
