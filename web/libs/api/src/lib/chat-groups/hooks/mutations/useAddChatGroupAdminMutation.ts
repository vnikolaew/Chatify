import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface AddChatGroupAdminModel {
   chatGroupId: string;
   newAdminId: string;
}

const addChatGroupAdmin = async (model: AddChatGroupAdminModel) => {
   const { status, data } = await chatGroupsClient.post(`admins`, model, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useAddChatGroupAdmin = () => {
   const client = useQueryClient();

   return useMutation(addChatGroupAdmin, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat group admin added successfully: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
