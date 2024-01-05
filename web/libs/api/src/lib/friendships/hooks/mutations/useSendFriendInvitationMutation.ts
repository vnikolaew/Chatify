import { friendshipsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { USER_DETAILS_KEY } from "../../../profile";
import { FriendInvitationStatus, UserDetailsEntry } from "@openapi";
import { produce } from "immer";
import { GetMyClaimsResponse } from "../../../auth";

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

   return useMutation<any, Error, SendFriendInvitationModel, any>({
      mutationFn: sendFriendInvite,
      onError: console.error,
      onSuccess: (data, { userId }) => {
         console.log("Friend invite sent successfully: " + data);
         const meId = client.getQueryData<GetMyClaimsResponse>([`me`, `claims`])
            ?.claims?.["nameidentifier"];

         // Update friend invitation user details:
         client.setQueryData<UserDetailsEntry>(
            [USER_DETAILS_KEY, userId],
            (old: UserDetailsEntry) => {
               if (!old) return old;
               return produce(old, (draft: UserDetailsEntry) => {
                  draft.friendInvitation = {
                     ...draft.friendInvitation,
                     status: FriendInvitationStatus.PENDING,
                     createdAt: new Date().toISOString(),
                     id: data.data.id,
                     inviterId: meId,
                     inviteeId: userId,
                  };

                  return draft;
               });
            }
         );
      },
      onSettled: (res) => console.log(res),
   });
};
