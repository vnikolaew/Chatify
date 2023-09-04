import { messagesClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface PinGroupChatMessageModel {
   messageId: string;
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
         onSuccess: (data) =>
            console.log("Chat message pinned successfully: " + data),
         onSettled: (res) => console.log(res),
      }
   );
};
