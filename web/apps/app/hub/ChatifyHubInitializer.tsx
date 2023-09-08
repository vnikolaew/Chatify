"use client";
import React, { Fragment, useEffect } from "react";
import { useChatifyClientContext } from "./ChatHubConnection";
import { InfiniteData, useQueryClient } from "@tanstack/react-query";
import {
   ChatGroupDetailsEntry,
   ChatGroupFeedEntry,
   ChatGroupMessageEntry,
   CursorPaged,
   UserStatus,
} from "@openapi";
import { GET_PAGINATED_GROUP_MESSAGES_KEY } from "@web/api";
import { produce } from "immer";
import { useCurrentUserId } from "@hooks";

export interface ChatifyHubInitializerProps {}

export const ChatifyHubInitializer = ({}: ChatifyHubInitializerProps) => {
   const client = useChatifyClientContext();
   const meId = useCurrentUserId();
   const queryClient = useQueryClient();

   useEffect(() => {
      client.onReceiveChatGroupMessage((message) => {
         // Update client cache with new message:
         console.log(message.senderId, meId);
         if (message.senderId === meId) return;
         console.log(`New message: `, message);

         queryClient.setQueryData<
            InfiniteData<CursorPaged<ChatGroupMessageEntry>>
         >(
            GET_PAGINATED_GROUP_MESSAGES_KEY(message.chatGroupId),
            (messages) => {
               const member = queryClient
                  .getQueryData<ChatGroupDetailsEntry>([
                     `chat-group`,
                     message.chatGroupId,
                  ])
                  .members.find((m) => m.id === message.senderId);

               return produce(messages, (draft) => {
                  (
                     draft!.pages[0] as CursorPaged<ChatGroupMessageEntry>
                  ).items.unshift({
                     message: {
                        id: message.messageId,
                        chatGroupId: message.chatGroupId,
                        content: message.content,
                        metadata: null!,
                        userId: message.senderId,
                        createdAt: new Date(message.timestamp).toString(),
                        reactionCounts: {},
                     },
                     forwardedMessage: {},
                     repliersInfo: {
                        replierInfos: [],
                        total: 0,
                        lastUpdatedAt: null!,
                     },
                     senderInfo: {
                        profilePictureUrl: member.profilePicture.mediaUrl!,
                        userId: message.senderId,
                        username: message.senderUsername,
                     },
                  });
                  return draft;
               });
            }
         );
         queryClient.setQueryData<ChatGroupFeedEntry[]>([`feed`], (feed) => {
            return produce(feed, (draft) => {
               const feedItemIndex = draft.findIndex(
                  (e) => e.chatGroup.id === message.chatGroupId
               );
               feed[feedItemIndex].latestMessage = {
                  id: message.messageId,
                  chatGroupId: message.chatGroupId,
                  content: message.content,
                  metadata: null!,
                  userId: message.senderId,
                  createdAt: new Date(message.timestamp).toString(),
                  reactionCounts: {},
               };

               return [
                  feed[feedItemIndex],
                  ...feed.slice(0, feedItemIndex),
                  ...feed.slice(feedItemIndex + 1),
               ];
            });
         });
      });

      client.onUserStatusChanged((event) => {
         const chatGroup = queryClient
            .getQueriesData<ChatGroupDetailsEntry>({
               exact: false,
               queryKey: [`chat-group`],
            })
            .map(([_, group]) => group)
            .find((g) => g.members.some((m) => m.id === event.userId));

         if (chatGroup) {
            queryClient.setQueryData<ChatGroupDetailsEntry>(
               [`chat-group`, chatGroup.chatGroup.id],

               (group) => {
                  return produce(group, (draft) => {
                     const user = group.members.find(
                        (m) => m.id === event.userId
                     );
                     user.status = event.newStatus as unknown as UserStatus;
                     return draft;
                  });
               }
            );
         }
      });

      client.onChatGroupMemberStartedTyping((event) => {
         queryClient.setQueryData<Set<string>>(
            [`chat-group`, event.chatGroupId, "typing"],
            (old) =>
               produce(old, (draft) => {
                  draft.add(event.userId);
                  return draft;
               })
         );
      });

      client.onChatGroupMemberStoppedTyping((event) => {
         queryClient.setQueryData<Set<string>>(
            [`chat-group`, event.chatGroupId, "typing"],
            (old) =>
               produce(old, (draft) => {
                  draft.delete(event.userId);
                  return draft;
               })
         );
      });

      client.onChatGroupMessageRemoved((event) => {
         const message = queryClient
            .getQueryData<InfiniteData<CursorPaged<ChatGroupMessageEntry>>>(
               GET_PAGINATED_GROUP_MESSAGES_KEY(event.chatGroupId)
            )
            .pages.flatMap((p) => p.items)
            .find((m) => m.message.id === event.messageId);

         if (message) {
            queryClient.setQueryData<
               InfiniteData<CursorPaged<ChatGroupMessageEntry>>
            >(
               GET_PAGINATED_GROUP_MESSAGES_KEY(event.chatGroupId),
               (messages) => {
                  return produce(messages, (draft) => {
                     const message = draft.pages
                        .flatMap((p) => p.items)
                        .find((m) => m.message.id === event.messageId);

                     message.message.metadata.deleted = true as any;
                     message.message.metadata.updatedAt = event.timestamp;
                     return draft;
                  });
               }
            );
         }
      });

      client.onChatGroupMessageEdited((event) => {
         const message = queryClient
            .getQueryData<InfiniteData<CursorPaged<ChatGroupMessageEntry>>>(
               GET_PAGINATED_GROUP_MESSAGES_KEY(event.chatGroupId)
            )
            .pages.flatMap((p) => p.items)
            .find((m) => m.message.id === event.messageId);
         if (message) {
            queryClient.setQueryData<
               InfiniteData<CursorPaged<ChatGroupMessageEntry>>
            >(
               GET_PAGINATED_GROUP_MESSAGES_KEY(event.chatGroupId),
               (messages) => {
                  return produce(messages, (draft) => {
                     const message = draft.pages
                        .flatMap((p) => p.items)
                        .find((m) => m.message.id === event.messageId);

                     message.message.metadata.edited = true as any;
                     message.message.metadata.updatedAt = event.timestamp;
                     message.message.content = event.newContent;
                     return draft;
                  });
               }
            );
         }
      });

      client.onTest((groupId, value) =>
         console.log(`New message: ${value} from group ${groupId}.`)
      );

      client
         .start()
         .then(() => console.log(`Client successfully started ...`))
         .catch((err) => console.log(`An error occurred: ${err} ...`));

      return () => {
         client
            .stop()
            .then((_) => console.log(`Client successfully disconnected`))
            .catch(console.error);
      };
   }, []);

   return <Fragment />;
};
