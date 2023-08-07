import { messagesClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface EditGroupChatMessageModel {
   chatGroupId: string;
   messageId: string;
   newContent: string;
}

const editGroupChatMessage = async (model: EditGroupChatMessageModel) => {
   const { messageId, ...request } = model;
   const { status, data } = await messagesClient.put(`${messageId}`, request, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useEditGroupChatMessageMutation = () => {
   const client = useQueryClient();

   return useMutation(editGroupChatMessage, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat message edited successfully: " + data),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
