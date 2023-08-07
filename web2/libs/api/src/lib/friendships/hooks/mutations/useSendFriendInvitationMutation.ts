import { friendshipsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

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

   return useMutation(sendFriendInvite, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("Friend invite sent successfully: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
