import { messagesClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { DeleteGroupChatMessageModel } from "./useDeleteGroupChatMessageMutation";

export interface DeleteGroupChatMessageReplyModel
   extends DeleteGroupChatMessageModel {}

const deleteGroupChatMessageReply = async (
   model: DeleteGroupChatMessageReplyModel
) => {
   const { messageId, ...request } = model;
   const { status, data } = await messagesClient.delete(
      `replies/${messageId}`,
      {
         headers: {},
         data: request,
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

export const useDeleteGroupChatMessageReplyMutation = () => {
   const client = useQueryClient();

   return useMutation(deleteGroupChatMessageReply, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat message deleted successfully: " + data),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
