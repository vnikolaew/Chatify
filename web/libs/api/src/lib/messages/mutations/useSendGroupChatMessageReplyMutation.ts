import { messagesClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { SendGroupChatMessageModel } from "./useSendGroupChatMessageMutation";

export interface SendGroupChatMessageReplyModel
   extends SendGroupChatMessageModel {
   replyToId: string;
}

const sendGroupChatMessageReply = async (
   model: SendGroupChatMessageReplyModel
) => {
   const { replyToId, ...request } = model;
   const { status, data } = await messagesClient.postForm(
      `replies/${replyToId}`,
      request,
      {
         headers: {},
      }
   );

   if (
      status === HttpStatusCode.BadRequest ||
      status === HttpStatusCode.NotFound
   ) {
      throw new Error("error");
   }

   return data;
};

export const useSendGroupChatMessageReplyMutation = () => {
   const client = useQueryClient();

   return useMutation({
      mutationFn: sendGroupChatMessageReply,
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat message reply sent successfully: " + data),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
