import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface LeaveChatGroupModel {
   groupId: string;
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

export const useLeaveChatGroupMutation = () => {
   const client = useQueryClient();

   return useMutation<any, Error, LeaveChatGroupModel, any>({
      mutationFn: leaveChatGroup,
      onError: console.error,
      onSuccess: (data) => console.log("Chat group left successfully: " + data),
      onSettled: (res) => console.log(res),
   });
};
