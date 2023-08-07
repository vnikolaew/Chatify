import { messagesClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface SendGroupChatMessageModel {
   chatGroupId: string;
   content: string;
   files?: Blob[];
   metadata?: Record<string, string>;
}

const sendGroupChatMessage = async (model: SendGroupChatMessageModel) => {
   const { status, data } = await messagesClient.postForm(
      `${model.chatGroupId}`,
      model,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useSendGroupChatMessageMutation = () => {
   const client = useQueryClient();

   return useMutation(sendGroupChatMessage, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat message sent successfully: " + data),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
