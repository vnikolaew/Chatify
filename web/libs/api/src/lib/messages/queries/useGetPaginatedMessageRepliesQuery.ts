import { messagesClient } from "../client";
import { InfiniteData, useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   ChatGroupDetailsEntry,
   ChatGroupMessageEntry,
   ChatMessageReply,
   ChatMessageReplyCursorPagedApiResponse,
   CursorPaged,
   MessageSenderInfoEntry,
   // @ts-ignore
} from "@openapi";

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

export const useGetPaginatedMessageRepliesQuery = (
   model: GetPaginatedMessageRepliesModel
) => {
   const client = useQueryClient();

   return useQuery<ChatMessageReply[], Error, ChatMessageReply[], any>({
      queryKey: [
         `message`,
         model.messageId,
         `replies`,
         model.pageSize,
         model.pagingCursor,
      ],
      queryFn: () => getPaginatedMessageReplies(model),
      cacheTime: 60 * 60 * 1000,
      onSuccess: (data) => {
         data.forEach((reply) => {
            const x = client.getQueryData<
               InfiniteData<CursorPaged<ChatGroupMessageEntry>>
            >([`chat-group`, reply.chatGroupId, `messages`], { exact: false });

            const user = x?.pages
               .flatMap((p) => p.items)
               .find((m) => m.message?.userId === reply.userId)
               ?.senderInfo as MessageSenderInfoEntry;

            console.log("Chat group user: ", user);
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
                  .getQueryData<ChatGroupDetailsEntry>(
                     [`chat-group`, reply.chatGroupId],
                     { exact: true }
                  )
                  ?.members.find((m) => m.id === reply.userId);
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
