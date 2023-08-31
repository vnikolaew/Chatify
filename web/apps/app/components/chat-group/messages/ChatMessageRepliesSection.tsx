"use client";
import React from "react";
import { getMediaUrl, useGetPaginatedMessageRepliesQuery } from "@web/api";
import { Avatar, Divider, Link, Skeleton, Tooltip } from "@nextui-org/react";
import { twMerge } from "tailwind-merge";
import { ChatGroupMemberInfoCard } from "@components/members";
import moment from "moment";
import { useCurrentUserId } from "@hooks";
import ChatMessageReactionSection from "./ChatMessageReactionSection";

export interface ChatMessageRepliesSectionProps {
   messageId: string;
   total: number;
}

export const ChatMessageRepliesSection = ({
   messageId,
   total,
}: ChatMessageRepliesSectionProps) => {
   const {
      data: replies,
      isLoading,
      error,
   } = useGetPaginatedMessageRepliesQuery({
      messageId,
      pagingCursor: null!,
      pageSize: 5,
   });
   const meId = useCurrentUserId();

   if (isLoading)
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
               <span>{replies?.length}</span>
               <span className={`ml-1`}>
                  {replies?.length > 1 ? "replies" : "reply"}
               </span>
            </div>
            <Divider
               className={`text-default-300 h-[1.5px] w-2/3`}
               orientation={"horizontal"}
            />
         </div>
         {replies?.map((reply, i) => (
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
                  src={getMediaUrl(reply.user?.profilePicture?.mediaUrl)}
               />
               <div
                  className={twMerge(
                     `flex flex-col justify-center self-start items-start gap-0`
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
                           "HH:MM DD/MM/YYYY"
                        )}
                     </span>
                  </div>
                  <span
                     className={`max-w-[500px] leading-4 font-light text-[0.7rem] text-foreground-500`}
                  >
                     {reply.content}
                  </span>
                  <ChatMessageReactionSection
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
