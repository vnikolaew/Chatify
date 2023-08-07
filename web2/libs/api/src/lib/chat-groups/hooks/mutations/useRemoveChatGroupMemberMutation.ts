import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface RemoveChatGroupMemberModel {
   chatGroupId?: string;
   memberId?: string;
}

const removeChatGroupMember = async (model: RemoveChatGroupMemberModel) => {
   const { status, data } = await chatGroupsClient.delete(`members`, {
      headers: {},
      data: model,
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useRemoveChatGroupMember = () => {
   const client = useQueryClient();

   return useMutation(removeChatGroupMember, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat group member removed successfully: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
