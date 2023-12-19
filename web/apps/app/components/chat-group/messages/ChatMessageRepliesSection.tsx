"use client";
import React, { useCallback } from "react";
import { getMediaUrl, useGetChatGroupDetailsQuery, useGetPaginatedMessageRepliesQuery } from "@web/api";
import { Avatar, Divider, Link, Skeleton, Tooltip } from "@nextui-org/react";
import { twMerge } from "tailwind-merge";
import moment from "moment";
import { useCurrentChatGroup, useCurrentUserId } from "@hooks";
import { ChatMessageReactionSection } from "@components/chat-group";
import ChatGroupMemberInfoCard from "@components/sidebar/members/ChatGroupMemberInfoCard";
import { ChatMessageReply } from "@openapi";

export interface ChatMessageRepliesSectionProps {
   messageId: string;
   total: number;
}

export const ChatMessageRepliesSection = ({
                                             messageId,
                                             total,
                                          }: ChatMessageRepliesSectionProps) => {
   const groupId = useCurrentChatGroup();
   const { data: groupDetails, error: groupError } = useGetChatGroupDetailsQuery(groupId);
   const {
      data: replies,
      isLoading,
      isFetching,
      error,
   } = useGetPaginatedMessageRepliesQuery({
      messageId,
      pagingCursor: null!,
      pageSize: 5,
   });

   const getUserMediaUrlForReply = useCallback((reply: ChatMessageReply) =>
         getMediaUrl(
            reply.user?.profilePicture?.mediaUrl ??
            groupDetails?.members?.find(m => m.id === reply?.userId)?.profilePicture?.mediaUrl),
      []);

   const meId = useCurrentUserId();

   if (error) {
      return (<div className={`text-xs text-red-500`}>An error occurred: {error.message}</div>);
   }
   if (isLoading && isFetching)
      return (
         <div className={`flex mb-4 flex-col gap-2 items-start`}>
            {Array.from({ length: total }).map((_, i) => (
               <div className={`flex items-center gap-2`} key={i}>
                  <Skeleton className={`w-8 h-8 rounded-lg`} />
                  <div
                     className={`flex flex-col justify-evenly items-start gap-2`}
                  >
                     <div className={`flex items-center gap-1`}>
                        <Skeleton className={`h-2 w-24 rounded-full`} />
                        <Skeleton className={`h-1 w-12 rounded-full`} />
                     </div>
                     <Skeleton className={`h-3 w-36 rounded-full`} />
                  </div>
               </div>
            ))}
         </div>
      );

   return (
      <div className={` flex mb-4 flex-col items-start w-full gap-2`}>
         <div className={`flex w-full items-center gap-2`}>
            <div className={`text-default-300 inline-flex text-center text-xs`}>
               <span>{replies?.pages?.flatMap(_ => _.items)?.length}</span>
               <span className={`ml-1`}>
                  {replies?.pages?.flatMap(_ => _.items)?.length !== 1 ? "replies" : "reply"}
               </span>
            </div>
            <Divider
               className={`text-default-300 h-[1.5px] w-2/3`}
               orientation={"horizontal"}
            />
         </div>
         {replies?.pages?.flatMap(_ => _.items)?.map((reply, i) => (
            <div
               className={`flex px-2 py-1 rounded-lg gap-2 items-center`}
               key={reply.id}
            >
               <Avatar
                  className={`w-6 h-6`}
                  size={"md"}
                  radius={"md"}
                  color={"default"}
                  isBordered
                  src={getUserMediaUrlForReply(reply)}
               />
               <div
                  className={twMerge(
                     `flex flex-col justify-center self-start items-start gap-0`,
                  )}
               >
                  <div className={twMerge(`items-center flex gap-1`)}>
                     <Tooltip
                        delay={500}
                        closeDelay={300}
                        offset={10}
                        showArrow
                        placement={"right"}
                        content={
                           <ChatGroupMemberInfoCard userId={reply.user?.id} />
                        }
                     >
                        <Link
                           className={`text-xs cursor-pointer`}
                           underline={"hover"}
                           color={`foreground`}
                        >
                           {reply.user?.username}{" "}
                           {meId === reply.userId && (
                              <span className={`text-default-400 ml-1`}>
                                 {" "}
                                 (you)
                              </span>
                           )}
                        </Link>
                     </Tooltip>
                     <span
                        className={`text-[0.6rem] inline-flex items-center justify-center h-[16px] text-center font-light text-default-500`}
                     >
                        {moment(new Date(reply.createdAt)).format(
                           "HH:MM DD/MM/YYYY",
                        )}
                     </span>
                  </div>
                  <span
                     className={`max-w-[500px] leading-4 font-light text-[0.7rem] text-foreground-500`}
                  >
                     {reply.content}
                  </span>
                  <ChatMessageReactionSection
                     isReply
                     messageId={reply.id}
                     reactionCounts={reply.reactionCounts}
                     userReaction={null!}
                  />
               </div>
            </div>
         ))}
      </div>
   );
};
