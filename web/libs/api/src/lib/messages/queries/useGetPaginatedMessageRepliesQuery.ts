import { messagesClient } from "../client";
import {
   InfiniteData,
   useInfiniteQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   ChatGroupDetailsEntry,
   ChatGroupMessageEntry,
   ChatMessageReply,
   ChatMessageReplyCursorPagedApiResponse,
   CursorPaged,
   MessageSenderInfoEntry,
   User,
   // @ts-ignore
} from "@openapi";
import { GET_PAGINATED_GROUP_MESSAGES_KEY } from "./useGetPaginatedGroupMessagesQuery";
import { DEFAULT_CACHE_TIME } from "../../constants";

export interface GetPaginatedMessageRepliesModel {
   messageId: string;
   pageSize: number;
   pagingCursor: string;
}

const getPaginatedMessageReplies = async (
   model: GetPaginatedMessageRepliesModel
): Promise<ChatMessageReply[]> => {
   const { messageId, pageSize, pagingCursor } = model;
   const params = new URLSearchParams({
      pageSize: pageSize.toString(),
   });
   if (!!pagingCursor) params.set("pagingCursor", pagingCursor);

   const { status, data } =
      await messagesClient.get<ChatMessageReplyCursorPagedApiResponse>(
         `${messageId}/replies`,
         {
            headers: {},
            params,
         }
      );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data;
};

export const GET_PAGINATED_GROUP_MESSAGE_REPLIES_KEY = (messageId: string) => [
   `message`,
   messageId,
   `replies`,
];

export const useGetPaginatedMessageRepliesQuery = (
   model: GetPaginatedMessageRepliesModel
) => {
   const client = useQueryClient();

   return useInfiniteQuery<
      CursorPaged<ChatMessageReply>,
      Error,
      CursorPaged<ChatMessageReply>,
      any
   >({
      queryKey: GET_PAGINATED_GROUP_MESSAGE_REPLIES_KEY(model.messageId),
      queryFn: () => getPaginatedMessageReplies(model),
      getNextPageParam: (lastPage) => {
         return lastPage.pagingCursor;
      },
      getPreviousPageParam: (_, allPages) => allPages.at(-1)?.pagingCursor,
      gcTime: DEFAULT_CACHE_TIME,
      onSuccess: (data: InfiniteData<CursorPaged<ChatMessageReply>>) => {
         data.pages
            .flatMap((_) => _)
            .forEach((reply) => {
               const x = client.getQueryData<
                  InfiniteData<CursorPaged<ChatGroupMessageEntry>>
               >(GET_PAGINATED_GROUP_MESSAGES_KEY(reply.chatGroupId));

               const user = x?.pages
                  .flatMap((p) => p.items)
                  .find((m) => m.message?.userId === reply.userId)
                  ?.senderInfo as MessageSenderInfoEntry;

               if (user) {
                  reply.user = {
                     id: user?.userId,
                     username: user?.username,
                     profilePicture: {
                        mediaUrl: user?.profilePictureUrl,
                     },
                  };
               } else {
                  const chatGroupUser = client
                     .getQueryData<ChatGroupDetailsEntry>([
                        `chat-group`,
                        reply.chatGroupId,
                     ])
                     ?.members.find((m: User) => m.id === reply.userId);
                  if (chatGroupUser) {
                     reply.user = {
                        id: chatGroupUser.id,
                        username: chatGroupUser.username,
                        profilePicture: chatGroupUser.profilePicture,
                     };
                  }
               }
            });
      },
   });
};
