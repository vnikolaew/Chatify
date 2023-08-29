"use client";
import { ChatGroupMessageEntry } from "@openapi";
import { Avatar, Button, Chip, Link, Tooltip } from "@nextui-org/react";
import { getMediaUrl } from "@web/api";
import { twMerge } from "tailwind-merge";
import { ChatGroupMemberInfoCard } from "@components/members";
import moment from "moment/moment";
import React from "react";
import * as repl from "repl";

export interface ChatMessageEntryProps
   extends React.DetailedHTMLProps<
      React.HTMLAttributes<HTMLDivElement>,
      HTMLDivElement
   > {
   message: ChatGroupMessageEntry;
   isMe: boolean;
}

const ChatMessageEntry = ({
   message,
   isMe,
   ...rest
}: ChatMessageEntryProps) => {
   return (
      <div
         className={`flex px-2 py-1 rounded-lg gap-3 items-center transition-background duration-100 hover:bg-default-200 ${
            isMe && "flex-row-reverse"
         }`}
         key={message.message.id}
         {...rest}
      >
         <Avatar
            className={`w-10 h-10`}
            size={"md"}
            radius={"md"}
            color={"warning"}
            isBordered
            src={getMediaUrl(message.senderInfo.profilePictureUrl)}
         />
         <div
            className={twMerge(
               `flex flex-col justify-evenly self-start items-start gap-2`,
               isMe && `items-end`
            )}
         >
            <div
               className={twMerge(
                  `items-center flex gap-2`,
                  isMe && `flex-row-reverse`
               )}
            >
               <Tooltip
                  delay={500}
                  closeDelay={300}
                  offset={10}
                  showArrow
                  placement={"right"}
                  content={
                     <ChatGroupMemberInfoCard
                        userId={message.senderInfo.userId}
                     />
                  }
               >
                  <Link
                     className={`text-small cursor-pointer`}
                     underline={"hover"}
                     color={`foreground`}
                  >
                     {message.senderInfo.username}
                  </Link>
               </Tooltip>
               <time className={`text-xs font-light text-default-500`}>
                  {moment(new Date(message.message.createdAt)).format(
                     "HH:MM DD/MM/YYYY"
                  )}
               </time>
            </div>
            <span
               className={`max-w-[500px] text-small text-foreground-500 ${
                  isMe && "text-right"
               }`}
            >
               {message.message.content}
            </span>
            <span
               className={`max-w-[500px] text-small text-default-300 ${
                  isMe && "text-right"
               }`}
            >
               <Chip
                  className={`py-0 flex items-center`}
                  size={"sm"}
                  variant={"bordered"}
                  color={"default"}
               >
                  <div className={`flex items-center gap-1`}>
                     {message.repliersInfo.replierInfos.map((replier, i) => (
                        <Avatar
                           className={`w-4 h-4`}
                           color={"default"}
                           src={replier.profilePictureUrl}
                           size={"sm"}
                        />
                     ))}
                  </div>
               </Chip>
            </span>
         </div>
      </div>
   );
};

export default ChatMessageEntry;
