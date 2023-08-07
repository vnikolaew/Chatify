import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface AddChatGroupMemberModel {
   chatGroupId?: string;
   newMemberId?: string;
   membershipType?: number;
}

const addChatGroupMember = async (model: AddChatGroupMemberModel) => {
   const { status, data } = await chatGroupsClient.post(`members`, model, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useAddChatGroupMember = () => {
   const client = useQueryClient();

   return useMutation(addChatGroupMember, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat group member added successfully: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
