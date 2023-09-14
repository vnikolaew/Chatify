import { messagesClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
//@ts-ignore
import { ChatGroupDetailsEntry, ChatGroupMessageEntry } from "@openapi";
import { produce } from "immer";
import { GetMyClaimsResponse } from "../../auth";

export interface ForwardChatMessageModel {
   messageId: string;
   content: string;
   groupId: string;
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
         onError: (error, { messageId, groupId }) => {},
         onMutate: ({ messageId, groupId }) => {
            const meId = client.getQueryData<GetMyClaimsResponse>([
               `me`,
               `claims`,
            ])?.claims["nameidentifier"];

            let pinnedMessage: ChatGroupMessageEntry;
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
         onSuccess: (data) =>
            console.log("Chat message forwarded successfully: " + data),
         onSettled: (res) => console.log(res),
      }
   );
};
