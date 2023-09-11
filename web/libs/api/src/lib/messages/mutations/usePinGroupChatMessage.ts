import { messagesClient } from "../client";
import {
   InfiniteData,
   useMutation,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   ChatGroupDetailsEntry,
   ChatGroupMessageEntry,
   ChatMessage,
} from "@openapi";
import { produce } from "immer";
import { GetMyClaimsResponse } from "../../auth";
import { GET_PINNED_GROUP_MESSAGES_KEY } from "../../chat-groups";
import { GET_PAGINATED_GROUP_MESSAGES_KEY } from "../queries";
import { CursorPaged } from "../../../../openapi/common/CursorPaged";

export interface PinGroupChatMessageModel {
   messageId: string;
   groupId: string;
}

const pinGroupChatMessage = async ({ messageId }: PinGroupChatMessageModel) => {
   const { status, data } = await messagesClient.post(`/pins/${messageId}`, {
      headers: {},
      data: {},
   });

   if (
      status === HttpStatusCode.BadRequest ||
      status == HttpStatusCode.NotFound
   ) {
      throw new Error("error");
   }

   return data;
};

export const usePinGroupChatMessage = () => {
   const client = useQueryClient();

   return useMutation<any, Error, PinGroupChatMessageModel, any>(
      pinGroupChatMessage,
      {
         onError: console.error,
         onSuccess: (data, { messageId, groupId }) => {
            console.log("Chat message pinned successfully: " + data);
            const meId = client.getQueryData<GetMyClaimsResponse>([
               `me`,
               `claims`,
            ])?.claims["nameidentifier"];

            client.setQueryData<ChatMessage[]>(
               GET_PINNED_GROUP_MESSAGES_KEY(groupId),
               (old) => {
                  return produce(old, (draft) => {
                     const message = client
                        .getQueryData<
                           InfiniteData<CursorPaged<ChatGroupMessageEntry>>
                        >(GET_PAGINATED_GROUP_MESSAGES_KEY(groupId))
                        ?.pages.flatMap((p) => p.items)
                        .find((m) => m?.message?.id === messageId);

                     if (message?.message) draft?.push(message?.message);
                     return draft;
                  });
               }
            );

            client.setQueryData<ChatGroupDetailsEntry>(
               [`chat-group`, groupId],
               (old: ChatGroupDetailsEntry) => {
                  return produce(old, (draft: ChatGroupDetailsEntry) => {
                     draft.chatGroup?.pinnedMessages?.push({
                        messageId,
                        createdAt: new Date().toISOString(),
                        pinnerId: meId,
                     });
                     return draft;
                  });
               }
            );
         },
         onSettled: (res) => console.log(res),
      }
   );
};
