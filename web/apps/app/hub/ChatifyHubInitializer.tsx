"use client";
import React, { Fragment, useEffect } from "react";
import { useChatifyClientContext } from "./ChatHubConnection";
import { InfiniteData, useQueryClient } from "@tanstack/react-query";
import {
   ChatGroupDetailsEntry,
   ChatGroupFeedEntry,
   ChatGroupMessageEntry,
   ChatMessageReaction,
   CursorPaged,
   UserNotification,
   UserNotificationType,
   UserStatus,
} from "@openapi";
import {
   GET_ALL_REACTIONS_KEY,
   GET_PAGINATED_GROUP_MESSAGES_KEY,
   GetMyClaimsResponse,
   NOTIFICATIONS_KEY,
   USER_DETAILS_KEY,
} from "@web/api";
import { enableMapSet, produce } from "immer";

export interface ChatifyHubInitializerProps {
}

export interface IUserTyping {
   userId: string;
   username: string;
   groupId: string;
}

export const ChatifyHubInitializer = ({}: ChatifyHubInitializerProps) => {
   const client = useChatifyClientContext();
   const queryClient = useQueryClient();

   useEffect(() => {
      enableMapSet();
      client.onReceiveChatGroupMessage((message) => {
         // Update client cache with new message:
         const meId = queryClient.getQueryData<GetMyClaimsResponse>([
            `me`,
            `claims`,
         ]).claims.nameidentifier;
         if (message.senderId === meId) {
            // Only update message attachments:
            queryClient.setQueryData<
               InfiniteData<CursorPaged<ChatGroupMessageEntry>>
            >(
               GET_PAGINATED_GROUP_MESSAGES_KEY(message.chatGroupId),
               (messages) => {
                  return produce(messages, (draft) => {
                     (
                        draft!.pages[0] as CursorPaged<ChatGroupMessageEntry>
                     ).items[0].message.attachments = message.attachments;
                     return draft;
                  });
               },
            );
            return;
         }

         console.log(`New message: `, message);

         // Update users typing:
         queryClient.setQueryData<Set<IUserTyping>>(
            [`chat-group`, message.chatGroupId, `typing`],
            (old) =>
               produce(
                  old,
                  () =>
                     new Set(
                        [...old].filter((u) => u.userId !== message.senderId),
                     ),
               ),
         );

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

               const newMessagePages = produce(messages, (draft) => {
                  (
                     draft!.pages[0] as CursorPaged<ChatGroupMessageEntry>
                  ).items.push({
                     message: {
                        id: message.messageId,
                        chatGroupId: message.chatGroupId,
                        content: message.content,
                        metadata: null!,
                        userId: message.senderId,
                        createdAt: new Date(message.timestamp).toString(),
                        reactionCounts: {},
                        attachments: message.attachments,
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
               console.log({ newMessagePages });
               return newMessagePages;
            },
         );
         queryClient.setQueryData<ChatGroupFeedEntry[]>([`feed`], (feed) => {
            return produce(feed, (draft) => {
               const feedItemIndex = draft.findIndex(
                  (e) => e.chatGroup.id === message.chatGroupId,
               );
               const latestMessage = {
                  id: message.messageId,
                  chatGroupId: message.chatGroupId,
                  content: message.content,
                  metadata: null!,
                  userId: message.senderId,
                  createdAt: new Date(message.timestamp).toString(),
                  reactionCounts: {},
                  attachments: message.attachments,
               };

               return [
                  {
                     ...feed[feedItemIndex],
                     latestMessage,
                  },
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
                        (m) => m.id === event.userId,
                     );
                     user.status = event.newStatus as unknown as UserStatus;
                     return draft;
                  });
               },
            );
         }
      });

      client.onChatGroupMemberStartedTyping(
         ({ chatGroupId, userId, username }) => {
            console.log(`Member started typing: `, userId);
            queryClient.setQueryData<Set<IUserTyping>>(
               [`chat-group`, chatGroupId, "typing"],
               (old) =>
                  produce(old, (draft) => {
                     if (!draft) draft = new Set<IUserTyping>();
                     draft.add({ userId, username, groupId: chatGroupId });
                     return draft;
                  }),
            );
         },
      );

      client.onChatGroupMemberStoppedTyping(
         ({ chatGroupId, userId, timestamp, username }) => {
            console.log(`Member stopped typing: `, userId);
            queryClient.setQueryData<Set<IUserTyping>>(
               [`chat-group`, chatGroupId, "typing"],
               (old) =>
                  produce(old, (draft) => {
                     if (!draft) draft = new Set<IUserTyping>();
                     for (const element of draft) {
                        if (element.userId === userId) {
                           draft.delete(element);
                           return draft;
                        }
                     }
                     return draft;
                  }),
            );
         },
      );

      client.onChatGroupMessageUnReactedTo((event) => {
         queryClient.setQueryData<
            InfiniteData<CursorPaged<ChatGroupMessageEntry>>
         >(GET_PAGINATED_GROUP_MESSAGES_KEY(event.chatGroupId), (old) => {
            return produce(old, (draft) => {
               const message = draft.pages
                  .flatMap((_) => _.items)
                  .find(
                     (m) => m.message.id === event.messageId,
                  ) as ChatGroupMessageEntry;

               if (
                  message.message.reactionCounts[
                     event.reactionType.toString()
                     ] > 0
               ) {
                  message.message.reactionCounts[
                     event.reactionType.toString()
                     ]--;
               }
               return draft;
            });
         });

         queryClient.setQueryData<ChatMessageReaction[]>(
            GET_ALL_REACTIONS_KEY(event.messageId),
            (old) => {
               return produce(old, (draft) => {
                  draft = draft.filter((r) => r.id !== event.messageReactionId);
                  return draft;
               });
            },
         );
      });

      client.onChatGroupMessageReactedTo((event) => {
         queryClient.setQueryData<
            InfiniteData<CursorPaged<ChatGroupMessageEntry>>
         >(GET_PAGINATED_GROUP_MESSAGES_KEY(event.chatGroupId), (old) => {
            return produce(old, (draft) => {
               const message = draft.pages
                  .flatMap((_) => _.items)
                  .find(
                     (m) => m.message.id === event.messageId,
                  ) as ChatGroupMessageEntry;

               // string -> number
               message.message.reactionCounts[
                  event.reactionType.toString()
                  ] ??= 0;
               message.message.reactionCounts[event.reactionType.toString()]++;
               return draft;
            });
         });

         queryClient.setQueryData<ChatMessageReaction[]>(
            GET_ALL_REACTIONS_KEY(event.messageId),
            (old) => {
               return produce(old, (draft) => {
                  draft?.push({
                     messageId: event.messageId,
                     reactionCode: event.reactionType,
                     userId: event.userId,
                     chatGroupId: event.chatGroupId,
                     createdAt: event.timestamp,
                     id: event.messageReactionId,
                  });
                  return draft;
               });
            },
         );
      });

      client.onChatGroupNewAdminAdded((event) => {
         queryClient.setQueryData<ChatGroupDetailsEntry>(
            [`chat-group`, event.chatGroupId],
            (old) => {
               return produce(old, (draft) => {
                  const existing = draft.members.find(
                     (m) => m.id === event.adminId,
                  );
                  if (existing) {
                     draft.chatGroup.adminIds.push(existing.id);
                     draft.chatGroup.admins.push(existing);
                  } else {
                     draft.chatGroup.adminIds.push(event.adminId);
                  }

                  return draft;
               });
            },
         );
      });

      client.onChatGroupMessageRemoved((event) => {
         queryClient.setQueryData<
            InfiniteData<CursorPaged<ChatGroupMessageEntry>>
         >(GET_PAGINATED_GROUP_MESSAGES_KEY(event.chatGroupId), (messages) => {
            return produce(messages, (draft) => {
               const message = draft.pages
                  .flatMap((p) => p.items)
                  .find((m) => m.message.id === event.messageId);

               if (message) {
                  message.message.metadata.deleted = true as any;
                  message.message.metadata.updatedAt = event.timestamp;
               }

               return draft;
            });
         });
      });

      client.onChatGroupMessageEdited((event) => {
         queryClient.setQueryData<
            InfiniteData<CursorPaged<ChatGroupMessageEntry>>
         >(GET_PAGINATED_GROUP_MESSAGES_KEY(event.chatGroupId), (messages) => {
            return produce(messages, (draft) => {
               const message = draft.pages
                  .flatMap((p) => p.items)
                  .find((m) => m.message.id === event.messageId);

               if (message) {
                  message.message.metadata.edited = true as any;
                  message.message.metadata.updatedAt = event.timestamp;
                  message.message.content = event.newContent;
               }
               return draft;
            });
         });
      });

      client.onFriendInvitationAccepted((event) => {
         queryClient.setQueriesData<CursorPaged<UserNotification>>(
            { queryKey: [NOTIFICATIONS_KEY], exact: false },
            (old) => {
               if (old.pagingCursor) return old;
               return produce(old, (draft) => {
                  draft?.items?.unshift({
                     type: UserNotificationType.ACCEPTED_FRIEND_INVITE,
                     createdAt: new Date().toISOString(),
                     userId: event.inviteeId,
                     read: false,
                     metadata: event.metadata,
                     summary: `${event.inviteeUsername} accepted your friend request.`,
                     user: {
                        id: event.inviteeId,
                        username: event.inviteeUsername,
                     },
                  });
               });
            },
         );
      });

      client.onFriendInvitationDeclined((event) => {
         queryClient.setQueriesData<CursorPaged<UserNotification>>(
            { queryKey: [NOTIFICATIONS_KEY], exact: false },
            (old) => {
               if (old.pagingCursor) return old;
               return produce(old, (draft) => {
                  draft?.items?.unshift({
                     type: UserNotificationType.DECLINED_FRIEND_INVITE,
                     createdAt: new Date().toISOString(),
                     userId: event.inviteeId,
                     read: false,
                     metadata: event.metadata,
                     summary: `${event.inviteeUsername} declined your friend request.`,
                     user: {
                        id: event.inviteeId,
                        username: event.inviteeUsername,
                     },
                  });
               });
            },
         );
      });

      client.onFriendInvitationReceived((event) => {
         console.log(`New friend invite: `, event);
         queryClient.setQueriesData<CursorPaged<UserNotification>>(
            { queryKey: [NOTIFICATIONS_KEY], exact: false },
            (old) => {
               if (old.pagingCursor) return old;
               return produce(old, (draft) => {
                  draft?.items?.unshift({
                     type: UserNotificationType.INCOMING_FRIEND_INVITE,
                     createdAt: new Date().toISOString(),
                     userId: event.inviterId,
                     read: false,
                     metadata: event.metadata,
                     user: {
                        id: event.inviterId,
                        username: event.inviterUsername,
                     },
                  });
               });
            },
         );
      });

      client.onChatGroupMemberLeft((event) => {
         queryClient.setQueryData<ChatGroupDetailsEntry>(
            [`chat-group`, event.chatGroupId],
            (old) => {
               return produce(old, (draft) => {
                  draft.members = draft.members.filter(
                     (m) => m.id !== event.userId,
                  );
                  return draft;
               });
            },
         );
      });

      client.onChatGroupMemberAdded((event) => {
         queryClient.setQueryData<ChatGroupDetailsEntry>(
            [`chat-group`, event.chatGroupId],
            (old) => {
               queryClient.getQueryData([USER_DETAILS_KEY, event.userId]);
               return produce(old, (draft) => {
                  draft.members.push({
                     username: event.username,
                     id: event.userId,
                  });
                  return draft;
               });
            },
         );
      });

      client.onChatGroupMemberRemoved((event) => {
         queryClient.setQueryData<ChatGroupDetailsEntry>(
            [`chat-group`, event.chatGroupId],
            (old) => {
               return produce(old, (draft) => {
                  draft.members = draft.members.filter(
                     (m) => m.id !== event.userId,
                  );
                  return draft;
               });
            },
         );
      });

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
