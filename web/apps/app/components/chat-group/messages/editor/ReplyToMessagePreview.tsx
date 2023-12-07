"use client";
import React, { DetailedHTMLProps, HTMLAttributes, useMemo } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { GET_PAGINATED_GROUP_MESSAGES_KEY } from "@web/api";
import { useCurrentChatGroup } from "@hooks";
import { ChatGroupMessageEntry, CursorPaged } from "@openapi";
import { ChatMessageEntry } from "@components/chat-group";

export interface ReplyToMessagePreviewProps extends DetailedHTMLProps<HTMLAttributes<HTMLDivElement>, HTMLDivElement> {
   replyToId: string;
}

const ReplyToMessagePreview = ({ replyToId, ...rest }: ReplyToMessagePreviewProps) => {
   const groupId = useCurrentChatGroup();
   const { data } = useInfiniteQuery<CursorPaged<ChatGroupMessageEntry>>(GET_PAGINATED_GROUP_MESSAGES_KEY(groupId));
   const message = useMemo(() => data?.pages?.flatMap(p => p.items)?.find(m => m.message.id === replyToId), [data.pages, replyToId]);

   return (
      <div
         className={`absolute w-full rounded-md p-2 translate-y-[-100%] bg-zinc-900 border-foreground-200 border-1 text-xs -top-1 left-0`} {...rest}>
         <ChatMessageEntry isReplyTo className={`w-full`} message={message} isMe={false} />
      </div>
   );
};

export default ReplyToMessagePreview;


const x = { };
