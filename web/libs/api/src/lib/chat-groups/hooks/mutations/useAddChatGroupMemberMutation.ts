import { useMutation, useQueryClient } from "@tanstack/react-query";
import { sleep } from "../../../utils";
import { ChatGroupDetailsEntry, User } from "@openapi";

export interface AddChatGroupMemberModel {
   chatGroupId?: string;
   newMemberId?: string;
   membershipType?: number;
}

const addChatGroupMember = async (model: AddChatGroupMemberModel) => {
   // const { status, data } = await chatGroupsClient.post(`members`, model, {
   //    headers: {},
   // });
   //
   //
   // if (status === HttpStatusCode.BadRequest) {
   //    throw new Error("error");
   // }
   //
   await sleep(2000);
   return {};
};

export const useAddChatGroupMember = () => {
   const client = useQueryClient();

   return useMutation<any, Error, AddChatGroupMemberModel, any>(
      addChatGroupMember,
      {
         onError: console.error,
         onSuccess: (data, { chatGroupId, newMemberId }) => {
            console.log("Chat group member added successfully: " + data);
            const friend = client
               .getQueryData<User[]>([`friends`], { exact: true })!
               .find((f) => f.id === newMemberId);

            client.setQueryData<ChatGroupDetailsEntry>(
               [`chat-group`, chatGroupId],
               (old: ChatGroupDetailsEntry | undefined) => ({
                  ...old,
                  members: [...old?.members, friend],
               })
            );
         },
         onSettled: (res) => console.log(res),
         cacheTime: 60 * 60 * 1000,
      }
   );
};
