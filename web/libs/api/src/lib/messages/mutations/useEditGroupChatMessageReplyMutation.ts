import { messagesClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   EditGroupChatMessageModel,
   useEditGroupChatMessageMutation,
} from "./useEditGroupChatMessageMutation";

export interface EditGroupChatMessageReplyModel
   extends EditGroupChatMessageModel {}

const editGroupChatMessageReply = async (
   model: EditGroupChatMessageReplyModel
) => {
   const { messageId, ...request } = model;

   const { status, data } = await messagesClient.put(
      `replies/${messageId}`,
      request,
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

export const useEditGroupChatMessageReplyMutation = () => {
   const client = useQueryClient();

   return useMutation({
      mutationFn: editGroupChatMessageReply,
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat message reply edited successfully: " + data),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
