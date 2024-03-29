"use client";
import React, {
   createContext,
   PropsWithChildren,
   SetStateAction,
   Suspense,
   UIEventHandler,
   useCallback,
   useContext,
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
import { useTranslations } from "next-intl";
import { Meh } from "lucide-react";
import DownArrow from "../../icons/DownArrow";
import StartupRocketIcon from "../../icons/StartupRocketIcon";
import { LoadingChatMessageEntry } from "./LoadingChatMessageEntry";
import ChatMessageEntries from "./ChatMessageEntries";
import MembersTypingSection from "./MembersTypingSection";
import MessageTextEditor from "./editor/MessageTextEditor";
import PlateMessageEditor from "./editor/PlateMessageEditor";

export interface ChatMessagesSectionProps {
   groupId: string;
}

const ReplyToMessageContext = createContext<
   [string, React.Dispatch<SetStateAction<string>>]
>(null!);

export const useReplyToMessageContext = () => useContext(ReplyToMessageContext);

const ReplyToMessageContextProvider = ({ children }: PropsWithChildren) => {
   const [replyToMessageId, setReplyToMessageId] = useState<string>(null!);

   return (
      <ReplyToMessageContext.Provider
         value={[replyToMessageId, setReplyToMessageId]}
      >
         {children}
      </ReplyToMessageContext.Provider>
   );
};
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
   const groupHasNoMessages = useMemo(
      () =>
         messages?.pages?.length === 1 &&
         messages?.pages[0].items.length === 0 &&
         !messages?.pages[0].hasMore,
      [messages?.pages, messages?.pages[0].items.length]
   );

   const showConversationBeginningMessage = useMemo(
      () =>
         !messages?.pages?.at(-1)?.hasMore &&
         !isLoading &&
         !isFetchingNextPage &&
         !error,
      [messages?.pages, isLoading, isFetchingNextPage, error]
   );
   const t = useTranslations(`MainArea.ChatMessages`);

   const handleMessageSectionScroll: UIEventHandler<HTMLDivElement> =
      useCallback(
         async (e) => {
            if (
               messagesSectionRef.current?.scrollTop === 0 &&
               messages?.pages?.at(-1)?.hasMore
            ) {
               // Fetch next page of messages:
               sleep(200).then(async (_) => await fetchNextPage());
            }
         },
         [messages?.pages, fetchNextPage]
      );

   const handleScrollDown = useCallback(() => {
      messagesSectionRef.current?.scrollTo({
         top: messagesSectionRef.current?.scrollHeight - 30,
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

   const editorRef = useRef<HTMLDivElement>();

   return (
      <section className={`flex w-full flex-col items-start overflow-hidden`}>
         {groupId && (
            <ReplyToMessageContextProvider>
               <div className={`relative min-h-[60vh] w-full`}>
                  {isScrollDownButtonVisible && !isLoading && (
                     <Tooltip
                        showArrow
                        offset={2}
                        placement={"top"}
                        color={"default"}
                        size={"sm"}
                        classNames={{
                           base: `px-2 py-1 text-[.6rem]`,
                           content: `text-[.6rem] h-4`,
                        }}
                        content={t(`ScrollToBottom`)}
                     >
                        <Button
                           size={"md"}
                           radius={"full"}
                           onPress={handleScrollDown}
                           isIconOnly
                           className={`absolute bottom-20 right-10 z-10 cursor-pointer opacity-80`}
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
                     id={`scroll-shadow`}
                     size={20}
                     ref={messagesSectionRef}
                     className={`relative w-full`}
                     orientation={"vertical"}
                  >
                     {!groupHasNoMessages &&
                        showConversationBeginningMessage && (
                           <div
                              className={`my-8 flex w-full items-center justify-center gap-2 text-center`}
                           >
                              <StartupRocketIcon
                                 className={`fill-default-400`}
                                 size={24}
                              />
                              <span className={`text-medium text-default-400`}>
                                 {t(`ConversationBeginningMessage`)}
                              </span>
                           </div>
                        )}
                     {!isLoading && groupHasNoMessages && (
                        <div
                           className={`my-8 flex w-full items-center justify-center gap-2 text-center`}
                        >
                           <Meh className={`text-default-400`} size={24} />
                           <span className={`text-medium text-default-400`}>
                              Group has no messages yet. Be the first one to
                              text.
                           </span>
                        </div>
                     )}
                     <div
                        className={`relative mt-12 flex max-h-[60vh] w-full flex-col gap-4 px-2`}
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
                        <ChatMessageEntries
                           messages={messages}
                           ref={firstMessageRef}
                        />
                        <div className={`mb-4`}>
                           <MembersTypingSection />
                        </div>
                     </div>
                  </ScrollShadow>
               </div>
               <div
                  className={`mx-4 mt-4 flex w-full flex-col items-start gap-8`}
               >
                  <Suspense
                     fallback={
                        <Skeleton className={`rounded-small h-16 w-full`} />
                     }
                  >
                     <MessageTextEditor
                        ref={editorRef}
                        chatGroup={groupDetails}
                     />
                  </Suspense>
               </div>
            </ReplyToMessageContextProvider>
         )}
      </section>
   );
};
