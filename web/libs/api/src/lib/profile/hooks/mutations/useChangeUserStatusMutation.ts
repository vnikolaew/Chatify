import { profileClient } from "../../client";
import { QueryKey, useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { useGetMyClaimsQuery } from "../../../auth";
import { USER_DETAILS_KEY } from "../queries";
import {
   ChatGroupDetailsEntry,
   User,
   UserDetailsEntry,
   UserStatus,
   // @ts-ignore
} from "@openapi";
import { produce } from "immer";

export interface ChangeUserStatusModel {
   newStatus: string;
}

const changeUserStatus = async (model: ChangeUserStatusModel) => {
   const { status, data } = await profileClient.put(`status`, model, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useChangeUserStatusMutation = () => {
   const client = useQueryClient();
   const { data: userId } = useGetMyClaimsQuery({
      networkMode: "offlineFirst",
      select: (data) => data.claims["nameidentifier"],
   });

   return useMutation({
      mutationFn: changeUserStatus,
      onError: console.error,
      onSuccess: (data, { newStatus }) => {
         console.log("Updating User status to " + newStatus);
         client.setQueryData<UserDetailsEntry>(
            [USER_DETAILS_KEY, userId],
            (current: UserDetailsEntry | undefined) =>
               produce(current, (draft: UserDetailsEntry) => {
                  draft.user = {
                     ...draft.user,
                     status: newStatus as UserStatus,
                  };
               })
         );

         // Find all groups the user is member of:
         const groups = client
            .getQueriesData<ChatGroupDetailsEntry>({
               queryKey: [`chat-group`],
               exact: false,
            })
            .map(([_, group]: [QueryKey, ChatGroupDetailsEntry]) => group)
            .filter(
               (g) =>
                  !!g?.chatGroup &&
                  g?.members?.some((m: User) => m.id === userId)
            );

         groups.forEach((group) => {
            client.setQueryData<ChatGroupDetailsEntry>(
               [`chat-group`, group.chatGroup?.id],
               (group: ChatGroupDetailsEntry) =>
                  produce(group, (draft: ChatGroupDetailsEntry) => {
                     const user = draft?.members?.find(
                        (m: User) => m.id === userId
                     );
                     if (user) user.status = newStatus;
                     return draft;
                  })
            );
         });
      },
   });
};
