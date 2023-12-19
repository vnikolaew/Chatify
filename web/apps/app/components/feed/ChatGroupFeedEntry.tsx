"use client";
import React, { Fragment, useMemo } from "react";
import { ChatGroupFeedEntry } from "@openapi";
import { Avatar, AvatarProps, Button, Skeleton } from "@nextui-org/react";
import moment from "moment";
import Link from "next/link";
import { useQueryClient } from "@tanstack/react-query";
import {
   GET_PAGINATED_GROUP_MESSAGES_KEY,
   getChatGroupDetails,
   getMediaUrl,
   getPaginatedGroupMessages,
   useGetUserDetailsQuery,
} from "@web/api";
import {
   useCurrentChatGroup,
   useCurrentUserId,
   useGetUsersTyping,
} from "@hooks";
import { Time } from "@components/common";

export interface ChatGroupFeedEntryProps {
   feedEntry: ChatGroupFeedEntry;
   avatarColor: AvatarProps[`color`];
}

function formatDate(dateTime: string) {
   const date = moment(new Date(dateTime));
   return date.isSame(new Date(), "date")
      ? date.format("HH:MM A")
      : date.isSame(new Date(), "year")
         ? date.format("MMM D")
         : date.format("M/D/YY");
}

const ChatGroupFeedEntry = ({ feedEntry, avatarColor }: ChatGroupFeedEntryProps) => {
   const client = useQueryClient();
   const meId = useCurrentUserId();
   const groupId = useCurrentChatGroup();

   const usersTyping = useGetUsersTyping(feedEntry?.chatGroup?.id);
   const isActive = useMemo(
      () => feedEntry?.chatGroup?.id === groupId,
      [groupId, feedEntry?.chatGroup?.id],
   );

   const handlePrefetchGroupDetails = async () => {
      await client.prefetchQuery([`chat-group`, feedEntry?.chatGroup?.id], {
         queryFn: ({ queryKey: [_, id] }) =>
            getChatGroupDetails({ chatGroupId: id }),
         staleTime: 30 * 60 * 1000,
      });

      await client.prefetchInfiniteQuery(
         GET_PAGINATED_GROUP_MESSAGES_KEY(feedEntry.chatGroup.id),
         {
            queryFn: ({ queryKey: [_, groupId] }) =>
               getPaginatedGroupMessages({
                  groupId,
                  pageSize: 5,
                  pagingCursor: null!,
               }),
            getNextPageParam: (lastPage) => lastPage.pagingCursor,
            getPreviousPageParam: (_, allPages) =>
               allPages.at(-1)?.pagingCursor,
         },
      );
   };
   const isPrivateGroup = useMemo(
      () => feedEntry?.chatGroup?.metadata?.private === `true`,
      [feedEntry?.chatGroup?.metadata?.private],
   );

   const {
      data: user,
      isLoading,
      error,
   } = useGetUserDetailsQuery(
      feedEntry?.chatGroup?.adminIds?.filter((id) => id !== meId)?.[0],
      { enabled: isPrivateGroup },
   );

   const sideBgColor = useMemo(() => {
      if (isActive) {
         switch (avatarColor) {
            case "danger":
               return `border-l-danger`;
            case "warning":
               return `border-l-warning`;
            case "primary":
               return `border-l-primary`;
            case "success":
               return `border-l-success`;
            case "secondary":
               return `border-l-secondary`;
            case "default":
               return `border-l-default`;
            default:
               return `border-l-transparent`;
         }
      }
      return `border-l-transparent`;

   }, [avatarColor, isActive]);

   const messageSummary = useMemo(() => {
      return [...usersTyping]
         .filter((_) => _.userId !== meId).length > 0
         ? `${[...usersTyping]
            .filter((u) => u.userId !== meId)
            .map((_) => _.username)
            .join(", ")} ${
            usersTyping.size === 1 ? ` is ` : ` are `
         } currently typing ...`
         : `${feedEntry.latestMessage?.content?.substring(0, 30)}${
         feedEntry.latestMessage?.content?.length > 30 ? `...` : ``
      }` ?? `No messages yet.`;
   }, [usersTyping, feedEntry?.latestMessage?.content, meId]);

   return (
      <Button
         onMouseEnter={async (_) => await handlePrefetchGroupDetails()}
         color={"default"}
         as={Link}
         href={`?c=${feedEntry?.chatGroup?.id}`}
         size={"lg"}
         variant={"light"}
         radius={"md"}
         className={`flex h-fit shadow-md transition-background duration-100 hover:bg-default-500 ${
            isActive
               ? `bg-default-100 border-l-2 ${sideBgColor}`
               : `bg-transparent`
         } items-center cursor-pointer w-full gap-4 p-3`}
      >
         <Avatar
            fallback={<Skeleton className={`h-10 w-10 rounded-full`} />}
            isBordered
            radius={"full"}
            color={avatarColor ?? "primary"}
            size={"md"}
            className={`aspect-square ml-2 object-cover`}
            src={getMediaUrl(
               isPrivateGroup
                  ? user?.user?.profilePicture?.mediaUrl
                  : feedEntry?.chatGroup?.picture?.mediaUrl,
            )}
         />
         <div
            className={`flex flex-1 flex-col justify-evenly items-center gap-0`}
         >
            <div className={`flex w-full gap-2 items-center justify-between`}>
               <span className="text-sm w-3/4 truncate font-semibold text-default-800">
                  {isPrivateGroup
                     ? user?.user?.username
                     : feedEntry?.chatGroup?.name?.substring(0, 20)}
               </span>
               <time className={`text-xxs font-light text-default-500`}>
                  {feedEntry?.latestMessage?.createdAt
                     ? formatDate(feedEntry.latestMessage.createdAt)
                     : "-"}
               </time>
            </div>
            <ChatFeedEntryMessageSummary
               meId={meId}
               feedEntry={feedEntry}
               isPrivateGroup={isPrivateGroup}
               messageSummary={messageSummary} />
         </div>
      </Button>
   );
};

interface ChatFeedEntryMessageSummaryProps {
   feedEntry?: ChatGroupFeedEntry;
   meId: string;
   isPrivateGroup: boolean;
   messageSummary: string;
}


const ChatFeedEntryMessageSummary = ({
                                        messageSummary,
                                        feedEntry,
                                        meId,
                                        isPrivateGroup,
                                     }: ChatFeedEntryMessageSummaryProps) => {
   const messageSenderId = feedEntry.latestMessage?.userId;

   return (
      <div className={`w-full flex items-center gap-1 h-4 rounded-full`}>
         {isPrivateGroup && messageSenderId === meId && (
            <span className={`text-xs text-default-400 font-semibold`}>
               You:
            </span>
         )}
         {isPrivateGroup && messageSenderId !== meId && (
            <Fragment />
         )}

         {!isPrivateGroup && messageSenderId === meId && (
            <span className={`text-xs text-default-400 font-semibold`}>
               You:
            </span>
         )}

         {!isPrivateGroup && messageSenderId !== meId && (
            <span className={`text-xs text-default-400 font-semibold`}>
               {feedEntry.messageSender?.username}:
            </span>
         )}

         <p
            className={`text-[.7rem] font-normal w-full truncate leading-3 text-default-500`}
            dangerouslySetInnerHTML={{
               __html: messageSummary,
            }}
         ></p>
      </div>
   );

};

export default ChatGroupFeedEntry;
