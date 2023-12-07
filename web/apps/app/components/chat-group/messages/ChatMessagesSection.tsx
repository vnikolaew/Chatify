"use client";
import React, {
    createContext,
    PropsWithChildren, SetStateAction,
    Suspense,
    UIEventHandler,
    useCallback, useContext,
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
import DownArrow from "@components/icons/DownArrow";
import StartupRocketIcon from "@components/icons/StartupRocketIcon";
import { LoadingChatMessageEntry } from "@components/chat-group/messages/LoadingChatMessageEntry";
import MessageTextEditor from "@components/chat-group/messages/editor/MessageTextEditor";
import MembersTypingSection from "@components/chat-group/messages/MembersTypingSection";
import { useCurrentUserId, useIsChatGroupPrivate } from "@hooks";
import ChatMessageEntries from "@components/chat-group/messages/ChatMessageEntries";
import { useTranslations } from "next-intl";

export interface ChatMessagesSectionProps {
    groupId: string;
}

const ReplyToMessageContext = createContext<[string, React.Dispatch<SetStateAction<string>>]>(null!);

export const useReplyToMessageContext = () => useContext(ReplyToMessageContext);

const ReplyToMessageContextProvider = ({ children }: PropsWithChildren) => {
    const [replyToMessageId, setReplyToMessageId] = useState<string>(null!);

    return <ReplyToMessageContext.Provider value={[replyToMessageId, setReplyToMessageId]}>
        {children}
    </ReplyToMessageContext.Provider>;

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
        { enabled: !!groupId },
    );
    const meId = useCurrentUserId();
    const isPrivate = useIsChatGroupPrivate(groupDetails);
    const showConversationBeginningMessage = useMemo(
        () =>
            !messages?.pages?.at(-1)?.hasMore &&
            !isLoading &&
            !isFetchingNextPage &&
            !error,
        [messages?.pages, isLoading, isFetchingNextPage, error],
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
                    sleep(200).then(async _ => await fetchNextPage());
                }
            },
            [messages?.pages, fetchNextPage],
        );

    const handleScrollDown = useCallback(() => {
        messagesSectionRef.current?.scrollTo({
            top: messagesSectionRef.current?.scrollHeight - 30,
            behavior: "smooth",
        });
    }, []);

    useEffect(() => {
        console.log(`Running effect ...`);

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
                <ReplyToMessageContextProvider>
                    <div className={`w-full relative min-h-[60vh]`}>
                        {isScrollDownButtonVisible && !isLoading && (
                            <Tooltip
                                showArrow
                                offset={2}
                                placement={"top"}
                                color={"default"}
                                size={"sm"}
                                classNames={{
                                    base: `px-2 py-1 text-[.6rem]`,
                                }}
                                content={t(`ScrollToBottom`)}
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
                            id={`scroll-shadow`}
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
                              {t(`ConversationBeginningMessage`)}
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
                                <ChatMessageEntries messages={messages} ref={firstMessageRef} />
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
                                    isPrivate ?
                                        t(`MessageTextEditor.MessagePrivatePlaceholder`, {
                                            name: groupDetails?.members?.find(
                                                (_) => _.id !== meId,
                                            )?.username,
                                        }) : t(`MessageTextEditor.MessagePlaceholder`, { group: groupDetails?.chatGroup.name })
                                }
                            />
                        </Suspense>
                    </div>
                </ReplyToMessageContextProvider>
            )}
        </section>
    );
};
