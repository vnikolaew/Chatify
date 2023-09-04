"use client";
import React, { useMemo } from "react";
import { ChatGroupFeedEntry } from "@openapi";
import { Avatar, Button, Skeleton } from "@nextui-org/react";
import moment from "moment";
import Link from "next/link";
import { useQueryClient } from "@tanstack/react-query";
import { getChatGroupDetails, getMediaUrl } from "@web/api";
import { useCurrentChatGroup } from "@hooks";

export interface ChatGroupFeedEntryProps {
   feedEntry: ChatGroupFeedEntry;
}

function formatDate(dateTime: string) {
   const date = moment(new Date(dateTime));
   return date.isSame(new Date(), "date")
      ? date.format("HH:MM A")
      : date.isSame(new Date(), "year")
      ? date.format("MMM D")
      : date.format("M/D/YY");
}

const ChatGroupFeedEntry = ({ feedEntry }: ChatGroupFeedEntryProps) => {
   const client = useQueryClient();
   const groupId = useCurrentChatGroup();
   const isActive = useMemo(
      () => feedEntry.chatGroup.id === groupId,
      [feedEntry, groupId]
   );

   const handlePrefetchGroupDetails = async () => {
      await client.prefetchQuery(["chat-group", feedEntry.chatGroup.id], {
         queryFn: ({ queryKey: [_, id] }) =>
            getChatGroupDetails({ chatGroupId: id }),
         staleTime: 30 * 60 * 1000,
      });
   };

   return (
      <Button
         onMouseEnter={async (_) => await handlePrefetchGroupDetails()}
         color={"default"}
         as={Link}
         href={`?c=${feedEntry.chatGroup.id}`}
         size={"lg"}
         variant={"light"}
         radius={"md"}
         className={`flex h-fit shadow-md transition-background duration-100 hover:bg-default-500 ${
            isActive
               ? "bg-default-100 border-l-2 border-l-primary"
               : `bg-transparent`
         } items-center cursor-pointer w-full gap-4 p-3`}
      >
         <Avatar
            fallback={<Skeleton className={`h-12 w-12 rounded-full`} />}
            isBordered
            radius={"full"}
            color={"primary"}
            size={"lg"}
            className={`aspect-square ml-2 object-cover`}
            src={getMediaUrl(feedEntry?.chatGroup?.picture?.mediaUrl)}
         />
         <div
            className={`flex flex-1 flex-col justify-evenly items-center gap-1`}
         >
            <div className={`flex w-full gap-2 items-center justify-between`}>
               <span className="text-medium w-3/4 truncate font-semibold text-default-800">
                  {feedEntry.chatGroup.name.substring(0, 20)}
               </span>
               <time className={`text-xs font-light text-default-500`}>
                  {feedEntry.latestMessage?.createdAt
                     ? formatDate(feedEntry.latestMessage.createdAt)
                     : "-"}
               </time>
            </div>
            <div className={`w-full h-4 rounded-full`}>
               <p
                  className={`text-small font-normal w-full truncate leading-3 text-default-500`}
                  dangerouslySetInnerHTML={{
                     __html: feedEntry.latestMessage?.content
                        ? `${feedEntry.latestMessage.content.substring(0, 40)}`
                        : "No messages yet.",
                  }}
               ></p>
            </div>
         </div>
      </Button>
   );
};

export default ChatGroupFeedEntry;
