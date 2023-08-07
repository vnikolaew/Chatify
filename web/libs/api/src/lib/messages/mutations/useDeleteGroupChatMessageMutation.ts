import { messagesClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface DeleteGroupChatMessageModel {
   chatGroupId: string;
   messageId: string;
}

const deleteGroupChatMessage = async (model: DeleteGroupChatMessageModel) => {
   const { messageId, ...request } = model;
   const { status, data } = await messagesClient.delete(`${messageId}`, {
      headers: {},
      data: request,
   });

   if (
      status === HttpStatusCode.BadRequest ||
      status == HttpStatusCode.NotFound
   ) {
      throw new Error("error");
   }

   return data;
};

export const useDeleteGroupChatMessageMutation = () => {
   const client = useQueryClient();

   return useMutation(deleteGroupChatMessage, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat message deleted successfully: " + data),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
