import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { reactionsClient } from "../../client";
import { ReactToGroupMessageModel } from "./useReactToGroupMessageMutation";

export interface ReactToGroupMessageReplyModel
   extends ReactToGroupMessageModel {}

const reactToGroupMessageReply = async (
   model: ReactToGroupMessageReplyModel
) => {
   const { messageId, ...request } = model;
   const { status, data } = await reactionsClient.post(
      `replies/${messageId}`,
      request,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useReactToGroupMessageReplyMutation = () => {
   const client = useQueryClient();

   return useMutation<any, Error, ReactToGroupMessageModel, any>(reactToGroupMessageReply, {
      onError: console.error,
      onSuccess: (data) =>
         console.log(
            "Successfully reacted to chat group message reply: " + data
         ),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
