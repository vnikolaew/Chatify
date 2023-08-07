import { friendshipsClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface UnfriendUserModel {
   userId: string;
}

const unfriendUser = async (model: UnfriendUserModel) => {
   const { status, data } = await friendshipsClient.delete(`${model.userId}`, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useUnfriendUserMutation = () => {
   const client = useQueryClient();

   return useMutation(unfriendUser, {
      onError: console.error,
      onSuccess: (data) => console.log("User unfriended successfully: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
