"use client";
import React from "react";
import { ChatGroupMessageEntry } from "@openapi";
import { Avatar, Divider, Link } from "@nextui-org/react";
import moment from "moment";
import { getMediaUrl, useGetUserDetailsQuery } from "@web/api";

export interface ForwardedChatMessageEntryProps {
   message: ChatGroupMessageEntry;
}

const ForwardedChatMessageEntry = ({
   message,
}: ForwardedChatMessageEntryProps) => {
   const { data: user } = useGetUserDetailsQuery(
      message.forwardedMessage?.userId,
      {
         enabled: !!message.forwardedMessage?.userId,
      }
   );

   return (
      <div className={`flex mx-2 my-1 items-center gap-2 w-full`}>
         <Divider
            className={`w-[2px] h-16 rounded-full text-foreground-500 bg-foreground-500`}
            orientation={`vertical`}
         />
         <div className={`flex flex-col items-start gap-1`}>
            <div className={`flex items-center gap-2`}>
               <Avatar
                  src={getMediaUrl(user?.user?.profilePicture.mediaUrl)}
                  radius={`sm`}
                  className={`w-4 h-4`}
                  size={`sm`}
               />
               <span className={`text-xs`}>{user?.user?.username}</span>
            </div>
            <p
               className={`text-xs text-foreground-500`}
               dangerouslySetInnerHTML={{
                  __html: message.forwardedMessage?.content,
               }}
            />
            <div className={`flex h-4 items-center gap-2`}>
               <span className={`text-[.6rem] text-foreground-300`}>
                  {moment(
                     new Date(message.forwardedMessage?.createdAt)
                  ).fromNow()}
               </span>
               <Divider className={`w-[1px] bg-foreground-400 h-2`} />
               <Link
                  href={`/?c=${message.forwardedMessage?.chatGroupId}`}
                  className={`text-[.6rem] cursor-pointer`}
                  underline={"hover"}
                  color={`primary`}
               >
                  View conversation
               </Link>
            </div>
         </div>
      </div>
   );
};

export default ForwardedChatMessageEntry;
