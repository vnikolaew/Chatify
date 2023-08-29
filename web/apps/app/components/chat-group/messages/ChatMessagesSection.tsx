"use client";
import React, { Fragment, UIEventHandler, useEffect, useRef } from "react";
import {
   getMediaUrl,
   sleep,
   useGetMyClaimsQuery,
   useGetPaginatedGroupMessagesQuery,
} from "@web/api";
import {
   Avatar,
   Button,
   Link,
   ScrollShadow,
   Skeleton,
   Tooltip,
} from "@nextui-org/react";
import moment from "moment";
import { ChatGroupMemberInfoCard } from "@components/members";
import { twMerge } from "tailwind-merge";
import { useIntersectionObserver } from "../../../hooks/useIntersectionObserver";
import ChatMessageEntry from "@components/chat-group/messages/ChatMessageEntry";

export interface ChatMessagesSectionProps {
   groupId: string;
}

const ChatMessagesSection = ({ groupId }: ChatMessagesSectionProps) => {
   const messagesSectionRef = useRef<HTMLDivElement>(null!);
   const firstMessageRef = useRef<HTMLDivElement>(null!);

   const {
      data: messages,
      isLoading,
      error,
      fetchNextPage,
      hasNextPage,
      isFetchingNextPage,
   } = useGetPaginatedGroupMessagesQuery(
      {
         groupId,
         pageSize: 5,
         pagingCursor: null!,
      },
      { enabled: !!groupId }
   );
   const { data: me } = useGetMyClaimsQuery();
   console.log(`Messages: `, messages);

   const fetchMoreMessages = async () => {
      await fetchNextPage();
   };

   const handleMessageSectionScroll: UIEventHandler<HTMLDivElement> = async (
      e
   ) => {
      if (messagesSectionRef.current?.scrollTop === 0 && hasNextPage) {
         console.log("We are at the top ...");

         // Fetch next page of messages:
         await sleep(500);
         await fetchNextPage();
      }
   };

   useEffect(() => {
      messagesSectionRef.current?.scroll({ top: 20, behavior: "smooth" });
   }, [messages]);

   return (
      <section className={`w-full`}>
         <ScrollShadow
            onScroll={handleMessageSectionScroll}
            style={{}}
            ref={messagesSectionRef}
            className={`w-full`}
            orientation={"vertical"}
         >
            <div
               className={`flex px-4 pt-12 w-full max-h-[70vh] flex-col gap-4`}
            >
               {!isLoading &&
                  isFetchingNextPage &&
                  Array.from({ length: 5 }).map((_, i) => (
                     <LoadingChatMessageEntry reversed={i % 4 === 0} key={i} />
                  ))}
               {true &&
                  Array.from({ length: 5 }).map((_, i) => (
                     <LoadingChatMessageEntry reversed={i % 4 === 0} key={i} />
                  ))}
               {messages?.pages
                  ?.flatMap((p) => p.items)
                  .reverse()
                  ?.map((message, i) => {
                     const isMe =
                        message.senderInfo.userId === me.claims.nameidentifier;
                     return (
                        <Fragment key={i}>
                           <ChatMessageEntry
                              message={message}
                              isMe={isMe}
                              {...(i === 0 && { ref: firstMessageRef })}
                           />
                           <ChatMessageEntry message={message} isMe={isMe} />
                        </Fragment>
                     );
                  })}
            </div>
         </ScrollShadow>
         <div className={`mt-4`}>
            <Button
               onPress={fetchMoreMessages}
               variant={"light"}
               color={"primary"}
            >
               Fetch more
            </Button>
         </div>
      </section>
   );
};

const LoadingChatMessageEntry = ({
   reversed = false,
}: {
   reversed?: boolean;
}) => {
   return (
      <div
         className={twMerge(
            `flex px-2 py-1 rounded-lg gap-3 items-center transition-background duration-100 hover:bg-default-200`,
            reversed && `flex-row-reverse`
         )}
      >
         <Skeleton className={`w-12 h-12 rounded-lg`} />
         <div
            className={twMerge(
               `flex my-auto flex-col h-full justify-between self-start items-start gap-2`,
               reversed && `items-end`
            )}
         >
            <div
               className={twMerge(
                  `items-center flex gap-2`,
                  reversed && `flex-row-reverse`
               )}
            >
               <Skeleton className={`h-3 w-24 rounded-full`} />
               <Skeleton className={`h-2 w-20 rounded-full`} />
            </div>
            <Skeleton className={`h-4 w-96 rounded-full`} />
            <Skeleton className={`h-2 w-36 rounded-full`} />
         </div>
      </div>
   );
};

export default ChatMessagesSection;
