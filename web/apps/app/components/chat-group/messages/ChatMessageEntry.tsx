"use client";
import { ChatGroupMessageEntry } from "@openapi";
import {
   Avatar,
   AvatarGroup,
   Button,
   Chip,
   Link,
   Tooltip,
} from "@nextui-org/react";
import { getMediaUrl } from "@web/api";
import { twMerge } from "tailwind-merge";
import { ChatGroupMemberInfoCard } from "@components/members";
import moment from "moment/moment";
import React, { useState } from "react";
import * as repl from "repl";
import ChatMessageRepliesSection from "@components/chat-group/messages/ChatMessageRepliesSection";

export interface ChatMessageEntryProps
   extends React.DetailedHTMLProps<
      React.HTMLAttributes<HTMLDivElement>,
      HTMLDivElement
   > {
   message: ChatGroupMessageEntry;
   isMe: boolean;
   showReplies?: boolean;
}

const ChatMessageEntry = ({
   message,
   isMe,
   showReplies = false,
   ...rest
}: ChatMessageEntryProps) => {
   const [repliesExpanded, setRepliesExpanded] = useState(showReplies);

   return (
      <div
         className={`flex flex-col ${
            isMe ? "items-end" : "items-start"
         } gap-2 transition-background duration-100 hover:bg-default-100`}
      >
         <div
            className={`flex px-2 py-1 rounded-lg gap-3 items-center transition-background duration-100 hover:bg-default-100 ${
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
                  `flex flex-col justify-evenly self-start items-start gap-0`,
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
                        {message.senderInfo.username}{" "}
                        {isMe && (
                           <span className={`text-default-400 ml-1`}>
                              {" "}
                              (you)
                           </span>
                        )}
                     </Link>
                  </Tooltip>
                  <time className={`text-xs font-light text-default-500`}>
                     {moment(new Date(message.message.createdAt)).format(
                        "HH:MM DD/MM/YYYY"
                     )}
                  </time>
               </div>
               <span
                  className={`max-w-[500px] leading-5 text-[0.8rem] text-foreground-500 mt-1 ${
                     isMe && "text-right"
                  }`}
               >
                  {message.message.content}
               </span>
               <div
                  className={`max-w-[500px] flex items-center gap-0 text-small text-default-300 ${
                     isMe && "text-right"
                  }`}
               >
                  <Chip
                     className={`py-1 mt-1 flex items-center`}
                     size={"sm"}
                     variant={"faded"}
                     color={"default"}
                  >
                     <div className={`flex items-center gap-1`}>
                        {message.repliersInfo.replierInfos
                           .slice(0, 3)
                           .map((replier, i) => (
                              <Avatar
                                 classNames={{
                                    base: "w-5 h-5",
                                 }}
                                 color={"success"}
                                 radius={"sm"}
                                 className={`w-5 h-5`}
                                 src={replier.profilePictureUrl}
                                 size={"sm"}
                                 key={replier.userId}
                              />
                           ))}
                        {message.repliersInfo.total > 3 && (
                           <div
                              className={`rounded-md flex items-center justify-center bg-black text-xs w-5 h-5 text-foreground`}
                           >
                              +{message.repliersInfo.total - 3}
                           </div>
                        )}
                     </div>
                  </Chip>
                  <div className={`self-end`}>
                     <Link
                        className={`text-xs cursor-pointer ml-2`}
                        onPress={(_) => setRepliesExpanded(!repliesExpanded)}
                        underline={"hover"}
                        color={"primary"}
                     >
                        {message.repliersInfo.total}{" "}
                        {message.repliersInfo.total > 1 ? `replies` : `reply`}
                     </Link>
                  </div>
                  <div className={`self-end`}>
                     <span className={`text-xs text-default-400 ml-2`}>
                        Last reply at{" "}
                        {moment(
                           new Date(message.repliersInfo.lastUpdatedAt)
                        ).fromNow()}
                     </span>
                  </div>
               </div>
            </div>
         </div>
         {repliesExpanded && (
            <section className={`ml-16`}>
               <ChatMessageRepliesSection
                  messageId={message.message.id}
                  total={message.repliersInfo.total}
               />
            </section>
         )}
      </div>
   );
};

export default ChatMessageEntry;
