import { friendshipsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { USER_DETAILS_KEY } from "../../../profile";
import { FriendInvitationStatus, UserDetailsEntry } from "@openapi";
import { produce } from "immer";

export interface SendFriendInvitationModel {
   userId: string;
}

const sendFriendInvite = async (model: SendFriendInvitationModel) => {
   const { status, data } = await friendshipsClient.post(
      `invite/${model.userId}`,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useSendFriendInviteMutation = () => {
   const client = useQueryClient();

   return useMutation<any, Error, SendFriendInvitationModel, any>(
      sendFriendInvite,
      {
         onError: console.error,
         onSuccess: (data, { userId }) => {
            console.log("Friend invite sent successfully: " + data);

            // Update friend invitation user details:
            client.setQueryData<UserDetailsEntry>(
               [USER_DETAILS_KEY, userId],
               (old: UserDetailsEntry) => {
                  if (!old) return old;
                  return produce(old, (draft: UserDetailsEntry) => {
                     if (draft.friendInvitation) {
                        draft.friendInvitation.status =
                           FriendInvitationStatus.PENDING;
                        draft.friendInvitation.createdAt =
                           new Date().toISOString();
                     }
                     return draft;
                  });
               }
            );
         },
         onSettled: (res) => console.log(res),
         cacheTime: 60 * 60 * 1000,
      }
   );
};
