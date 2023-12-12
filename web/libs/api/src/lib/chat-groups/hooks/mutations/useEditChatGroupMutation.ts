import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { ChatGroup, ChatGroupDetailsEntry } from "@openapi";
import { produce } from "immer";

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
      },
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useEditChatGroupMutation = () => {
   const client = useQueryClient();
   return useMutation<any, Error, EditChatGroupModel, any>(editChatGroup, {
      onError: console.error,
      onSuccess: (data, { chatGroupId, about }) => {
         console.log("Chat group edited successfully: " + data);
         const chatGroup = client.getQueryData<ChatGroupDetailsEntry>([`chat-group`, chatGroupId], { exact: true });
         client.setQueryData<ChatGroupDetailsEntry>([`chat-group`, chatGroupId], (old: ChatGroupDetailsEntry) => {
            if (!old) return old;
            return produce(old, (group: ChatGroupDetailsEntry) => {
               if (group.chatGroup?.about && about) group.chatGroup.about = about;
               return group;
            });
         });

      },
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
