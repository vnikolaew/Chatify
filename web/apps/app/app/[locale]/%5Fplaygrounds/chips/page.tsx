"use client";
import React from "react";
import { Avatar, Chip } from "@nextui-org/react";
import Link from "next/link";

const ChipsPage = () => {
   return (
      <div className={`flex m-8 w-max flex-col gap-8`}>
         <div className={`grid gap-4 grid-cols-4`}>
            <Chip variant={"solid"}>Chip</Chip>
            <Chip variant={"bordered"} isDisabled>
               Chip
            </Chip>
            <Chip
               onClose={(_) => console.log("Closing ...")}
               variant={"flat"}
               size={"lg"}
               color={"primary"}
            >
               Chip
            </Chip>
            <Chip variant={"light"} size={"md"} color={"danger"}>
               Chip
            </Chip>
            <Chip variant={"shadow"} size={"sm"} color={"warning"}>
               Chip
            </Chip>
            <Chip
               avatar={
                  <Avatar
                     color={"danger"}
                     getInitials={(c) => c.charAt(0)}
                     size={"sm"}
                     name={"JW"}
                  />
               }
               variant={"faded"}
               radius={"full"}
               size={"md"}
               color={"success"}
            >
               Chip
            </Chip>
            <Chip variant={"dot"} radius={"sm"} size={"lg"} color={"secondary"}>
               Chip
            </Chip>
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

export default ChipsPage;
