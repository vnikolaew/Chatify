"use client";
import React, {
   UIEventHandler,
   useCallback,
   useEffect,
   useMemo,
   useRef,
   useState,
} from "react";
import {
   sleep,
   useGetChatGroupDetailsQuery,
   useGetMyClaimsQuery,
   useGetPaginatedGroupMessagesQuery,
} from "@web/api";
import { Button, ScrollShadow, Tooltip } from "@nextui-org/react";
import { ChatMessageEntry } from "@components/chat-group";
import DownArrow from "@components/icons/DownArrow";
import StartupRocketIcon from "@components/icons/StartupRocketIcon";
import { LoadingChatMessageEntry } from "@components/chat-group/messages/LoadingChatMessageEntry";
import MessageTextEditor from "@components/chat-group/messages/MessageTextEditor";

export interface ChatMessagesSectionProps {
   groupId: string;
}

export const ChatMessagesSection = ({ groupId }: ChatMessagesSectionProps) => {
   const messagesSectionRef = useRef<HTMLDivElement>(null!);
   const firstMessageRef = useRef<HTMLDivElement>(null!);
   const [isScrollDownButtonVisible, setIsScrollDownButtonVisible] =
      useState(true);
   const { data: groupDetails } = useGetChatGroupDetailsQuery(groupId);

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
   const showConversationBeginningMessage = useMemo(
      () =>
         !messages?.pages?.at(-1)?.hasMore && !isLoading && !isFetchingNextPage,
      [messages?.pages, isLoading, isFetchingNextPage]
   );

   console.log(`Messages: `, messages);

   const fetchMoreMessages = async () => {
      await fetchNextPage();
   };

   const handleMessageSectionScroll: UIEventHandler<HTMLDivElement> =
      useCallback(
         async (e) => {
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

            if (
               messagesSectionRef.current?.scrollTop === 0 &&
               messages?.pages?.at(-1)?.hasMore
            ) {
               console.log("We are at the top. Fetching next page ...");
               console.log(hasNextPage);

               // Fetch next page of messages:
               await sleep(200);
               await fetchNextPage();
            }
         },
         [messages?.pages, hasNextPage, fetchNextPage]
      );

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
      <section className={`w-full`}>
         <div className={`w-full relative`}>
            {isScrollDownButtonVisible && !isLoading && (
               <Tooltip
                  showArrow
                  offset={2}
                  placement={"top"}
                  color={"default"}
                  size={"sm"}
                  content={"Scroll to bottom"}
               >
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
               </Tooltip>
            )}
            <ScrollShadow
               onScroll={handleMessageSectionScroll}
               size={20}
               ref={messagesSectionRef}
               className={`w-full relative`}
               orientation={"vertical"}
            >
               {showConversationBeginningMessage && (
                  <div
                     className={`w-full flex justify-center gap-4 text-center items-center my-8`}
                  >
                     <StartupRocketIcon
                        className={`fill-default-400`}
                        size={24}
                     />
                     <span className={`text-medium text-default-400`}>
                        You've scrolled to the beginning of this conversation.
                     </span>
                  </div>
               )}
               <div
                  className={`flex relative px-2 my-12 w-full max-h-[60vh] flex-col gap-4`}
               >
                  {!isLoading &&
                     isFetchingNextPage &&
                     Array.from({ length: 5 }).map((_, i) => (
                        <LoadingChatMessageEntry
                           reversed={i % 4 === 0}
                           key={i}
                        />
                     ))}
                  {isLoading &&
                     Array.from({ length: 10 }).map((_, i) => (
                        <LoadingChatMessageEntry
                           reversed={i % 4 === 0}
                           key={i}
                        />
                     ))}
                  {messages?.pages
                     ?.flatMap((p) => p.items)
                     .reverse()
                     ?.map((message, i, arr) => {
                        const isMe =
                           message.senderInfo.userId ===
                           me.claims.nameidentifier;
                        const isLatest = i === arr.length - 1;
                        return (
                           <ChatMessageEntry
                              message={message}
                              key={i}
                              isMe={isMe}
                              {...(isLatest && { className: `mb-12` })}
                              {...(i === 0 && { ref: firstMessageRef })}
                              {...(i === 0 && { showReplies: true })}
                           />
                        );
                     })}
               </div>
            </ScrollShadow>
         </div>
         <div className={`mt-12 mx-4`}>
            <MessageTextEditor chatGroup={groupDetails?.chatGroup} />
         </div>
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
