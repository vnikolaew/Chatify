"use client";
import React from "react";
import { Avatar, Badge } from "@nextui-org/react";
import Link from "next/link";

const BadgesPage = () => {
   return (
      <div className={`flex m-4 w-max flex-col gap-8`}>
         <div className={`flex mt-4 items-center text-medium gap-8 `}>
            <Badge
               className={`py-0.5 hover:scale-105`}
               color={"warning"}
               size={"lg"}
               variant={"flat"}
               content={10}
            >
               <Avatar color={"danger"} size={"lg"} name={"John"} />
            </Badge>

            <Badge
               className={`py-0.5 hover:scale-105`}
               color={"danger"}
               size={"md"}
               variant={"solid"}
               content={`100+`}
            >
               <Avatar
                  size={"lg"}
                  src="https://i.pravatar.cc/150?u=a042581f4e29026024d"
               />
            </Badge>

            <Badge
               disableOutline={false}
               placement={"bottom-right"}
               className={`py-0.5 hover:scale-105`}
               color={"success"}
               size={"lg"}
               isDot
               variant={"solid"}
               content={`3`}
            >
               <Avatar
                  isBordered
                  color={"success"}
                  size={"lg"}
                  src="https://i.pravatar.cc/150?u=a042581f4e29026704d"
               />
            </Badge>
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

export default BadgesPage;
