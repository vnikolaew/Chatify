"use client";
import React from "react";
import { Button, Link } from "@nextui-org/react";
import { PlusIcon } from "@icons";

export interface AddNewFriendButtonProps {}

const AddNewFriendButton = ({}: AddNewFriendButtonProps) => {
   return (
      <div>
         <Button
            as={Link}
            href={`/friends`}
            startContent={<PlusIcon className={`fill-foreground`} size={16} />}
            variant={`light`}
            className={`text-xs gap-1`}
            size={"sm"}
            color={`default`}
         >
            Add a friend
         </Button>
      </div>
   );
};

export default AddNewFriendButton;
