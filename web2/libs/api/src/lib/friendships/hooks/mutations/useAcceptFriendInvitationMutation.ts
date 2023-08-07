import { friendshipsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface AcceptFriendInvitationModel {
   inviteId: string;
}

const acceptFriendInvite = async (model: AcceptFriendInvitationModel) => {
   const { status, data } = await friendshipsClient.post(
      `accept/${model.inviteId}`,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useAcceptFriendInviteMutation = () => {
   const client = useQueryClient();

   return useMutation(acceptFriendInvite, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Friend invite accepted successfully: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
