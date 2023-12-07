"use client";
import { ChatGroupMessageEntry } from "@openapi";
import {
    Avatar, Button,
    Chip,
    Link,
    Tooltip,
    useDisclosure,
} from "@nextui-org/react";
import { getMediaUrl, useGetChatGroupDetailsQuery } from "@web/api";
import { twMerge } from "tailwind-merge";
import moment from "moment/moment";
import React, { Fragment, useMemo, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { useCurrentUserId, useHover } from "@hooks";
import {
    MessageAttachmentsSection,
    ExpandRepliesLink,
    ChatMessageReactionSection,
    ChatMessageActionsToolbar,
    ChatMessageRepliesSection,
} from "@components/chat-group/messages";
import { Pin } from "lucide-react";
import ForwardChatMessageModal from "@components/chat-group/messages/ForwardChatMessageModal";
import ForwardedChatMessageEntry from "@components/chat-group/messages/ForwardedChatMessageEntry";
import ChatGroupMemberInfoCard from "@components/sidebar/members/ChatGroupMemberInfoCard";
import MessageTextEditor, { ChatifyFile } from "./editor/MessageTextEditor";

export interface ChatMessageEntryProps
    extends React.DetailedHTMLProps<
        React.HTMLAttributes<HTMLDivElement>,
        HTMLDivElement
    > {
    message: ChatGroupMessageEntry;
    isMe: boolean;
    isReplyTo?: boolean;
    showReplies?: boolean;
}

export const ChatMessageEntry = React.forwardRef<HTMLDivElement, ChatMessageEntryProps>(({
                                                                                             message,
                                                                                             isMe,
                                                                                             isReplyTo,
                                                                                             showReplies = false,
                                                                                             className,
                                                                                             ...rest
                                                                                         }: ChatMessageEntryProps, ref) => {
    const [repliesExpanded, setRepliesExpanded] = useState(showReplies);
    const [messageSectionRef, showMessageActions, setShowMessageActions] = useHover<HTMLDivElement>();
    const meId = useCurrentUserId();
    const [isEditingMessage, setIsEditingMessage] = useState(false);

    const {
        isOpen: forwardMessageModalOpen,
        onOpenChange: onForwardMessageModalOpenChange,
        onOpen,
    } = useDisclosure();

    const { data: groupDetails } = useGetChatGroupDetailsQuery(
        message.message.chatGroupId,
    );

    const isPinned = useMemo(
        () =>
            groupDetails?.chatGroup?.pinnedMessages?.some(
                (m) => m.messageId === message?.message.id,
            ),
        [groupDetails?.chatGroup?.pinnedMessages, message?.message.id],
    );

    const hasReplies = useMemo(
        () => message.repliersInfo.total > 0,
        [message.repliersInfo.total],
    );

    const hasAttachments = useMemo(() =>
        message?.message?.attachments?.length > 0 ?? false, [message?.message?.attachments?.length]);

    const isForwardedMessage = useMemo(() =>
        !!message?.forwardedMessage, [message?.forwardedMessage]);

    return (
        <div
            className={`w-full rounded-lg transition-background duration-100 hover:bg-default-100 ${isPinned && `bg-warning-50 bg-opacity-80`}`}
            ref={ref}>
            <div
                ref={messageSectionRef}
                className={`flex relative flex-col gap-2 items-start  ${className}`}
                {...rest}
            >
                {showMessageActions && !isReplyTo && (
                    <ChatMessageActionsToolbar
                        onEditMessageChange={setIsEditingMessage}
                        messageId={message.message.id}
                        onOpenForwardMessageModal={onOpen}
                        showMoreActions={isMe}
                    />
                )}
                <ForwardChatMessageModal
                    message={message?.message}
                    isOpen={forwardMessageModalOpen}
                    onOpenChange={onForwardMessageModalOpenChange}
                />
                {isPinned && (
                    <div className={`flex items-center gap-2 mt-2 mx-12`}>
                        <Pin
                            size={12}
                            className={`stroke-warning fill-warning -rotate-[30deg]`}
                        />
                        <span className={`text-xs text-default-500`}>
                  Pinned by{" "}<b>
            {
                groupDetails?.chatGroup?.pinnedMessages?.find(
                    (m) => m.messageId === message.message.id,
                )?.pinnerId === meId ? `you` :
                    groupDetails?.members?.find(
                        (m) =>
                            m.id ===
                            groupDetails?.chatGroup?.pinnedMessages?.find(
                                (m) => m.messageId === message.message.id,
                            )?.pinnerId,
                    )?.username
            }
            </b>
               </span>
                    </div>
                )}
                <div
                    className={`flex px-2 py-2 rounded-lg gap-3 items-start`}
                    key={message.message.id}
                >
                    <Avatar
                        className={`w-10 mt-2 h-10`}
                        size={"md"}
                        radius={"md"}
                        color={"warning"}
                        isBordered
                        src={getMediaUrl(message.senderInfo.profilePictureUrl)}
                    />
                    <div
                        className={twMerge(
                            `flex flex-col justify-evenly self-start items-start gap-0`,
                        )}
                    >
                        <div className={twMerge(`items-center flex gap-2`)}>
                            <Tooltip
                                delay={500}
                                closeDelay={300}
                                offset={10}
                                showArrow
                                placement={"right"}
                                content={
                                    <ChatGroupMemberInfoCard
                                        userId={message.senderInfo.userId}
                                    />
                                }
                            >
                                <Link
                                    className={`text-small cursor-pointer`}
                                    underline={"hover"}
                                    color={`foreground`}
                                >
                                    {message.senderInfo.username}{" "}
                                    {isMe && (
                                        <span className={`text-default-400 ml-1`}>
                              {" "}
                                            (you)
                           </span>
                                    )}
                                </Link>
                            </Tooltip>
                            <time className={`text-xs font-light text-default-500`}>
                                {moment(new Date(message.message.createdAt)).format(
                                    "HH:MM DD/MM/YYYY",
                                )}
                            </time>
                        </div>
                        {isEditingMessage ? (
                            <div className={`flex flex-col gap-1 w-full min-w-[700px]`}>
                     <span className={`text-small`}>
                      Editing message ...
                     </span>
                                <MessageTextEditor
                                    className={`mt-2`}
                                    initialAttachments={new Map<string, ChatifyFile>(message?.message?.attachments?.map(a => {
                                        return [a.mediaUrl, new ChatifyFile(new File([], a.fileName, { type: a.type })!, a.id)]!;
                                    }))} initialContent={message?.message?.content} chatGroup={groupDetails} />
                                <Button className={`self-end`} size={`sm`} color={`danger`} variant={`solid`}
                                        onPress={_ => setIsEditingMessage(false)}>Cancel</Button>
                            </div>
                        ) : (
                            <Fragment>
               <span
                   className={`max-w-[500px] leading-5 text-[0.8rem] text-foreground-500 mt-1 ${
                       !hasReplies && `mb-0`
                   }`}
                   dangerouslySetInnerHTML={{ __html: message.message.content }}
               ></span>
                                {hasAttachments && (
                                    <MessageAttachmentsSection
                                        messageId={message.message.id}
                                        attachments={message.message.attachments}
                                    />
                                )}
                                {isForwardedMessage && (
                                    <ForwardedChatMessageEntry message={message} />
                                )}
                            </Fragment>
                        )}
                        {
                            <ChatMessageReactionSection
                                userReaction={message.userReaction}
                                messageId={message.message.id}
                                reactionCounts={message.message.reactionCounts}
                            />
                        }
                        {hasReplies && (
                            <div
                                className={`max-w-[500px] mt-1 flex items-center gap-0 text-small text-default-300`}
                            >
                                <Chip
                                    className={`py-1 mt-1 flex items-center`}
                                    size={"sm"}
                                    variant={"faded"}
                                    color={"default"}
                                >
                                    <div className={`flex items-center gap-1`}>
                                        {message.repliersInfo.replierInfos
                                            .slice(0, 3)
                                            .map((replier, i) => (
                                                <Avatar
                                                    classNames={{
                                                        base: "w-5 h-5",
                                                    }}
                                                    color={"success"}
                                                    radius={"sm"}
                                                    className={`w-5 h-5`}
                                                    src={replier.profilePictureUrl}
                                                    size={"sm"}
                                                    key={replier.userId}
                                                />
                                            ))}
                                        {message.repliersInfo.replierInfos.length > 3 && (
                                            <div
                                                className={`rounded-md flex items-center justify-center bg-black text-xs w-5 h-5 text-foreground`}
                                            >
                                                +{message.repliersInfo.replierInfos.length - 3}
                                            </div>
                                        )}
                                    </div>
                                </Chip>
                                <div className={`self-end`}>
                                    <ExpandRepliesLink
                                        onPress={(_) => setRepliesExpanded(!repliesExpanded)}
                                        expanded={repliesExpanded}
                                        totalReplies={message.repliersInfo.total}
                                    />
                                </div>
                                <div className={`self-end`}>
                        <span className={`text-xs text-default-400 ml-2`}>
                           Last reply{" "}
                            {moment(
                                new Date(message.repliersInfo.lastUpdatedAt),
                            ).fromNow()}
                        </span>
                                </div>
                            </div>
                        )}
                    </div>
                </div>
                {hasReplies && (
                    <AnimatePresence>
                        {repliesExpanded && (
                            <motion.section
                                initial={{ opacity: 0, height: 0 }}
                                exit={{ opacity: 0, height: 0 }}
                                animate={{ opacity: 1, height: "auto" }}
                                transition={{ ease: "easeInOut", duration: 0.3 }}
                                className={`ml-16`}
                            >
                                <ChatMessageRepliesSection
                                    messageId={message.message.id}
                                    total={message.repliersInfo.total}
                                />
                            </motion.section>
                        )}
                    </AnimatePresence>
                )}
            </div>
        </div>
    );
});
