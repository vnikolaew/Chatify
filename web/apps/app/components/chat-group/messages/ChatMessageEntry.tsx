"use client";
import { ChatGroupMessageEntry } from "@openapi";
import {
   Avatar,
   Chip,
   Divider,
   Link,
   Tooltip,
   useDisclosure,
} from "@nextui-org/react";
import { getMediaUrl, useGetChatGroupDetailsQuery } from "@web/api";
import { twMerge } from "tailwind-merge";
import { ChatGroupMemberInfoCard } from "@components/members";
import moment from "moment/moment";
import React, { Fragment, useMemo, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { useHover } from "@hooks";
import {
   MessageAttachmentsSection,
   ExpandRepliesLink,
   ChatMessageReactionSection,
   ChatMessageActionsToolbar,
   ChatMessageRepliesSection,
} from "@components/chat-group/messages";
import { Pin } from "lucide-react";
import ForwardChatMessageModal from "@components/chat-group/messages/ForwardChatMessageModal";
import ForwardedChatMessageEntry from "@components/chat-group/messages/ForwardedChatMessageEntry";

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
   const [messageSectionRef, showMessageActions] = useHover<HTMLDivElement>();

   const {
      isOpen: forwardMessageModalOpen,
      onOpenChange: onForwardMessageModalOpenChange,
      onOpen,
   } = useDisclosure();

   const { data: groupDetails } = useGetChatGroupDetailsQuery(
      message.message.chatGroupId
   );

   const isPinned = useMemo(
      () =>
         groupDetails?.chatGroup?.pinnedMessages?.some(
            (m) => m.messageId === message?.message.id
         ),
      [groupDetails?.chatGroup?.pinnedMessages, message?.message.id]
   );

   const hasReplies = useMemo(
      () => message.repliersInfo.total > 0,
      [message.repliersInfo.total]
   );

   const hasAttachments = useMemo(() => {
      return message?.message?.attachments?.length > 0 ?? false;
   }, [message?.message?.attachments?.length]);

   const isForwardedMessage = useMemo(() => {
      return !!message?.forwardedMessage;
   }, [message?.forwardedMessage]);

   return (
      <div
         ref={messageSectionRef}
         className={`flex relative rounded-lg flex-col gap-2 items-start transition-background duration-100 hover:bg-default-100 ${className} ${
            isPinned && `bg-warning-50 bg-opacity-80`
         }`}
         {...rest}
      >
         {showMessageActions && (
            <Fragment>
               <ChatMessageActionsToolbar
                  messageId={message.message.id}
                  onOpenForwardMessageModal={onOpen}
                  showMoreActions={isMe}
               />
            </Fragment>
         )}
         <ForwardChatMessageModal
            message={message?.message}
            isOpen={forwardMessageModalOpen}
            onOpenChange={onForwardMessageModalOpenChange}
         />
         {isPinned && (
            <div className={`flex items-center gap-2 mt-2 mx-12`}>
               <Pin
                  size={12}
                  className={`stroke-warning fill-warning -rotate-[30deg]`}
               />
               <span className={`text-xs text-default-500`}>
                  Pinned by{" "}
                  {
                     groupDetails?.members?.find(
                        (m) =>
                           m.id ===
                           groupDetails?.chatGroup?.pinnedMessages?.find(
                              (m) => m.messageId === message.message.id
                           )?.pinnerId
                     )?.username
                  }
               </span>
            </div>
         )}
         <div
            className={`flex px-2 py-1 rounded-lg gap-3 items-start transition-background duration-100 hover:bg-default-100 `}
            key={message.message.id}
         >
            <Avatar
               className={`w-10 mt-2 h-10`}
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
                     !hasReplies && `mb-0`
                  }`}
                  dangerouslySetInnerHTML={{ __html: message.message.content }}
               ></span>
               {hasAttachments && (
                  <MessageAttachmentsSection
                     messageId={message.message.id}
                     attachments={message.message.attachments}
                  />
               )}
               {isForwardedMessage && (
                  <ForwardedChatMessageEntry message={message} />
               )}
               {
                  <ChatMessageReactionSection
                     userReaction={message.userReaction}
                     messageId={message.message.id}
                     reactionCounts={message.message.reactionCounts}
                  />
               }
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
                           {message.repliersInfo.replierInfos.length > 3 && (
                              <div
                                 className={`rounded-md flex items-center justify-center bg-black text-xs w-5 h-5 text-foreground`}
                              >
                                 +{message.repliersInfo.replierInfos.length - 3}
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
