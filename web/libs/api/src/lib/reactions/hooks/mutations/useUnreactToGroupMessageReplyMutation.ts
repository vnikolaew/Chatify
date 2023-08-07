import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { reactionsClient } from "../../client";
import { UnreactToGroupMessageModel } from "./useUnreactToGroupMessageMutation";

export interface UnreactToGroupMessageReplyModel
   extends UnreactToGroupMessageModel {}

const unreactToGroupMessageReply = async (
   model: UnreactToGroupMessageReplyModel
) => {
   const { messageReactionId, ...request } = model;

   const { status, data } = await reactionsClient.delete(
      `replies/${messageReactionId}`,
      {
         headers: {},
         data: request,
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useUnreactToGroupMessageReplyMutation = () => {
   const client = useQueryClient();

   return useMutation(unreactToGroupMessageReply, {
      onError: console.error,
      onSuccess: (data) =>
         console.log(
            "Successfully unreacted to chat group message reply: " + data
         ),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
