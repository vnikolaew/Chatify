import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { reactionsClient } from "../../client";

export interface ReactToGroupMessageModel {
   messageId: string;
   chatGroupId: string;
   reactionType: number;
}

const reactToGroupMessage = async (model: ReactToGroupMessageModel) => {
   const { messageId, ...request } = model;
   const { status, data } = await reactionsClient.post(
      `${messageId}`,
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

export const useReactToGroupMessageMutation = () => {
   const client = useQueryClient();

   return useMutation(reactToGroupMessage, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Successfully reacted to chat group message: " + data),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
