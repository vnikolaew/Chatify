"use client";
import React from "react";
import { Button, Link, Tooltip } from "@nextui-org/react";
import { AddUserIcon } from "@icons";
import { useTranslations } from "next-intl";

export interface AddNewFriendButtonProps {}

const AddNewFriendButton = ({}: AddNewFriendButtonProps) => {
   const t = useTranslations('MainNavbar.Popups');
   return (
      <div>
         <Tooltip
            showArrow
            closeDelay={100}
            delay={100}
            classNames={{
               base: `text-xs `,
               content: `text-[10px] h-5`
            }}
            shadow={`sm`}
            size={`sm`}
            content={t(`AddNewFriend`)}
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
