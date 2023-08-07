import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface EditChatGroupModel {
   chatGroupId: string;
   about?: string;
   name?: string;
   file?: Blob;
}

const editChatGroup = async (model: EditChatGroupModel) => {
   const { chatGroupId, ...request } = model;
   const { status, data } = await chatGroupsClient.patchForm(
      `/${chatGroupId}`,
      request,
      {
         headers: {
            "Content-Type": "multipart/form-data",
         },
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useEditChatGroupMutation = () => {
   const client = useQueryClient();
   return useMutation(editChatGroup, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Chat group edited successfully: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
