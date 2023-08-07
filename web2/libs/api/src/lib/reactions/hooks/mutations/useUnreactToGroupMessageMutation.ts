import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { reactionsClient } from "../../client";

export interface UnreactToGroupMessageModel {
   messageId: string;
   chatGroupId: string;
   messageReactionId: string;
}

const unreactToGroupMessage = async (model: UnreactToGroupMessageModel) => {
   const { messageReactionId, ...request } = model;

   const { status, data } = await reactionsClient.delete(
      `${messageReactionId}`,
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

export const useUnreactToGroupMessageMutation = () => {
   const client = useQueryClient();

   return useMutation(unreactToGroupMessage, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Successfully unreacted to chat group message: " + data),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
