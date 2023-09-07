import { friendshipsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { USER_DETAILS_KEY } from "../../../profile";
import {
   FriendInvitationStatus,
   ObjectApiResponse,
   UserDetailsEntry,
} from "@openapi";
import { produce } from "immer";
import { GetMyClaimsResponse } from "../../../auth";

export interface AcceptFriendInvitationModel {
   inviteId: string;
   userId: string;
}

const acceptFriendInvite = async (model: AcceptFriendInvitationModel) => {
   const { status, data } = await friendshipsClient.post<ObjectApiResponse>(
      `accept/${model.inviteId}`,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data!.data;
};

export const useAcceptFriendInviteMutation = () => {
   const client = useQueryClient();

   return useMutation<any, Error, AcceptFriendInvitationModel, any>(
      acceptFriendInvite,
      {
         onError: console.error,
         onSuccess: ({ id }: { id: string }, { inviteId, userId }, context) => {
            client.setQueryData<UserDetailsEntry>(
               [USER_DETAILS_KEY, userId],
               (old: UserDetailsEntry) => {
                  return produce(old, (draft: UserDetailsEntry) => {
                     draft.friendInvitation.status =
                        FriendInvitationStatus.ACCEPTED;

                     const meId = client.getQueryData<GetMyClaimsResponse>([
                        `me`,
                        `claims`,
                     ])!.claims["nameidentifier"];

                     draft.friendsRelation = {
                        createdAt: new Date().toISOString(),
                        id,
                        friendOneId: meId,
                        friendTwoId: userId,
                     };
                  });
               }
            );
         },
         onSettled: (res) => console.log(res),
         cacheTime: 60 * 60 * 1000,
      }
   );
};
