import { chatGroupsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
//@ts-ignore
import { ChatGroupDetailsEntry, User } from "@openapi";
import { produce } from "immer";

export interface RemoveChatGroupAdminModel {
   chatGroupId: string;
   adminId: string;
}

const removeChatGroupAdmin = async ({
   adminId,
   chatGroupId,
}: RemoveChatGroupAdminModel) => {
   const { status, data } = await chatGroupsClient.delete(
      `${chatGroupId}/admins/${adminId}`,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useRemoveChatGroupAdmin = () => {
   const client = useQueryClient();
   return useMutation<any, Error, RemoveChatGroupAdminModel, any>({
      mutationFn: removeChatGroupAdmin,
      onError: console.error,
      onSuccess: (_, { chatGroupId, adminId }) => {
         // Remove admin member from chat group admin Ids / Admins:
         client.setQueryData<ChatGroupDetailsEntry>(
            [`chat-group`, chatGroupId],
            (old: ChatGroupDetailsEntry) => {
               return produce(old, (groupDetails: ChatGroupDetailsEntry) => {
                  if (groupDetails.chatGroup?.adminIds) {
                     groupDetails.chatGroup.adminIds =
                        groupDetails.chatGroup.adminIds.filter(
                           (id: string) => id !== adminId
                        );
                  }

                  if (groupDetails.chatGroup?.admins) {
                     groupDetails.chatGroup.admins =
                        groupDetails.chatGroup.admins?.filter(
                           (admin: User) => admin.id !== adminId
                        ) ?? [];
                  }

                  groupDetails.chatGroup.updatedAt = new Date().toISOString();
               });
            }
         );
      },
      onSettled: (res) => console.log(res),
   });
};
