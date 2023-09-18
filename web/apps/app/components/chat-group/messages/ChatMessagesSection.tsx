"use client";
import React, {
   Fragment,
   Suspense,
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
   useGetPaginatedGroupMessagesQuery,
} from "@web/api";
import { Button, ScrollShadow, Skeleton, Tooltip } from "@nextui-org/react";
import { ChatMessageEntry } from "@components/chat-group";
import DownArrow from "@components/icons/DownArrow";
import StartupRocketIcon from "@components/icons/StartupRocketIcon";
import { LoadingChatMessageEntry } from "@components/chat-group/messages/LoadingChatMessageEntry";
import MessageTextEditor from "@components/chat-group/messages/editor/MessageTextEditor";
import MembersTypingSection from "@components/chat-group/messages/MembersTypingSection";
import { useCurrentUserId, useIsChatGroupPrivate } from "@hooks";

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
      isFetching,
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
   const meId = useCurrentUserId();
   const isPrivate = useIsChatGroupPrivate(groupDetails);
   const showConversationBeginningMessage = useMemo(
      () =>
         !messages?.pages?.at(-1)?.hasMore &&
         !isLoading &&
         !isFetchingNextPage &&
         !error,
      [messages?.pages, isLoading, isFetchingNextPage, error]
   );

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
      if (messages?.pages?.length > 1) {
         messagesSectionRef.current?.scrollTo({
            top: 20,
            behavior: "smooth",
         });
      } else {
         messagesSectionRef.current?.scrollTo({
            top: messagesSectionRef.current.scrollHeight,
            behavior: "smooth",
         });
      }
   }, [messages]);

   return (
      <section className={`w-full flex flex-col items-start overflow-hidden`}>
         {groupId && (
            <Fragment>
               <div className={`w-full relative min-h-[60vh]`}>
                  {isScrollDownButtonVisible && !isLoading && (
                     <Tooltip
                        showArrow
                        offset={2}
                        placement={"top"}
                        color={"default"}
                        size={"sm"}
                        classNames={{
                           base: `px-2 py-0 text-[.6rem]`,
                        }}
                        content={"Scroll to bottom"}
                     >
                        <Button
                           size={"md"}
                           radius={"full"}
                           onPress={handleScrollDown}
                           isIconOnly
                           className={`absolute opacity-80 z-10 cursor-pointer bottom-20 right-10`}
                           startContent={
                              <DownArrow
                                 className={`fill-transparent`}
                                 size={30}
                              />
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
                              You've scrolled to the beginning of this
                              conversation.
                           </span>
                        </div>
                     )}
                     <div
                        className={`flex relative px-2 mt-12 w-full max-h-[60vh] flex-col gap-4`}
                     >
                        {!isLoading &&
                           isFetchingNextPage &&
                           Array.from({ length: 5 }).map((_, i) => (
                              <LoadingChatMessageEntry
                                 reversed={i % 4 === 0}
                                 key={i}
                              />
                           ))}
                        {isFetching &&
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
                              const isMe = message.senderInfo.userId === meId;
                              const isLatest = i === arr.length - 1;
                              return (
                                 <ChatMessageEntry
                                    message={message}
                                    key={i}
                                    isMe={isMe}
                                    {...(isLatest && { className: `mb-4` })}
                                    {...(i === 0 && { ref: firstMessageRef })}
                                    {...(i === 0 && { showReplies: true })}
                                 />
                              );
                           })}
                        <div className={`mb-4`}>
                           <MembersTypingSection />
                        </div>
                     </div>
                  </ScrollShadow>
               </div>
               <div
                  className={`mt-4 w-full flex flex-col items-start gap-8 mx-4`}
               >
                  <Suspense
                     fallback={
                        <Skeleton className={`w-full h-16 rounded-small`} />
                     }
                  >
                     <MessageTextEditor
                        chatGroup={groupDetails}
                        placeholder={
                           isPrivate
                              ? `Message ${
                                   groupDetails?.members?.find(
                                      (_) => _.id !== meId
                                   )?.username
                                }`
                              : `Message in ${groupDetails?.chatGroup.name}`
                        }
                     />
                  </Suspense>
               </div>
            </Fragment>
         )}
      </section>
   );
};
