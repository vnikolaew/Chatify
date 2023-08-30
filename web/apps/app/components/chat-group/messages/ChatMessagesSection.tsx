"use client";
import React, {
   Fragment,
   UIEventHandler,
   useCallback,
   useEffect,
   useRef,
   useState,
} from "react";
import {
   sleep,
   useGetMyClaimsQuery,
   useGetPaginatedGroupMessagesQuery,
} from "@web/api";
import { Button, ScrollShadow, Skeleton } from "@nextui-org/react";
import { twMerge } from "tailwind-merge";
import ChatMessageEntry from "@components/chat-group/messages/ChatMessageEntry";
import DownArrow from "@components/icons/DownArrow";

export interface ChatMessagesSectionProps {
   groupId: string;
}

const ChatMessagesSection = ({ groupId }: ChatMessagesSectionProps) => {
   const messagesSectionRef = useRef<HTMLDivElement>(null!);
   const firstMessageRef = useRef<HTMLDivElement>(null!);
   const [isScrollDownButtonVisible, setIsScrollDownButtonVisible] =
      useState(true);

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

   const handleMessageSectionScroll: UIEventHandler<HTMLDivElement> =
      useCallback(async (e) => {
         if (
            Math.round(
               messagesSectionRef.current?.scrollTop +
                  messagesSectionRef.current?.getBoundingClientRect().height
            ) >=
            messagesSectionRef.current?.scrollHeight - 20
         ) {
            setIsScrollDownButtonVisible(false);
         } else {
            setIsScrollDownButtonVisible(true);
         }

         if (messagesSectionRef.current?.scrollTop === 0 && hasNextPage) {
            console.log("We are at the top. Fetching next page ...");

            // Fetch next page of messages:
            await sleep(500);
            await fetchNextPage();
         }
      }, []);

   const handleScrollDown = useCallback(() => {
      messagesSectionRef.current?.scrollTo({
         top: messagesSectionRef.current?.scrollHeight,
         behavior: "smooth",
      });
   }, []);

   useEffect(() => {
      messagesSectionRef.current?.scroll({ top: 20, behavior: "smooth" });
   }, [messages]);

   return (
      <section className={`w-full relative`}>
         {isScrollDownButtonVisible && (
            <Button
               size={"md"}
               radius={"full"}
               onPress={handleScrollDown}
               isIconOnly
               className={`absolute opacity-80 z-10 cursor-pointer bottom-20 right-10`}
               startContent={
                  <DownArrow className={`fill-transparent`} size={30} />
               }
               variant={"shadow"}
               color={"default"}
            />
         )}
         <ScrollShadow
            onScroll={handleMessageSectionScroll}
            size={20}
            style={{}}
            ref={messagesSectionRef}
            className={`w-full relative`}
            orientation={"vertical"}
         >
            <div
               className={`flex relative px-2 my-12 w-full max-h-[70vh] flex-col gap-4`}
            >
               {!isLoading &&
                  isFetchingNextPage &&
                  Array.from({ length: 5 }).map((_, i) => (
                     <LoadingChatMessageEntry reversed={i % 4 === 0} key={i} />
                  ))}
               {/*{true &&*/}
               {/*   Array.from({ length: 5 }).map((_, i) => (*/}
               {/*      <LoadingChatMessageEntry reversed={i % 4 === 0} key={i} />*/}
               {/*   ))}*/}
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
                           <ChatMessageEntry
                              {...(i === 0 && { showReplies: true })}
                              message={message}
                              isMe={isMe}
                           />
                        </Fragment>
                     );
                  })}
            </div>
         </ScrollShadow>
         {hasNextPage && (
            <div className={`mt-4`}>
               <Button
                  onPress={fetchMoreMessages}
                  variant={"light"}
                  color={"primary"}
               >
                  Fetch more
               </Button>
            </div>
         )}
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
            `flex px-2 py-1 rounded-lg gap-3 items-center`,
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
