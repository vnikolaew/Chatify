"use client";
import React from "react";
import { Button, Link, Tooltip } from "@nextui-org/react";
import { AddUserIcon } from "@icons";

export interface AddNewFriendButtonProps {}

const AddNewFriendButton = ({}: AddNewFriendButtonProps) => {
   return (
      <div>
         <Tooltip
            showArrow
            closeDelay={100}
            delay={100}
            classNames={{
               base: `text-xs`,
            }}
            shadow={`sm`}
            size={`sm`}
            content={"Add a new friend"}
         >
            <Button
               as={Link}
               onPress={console.log}
               href={`/friends`}
               startContent={
                  <AddUserIcon className={`fill-foreground`} size={20} />
               }
               radius={"full"}
               isIconOnly
               variant={`solid`}
               className={`text-xs gap-1 p-4 bg-transparent rounded-full hover:bg-default-200`}
               size={"md"}
               color={`default`}
            />
         </Tooltip>
      </div>
   );
};

export default AddNewFriendButton;
