"use client";
import React from "react";
import { ChatGroupFeedEntry } from "@openapi/index";
import { Avatar, Button, Skeleton } from "@nextui-org/react";
import moment from "moment";
import Link from "next/link";
import { useQueryClient } from "@tanstack/react-query";
import { getChatGroupDetails } from "@web/api";

export interface ChatGroupFeedEntryProps {
   feedEntry: ChatGroupFeedEntry;
}

function formatDate(dateTime: string) {
   const date = moment(new Date(dateTime));
   return date.isSame(new Date(), "date")
      ? date.format("HH:MM AA")
      : date.isSame(new Date(), "year")
      ? date.format("MMM D")
      : date.format("M/D/YY");
}

const ChatGroupFeedEntry = ({ feedEntry }: ChatGroupFeedEntryProps) => {
   const client = useQueryClient();

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
         className={`flex transition-background duration-100 hover:bg-default-300 h-fit bg-transparent items-center cursor-pointer w-full gap-4 p-3`}
      >
         <Avatar
            fallback={<Skeleton className={`h-12 w-12 rounded-full`} />}
            isBordered
            radius={"full"}
            color={"primary"}
            size={"lg"}
            className={`aspect-square object-cover`}
            src={feedEntry.chatGroup.picture.mediaUrl}
         />
         <div
            className={`flex flex-1 flex-col justify-evenly items-center gap-2`}
         >
            <div className={`flex w-full items-center justify-between`}>
               <span className="text-large w-3/4 truncate font-semibold text-default-800">
                  {feedEntry.chatGroup.name}
               </span>
               <time className={`text-xs text-default-500`}>
                  {formatDate(feedEntry.latestMessage.createdAt)}
                  {}
               </time>
            </div>
            <div className={`w-full h-4 rounded-full`}>
               <p
                  className={`text-small w-full truncate leading-3 text-default-500`}
               >
                  {feedEntry.latestMessage.content.substring(0, 40)}...
               </p>
            </div>
         </div>
      </Button>
   );
};

export default ChatGroupFeedEntry;
