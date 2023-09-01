"use client";
import { ChatGroupMessageEntry } from "@openapi";
import {
   Avatar,
   Button,
   ButtonGroup,
   Chip,
   Link,
   Tooltip,
} from "@nextui-org/react";
import { getMediaUrl } from "@web/api";
import { twMerge } from "tailwind-merge";
import { ChatGroupMemberInfoCard } from "@components/members";
import moment from "moment/moment";
import React, { useMemo, useState } from "react";
import {
   ChatMessageRepliesSection,
   ExpandRepliesLink,
} from "@components/chat-group/messages";
import { AnimatePresence, motion } from "framer-motion";
import ChatMessageReactionSection from "@components/chat-group/messages/ChatMessageReactionSection";

export interface ChatMessageEntryProps
   extends React.DetailedHTMLProps<
      React.HTMLAttributes<HTMLDivElement>,
      HTMLDivElement
   > {
   message: ChatGroupMessageEntry;
   isMe: boolean;
   showReplies?: boolean;
}

export const ChatMessageEntry = ({
   message,
   isMe,
   showReplies = false,
   className,
   ...rest
}: ChatMessageEntryProps) => {
   const [repliesExpanded, setRepliesExpanded] = useState(showReplies);
   const [showMessageActions, setShowMessageActions] = useState(false);

   const hasReplies = useMemo(
      () => message.repliersInfo.total > 0,
      [message.repliersInfo.total]
   );

   const hasReactions = useMemo(() => {
      return Object.entries(message?.message?.reactionCounts ?? {}).length > 0;
   }, [message?.message?.reactionCounts]);

   return (
      <div
         className={`flex relative rounded-lg flex-col gap-2 items-start transition-background duration-100 hover:bg-default-100 ${className}`}
         onMouseEnter={(_) => setShowMessageActions(true)}
         onMouseLeave={(_) => setShowMessageActions(false)}
         {...rest}
      >
         {showMessageActions && (
            <ButtonGroup
               size={"sm"}
               variant={"shadow"}
               color={"default"}
               className={`absolute h-5 text-xs min-h-fit max-h-fit rounded-md bg-dark -translate-y-1/2 top-0 right-10`}
            >
               <Button
                  startContent={<span>#1</span>}
                  isIconOnly
                  className={`bg-default-100 max-h-fit py-0 px-0 h-5`}
               />
               <Button
                  startContent={<span>#2</span>}
                  isIconOnly
                  className={`max-h-fit h-5`}
               />
               <Button
                  startContent={<span>#3</span>}
                  isIconOnly
                  className={`max-h-fit h-5 bg-default-400`}
               />
            </ButtonGroup>
         )}
         <div
            className={`flex px-2 py-1 rounded-lg gap-3 items-center transition-background duration-100 hover:bg-default-100 `}
            key={message.message.id}
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
                  `flex flex-col justify-evenly self-start items-start gap-0`
               )}
            >
               <div className={twMerge(`items-center flex gap-2`)}>
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
                     !hasReplies && `mb-4`
                  }`}
               >
                  {message.message.content}
               </span>
               {hasReactions && (
                  <ChatMessageReactionSection
                     userReaction={message.userReaction}
                     messageId={message.message.id}
                     reactionCounts={message.message.reactionCounts}
                  />
               )}
               {hasReplies && (
                  <div
                     className={`max-w-[500px] mt-1 flex items-center gap-0 text-small text-default-300`}
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
                        <ExpandRepliesLink
                           onPress={(_) => setRepliesExpanded(!repliesExpanded)}
                           expanded={repliesExpanded}
                           totalReplies={message.repliersInfo.total}
                        />
                     </div>
                     <div className={`self-end`}>
                        <span className={`text-xs text-default-400 ml-2`}>
                           Last reply{" "}
                           {moment(
                              new Date(message.repliersInfo.lastUpdatedAt)
                           ).fromNow()}
                        </span>
                     </div>
                  </div>
               )}
            </div>
         </div>
         {hasReplies && (
            <AnimatePresence>
               {repliesExpanded && (
                  <motion.section
                     initial={{ opacity: 0, height: 0 }}
                     exit={{ opacity: 0, height: 0 }}
                     animate={{ opacity: 1, height: "auto" }}
                     transition={{ ease: "easeInOut", duration: 0.3 }}
                     className={`ml-16`}
                  >
                     <ChatMessageRepliesSection
                        messageId={message.message.id}
                        total={message.repliersInfo.total}
                     />
                  </motion.section>
               )}
            </AnimatePresence>
         )}
      </div>
   );
};
