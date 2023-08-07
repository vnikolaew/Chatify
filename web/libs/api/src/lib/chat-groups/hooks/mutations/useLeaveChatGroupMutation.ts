import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface LeaveChatGroupModel {
   chatGroupId: string;
   reason?: string;
}

const leaveChatGroup = async (model: LeaveChatGroupModel) => {
   const { status, data } = await chatGroupsClient.post(`leave`, model, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useLeaveChatGroup = () => {
   const client = useQueryClient();

   return useMutation(leaveChatGroup, {
      onError: console.error,
      onSuccess: (data) => console.log("Chat group left successfully: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
