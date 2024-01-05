import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
//@ts-ignore
import { ChatGroupDetailsEntry, User } from "@openapi";
import { produce } from "immer";

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
   return useMutation<any, Error, AddChatGroupAdminModel, any>({
      mutationFn: addChatGroupAdmin,
      onError: console.error,
      onSuccess: (_, { chatGroupId, newAdminId }) => {
         // Add new admin member to chat group admin Ids:
         client.setQueryData<ChatGroupDetailsEntry>(
            [`chat-group`, chatGroupId],
            (old: ChatGroupDetailsEntry) => {
               return produce(old, (groupDetails: ChatGroupDetailsEntry) => {
                  if (groupDetails.chatGroup?.adminIds) {
                     groupDetails.chatGroup.adminIds.push(newAdminId);
                  }

                  if (groupDetails.chatGroup?.admins) {
                     groupDetails.chatGroup.admins.push(
                        groupDetails.members?.find(
                           (m: User) => m.id === newAdminId
                        )
                     );
                  }

                  groupDetails.chatGroup.updatedAt = new Date().toISOString();
               });
            }
         );
      },
      onSettled: (res) => console.log(res),
   });
};
