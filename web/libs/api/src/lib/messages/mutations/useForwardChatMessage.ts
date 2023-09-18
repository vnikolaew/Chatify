import { messagesClient } from "../client";
import {
   InfiniteData,
   useMutation,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
//@ts-ignore
import { ChatGroupMessageEntry, CursorPaged, UserDetailsEntry } from "@openapi";
import { GetMyClaimsResponse } from "../../auth";
import { queryClient } from "../../queryClient";
import { produce } from "immer";
import { GET_PAGINATED_GROUP_MESSAGES_KEY } from "../queries";

export interface ForwardChatMessageModel {
   messageId: string;
   groupId: string;
   content: string;
   groupIds: string[];
}

const forwardChatMessage = async (model: ForwardChatMessageModel) => {
   const { status, data } = await messagesClient.post(
      `/forward/${model.messageId}`,
      model,
      {
         headers: {},
      }
   );

   if (
      status === HttpStatusCode.BadRequest ||
      status == HttpStatusCode.NotFound
   ) {
      throw new Error("error");
   }

   return data;
};

export const useForwardChatMessage = () => {
   const client = useQueryClient();

   return useMutation<any, Error, ForwardChatMessageModel, any>(
      forwardChatMessage,
      {
         onError: (error, { messageId, groupIds }) => {
            groupIds.forEach((groupId) => {
               queryClient.setQueryData<
                  InfiniteData<CursorPaged<ChatGroupMessageEntry>>
               >(GET_PAGINATED_GROUP_MESSAGES_KEY(groupId), (old) => {
                  return produce(
                     old,
                     (
                        draft: InfiniteData<CursorPaged<ChatGroupMessageEntry>>
                     ) => {
                        draft?.pages?.[0]?.items?.shift();
                     }
                  );
               });
            });
            console.error(error);
         },
         onMutate: ({ messageId, groupIds, groupId, content }) => {
            const meId = client.getQueryData<GetMyClaimsResponse>([
               `me`,
               `claims`,
            ])?.claims["nameidentifier"];
            const me = client.getQueryData<UserDetailsEntry>([
               `user-details`,
               meId,
            ]);

            const forwardedMessage = queryClient
               .getQueryData<InfiniteData<CursorPaged<ChatGroupMessageEntry>>>(
                  GET_PAGINATED_GROUP_MESSAGES_KEY(groupId)
               )
               ?.pages.flatMap((_) => _.items)
               .find((m) => m.message?.id === messageId);

            // Retrieve all groups with the ids:
            groupIds.forEach((groupId) => {
               queryClient.setQueryData<
                  InfiniteData<CursorPaged<ChatGroupMessageEntry>>
               >(GET_PAGINATED_GROUP_MESSAGES_KEY(groupId), (old) => {
                  return produce(
                     old,
                     (
                        draft: InfiniteData<CursorPaged<ChatGroupMessageEntry>>
                     ) => {
                        draft?.pages?.[0]?.items?.unshift({
                           message: {
                              userId: meId,
                              chatGroupId: groupId,
                              content,
                              createdAt: new Date().toISOString(),
                              attachments: [],
                           },
                           repliersInfo: { total: 0, replierInfos: [] },
                           senderInfo: {
                              userId: meId,
                              username: me.user?.username,
                              profilePictureUrl:
                                 me.user?.profilePicture?.mediaUrl,
                           },
                           forwardedMessage,
                        });
                     }
                  );
               });
            });
         },
         onSuccess: (data) =>
            console.log("Chat message forwarded successfully: " + data),
         onSettled: (res) => console.log(res),
      }
   );
};
