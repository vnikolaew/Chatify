import { friendshipsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface DeclineFriendInvitationModel {
   inviteId: string;
}

const declineFriendInvite = async (model: DeclineFriendInvitationModel) => {
   const { status, data } = await friendshipsClient.post(
      `decline/${model.inviteId}`,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useDeclineFriendInviteMutation = () => {
   const client = useQueryClient();

   return useMutation<any, Error, DeclineFriendInvitationModel, any>(
      declineFriendInvite,
      {
         onError: console.error,
         onSuccess: (data) =>
            console.log("Friend invite declined successfully: " + data),
         onSettled: (res) => console.log(res),
         cacheTime: 60 * 60 * 1000,
      }
   );
};
