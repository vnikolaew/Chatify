"use client";
import React from "react";
import { User, UserStatus } from "@openapi";
import { Avatar, Badge, Skeleton, Tooltip } from "@nextui-org/react";
import { ChatGroupMemberInfoCard } from "@components/members";

export interface ChatGroupMemberEntryProps {
   member: User;
   isMe: boolean;
   myName: string;
   category: string;
   onHover?: React.MouseEventHandler<HTMLDivElement>;
}

export const ChatGroupMemberEntry = ({
   member,
   isMe,
   onHover,
   category,
   myName,
}: ChatGroupMemberEntryProps) => {
   const innerDiv = (
      <div
         onMouseEnter={onHover}
         className={`h-fit w-fit px-3 ml-4 mt-4 rounded-md transition-background duration-100 hover:bg-default-100 cursor-pointer flex items-start gap-3`}
      >
         <Badge
            color={
               member.status === UserStatus.ONLINE
                  ? "success"
                  : member.status === UserStatus.AWAY
                  ? "warning"
                  : "default"
            }
            content={""}
            placement={"bottom-right"}
            size={"sm"}
            variant={"shadow"}
            as={"span"}
         >
            <Avatar
               fallback={<Skeleton className={`h-10 w-10 rounded-full`} />}
               isBordered
               radius={"full"}
               color={
                  category === "admins"
                     ? "secondary"
                     : member.status === UserStatus.ONLINE
                     ? "success"
                     : "default"
               }
               size={"sm"}
               className={`aspect-square outline-1 object-cover`}
               src={member.profilePicture.mediaUrl}
            />
         </Badge>
         <span className={`text-medium`}>
            {myName === member.username
               ? `${member.username} (you)`
               : member.username}
         </span>
      </div>
   );

   return isMe ? (
      innerDiv
   ) : (
      <Tooltip
         delay={500}
         closeDelay={300}
         placement={"left"}
         key={member.id}
         content={<ChatGroupMemberInfoCard userId={member.id} />}
      >
         {innerDiv}
      </Tooltip>
   );
};
