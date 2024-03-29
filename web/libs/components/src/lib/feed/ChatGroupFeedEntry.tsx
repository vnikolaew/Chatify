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
} from "@web/hooks";

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

const ChatGroupFeedEntry = ({
   feedEntry,
   avatarColor,
}: ChatGroupFeedEntryProps) => {
   const client = useQueryClient();
   const meId = useCurrentUserId();
   const groupId = useCurrentChatGroup();

   const usersTyping = useGetUsersTyping(feedEntry?.chatGroup?.id!);
   const isActive = useMemo(
      () => feedEntry?.chatGroup?.id === groupId,
      [groupId, feedEntry?.chatGroup?.id]
   );

   const handlePrefetchGroupDetails = async () => {
      await client.prefetchQuery({
         queryKey: [`chat-group`, feedEntry?.chatGroup?.id],
         queryFn: ({ queryKey: [_, id] }) =>
            getChatGroupDetails({ chatGroupId: id! }),
         staleTime: 30 * 60 * 1000,
      });

      await client.prefetchInfiniteQuery({
         queryKey: GET_PAGINATED_GROUP_MESSAGES_KEY(feedEntry.chatGroup!.id!),
         queryFn: ({ queryKey: [_, groupId] }) =>
            getPaginatedGroupMessages({
               groupId,
               pageSize: 5,
               pagingCursor: null!,
            }),
         getNextPageParam: (lastPage) => lastPage.pagingCursor,
         getPreviousPageParam: (_, allPages) => allPages.at(-1)?.pagingCursor,
      });
   };
   const isPrivateGroup = useMemo(
      () => feedEntry?.chatGroup?.metadata?.private === `true`,
      [feedEntry?.chatGroup?.metadata?.private]
   );

   const {
      data: user,
      isLoading,
      error,
   } = useGetUserDetailsQuery(
      feedEntry?.chatGroup?.adminIds?.filter((id) => id !== meId)?.[0]!,
      { enabled: isPrivateGroup }
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
      const usersTypingWithoutMe = [...usersTyping].filter(
         (_) => _.userId !== meId
      );

      return usersTypingWithoutMe.length > 0
         ? `${usersTypingWithoutMe.map((_) => _.username).join(", ")} ${
              usersTyping.size === 1 ? ` is ` : ` are `
           } currently typing ...`
         : `${
              feedEntry.latestMessage?.content
                 ? feedEntry.latestMessage?.content?.substring(0, 30)
                 : ``
           }${feedEntry.latestMessage?.content?.length! > 30 ? `...` : ``}` ??
              `No messages yet.`;
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
         className={`transition-background hover:bg-default-500 flex h-fit shadow-md duration-100 ${
            isActive
               ? `bg-default-100 border-l-2 ${sideBgColor}`
               : `bg-transparent`
         } w-full cursor-pointer items-center gap-4 p-3`}
      >
         <Avatar
            fallback={<Skeleton className={`h-10 w-10 rounded-full`} />}
            isBordered
            radius={"full"}
            color={avatarColor ?? "primary"}
            size={"md"}
            className={`ml-2 aspect-square object-cover`}
            src={getMediaUrl(
               isPrivateGroup
                  ? user?.user?.profilePicture?.mediaUrl!
                  : feedEntry?.chatGroup?.picture?.mediaUrl!
            )}
         />
         <div
            className={`flex flex-1 flex-col items-center justify-evenly gap-0`}
         >
            <div className={`flex w-full items-center justify-between gap-2`}>
               <span className="text-default-800 w-3/4 truncate text-sm font-semibold">
                  {isPrivateGroup
                     ? user?.user?.username
                     : feedEntry?.chatGroup?.name?.substring(0, 20)}
               </span>
               <time className={`text-xxs text-default-500 font-light`}>
                  {feedEntry?.latestMessage?.createdAt
                     ? formatDate(feedEntry.latestMessage.createdAt)
                     : "-"}
               </time>
            </div>
            <ChatFeedEntryMessageSummary
               meId={meId}
               feedEntry={feedEntry}
               isPrivateGroup={isPrivateGroup}
               messageSummary={messageSummary}
            />
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
   const messageSenderId = feedEntry!.latestMessage?.userId;

   if (!messageSenderId && !messageSummary?.length) {
      return (
         <div className={`flex h-4 w-full items-center gap-1 rounded-full`}>
            <span className={`text-default-400 text-xs font-semibold`}>
               No messages yet.
            </span>
         </div>
      );
   }

   return (
      <div className={`flex h-4 w-full items-center gap-1 rounded-full`}>
         {isPrivateGroup && messageSenderId === meId && (
            <span className={`text-default-400 text-xs font-semibold`}>
               You:
            </span>
         )}
         {isPrivateGroup && messageSenderId !== meId && <Fragment />}

         {!isPrivateGroup && messageSenderId === meId && (
            <span className={`text-default-400 text-xs font-semibold`}>
               You:
            </span>
         )}

         {!isPrivateGroup && messageSenderId !== meId && (
            <span className={`text-default-400 text-xs font-semibold`}>
               {feedEntry!.messageSender?.username}:
            </span>
         )}

         <p
            className={`text-default-500 w-full truncate text-[.7rem] font-normal leading-3`}
            dangerouslySetInnerHTML={{
               __html: messageSummary,
            }}
         ></p>
      </div>
   );
};

export default ChatGroupFeedEntry;
