import { profileClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { useGetMyClaimsQuery } from "../../../auth";
import { USER_DETAILS_KEY } from "../queries";
import {
   ChatGroupDetailsEntry,
   User,
   UserDetailsEntry,
   UserStatus,
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
   return useMutation(changeUserStatus, {
      onError: console.error,
      onSuccess: (data, { newStatus }) => {
         console.log("User status changed successfully: " + data);
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

         client.setQueriesData<ChatGroupDetailsEntry>(
            [`chat-group`],
            (group: ChatGroupDetailsEntry) =>
               produce(group, (draft: ChatGroupDetailsEntry) => {
                  const user = draft.members?.find(
                     (m: User) => m.userId === userId
                  );
                  user.status = newStatus;
                  return draft;
               })
         );
      },
      onSettled: (res) => console.log(res),
   });
};
