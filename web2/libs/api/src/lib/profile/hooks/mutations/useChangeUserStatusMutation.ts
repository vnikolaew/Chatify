import { profileClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface ChangeUserStatusModel {
   newStatus: number;
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

   return useMutation(changeUserStatus, {
      onError: console.error,
      onSuccess: (data) =>
         console.log("User status changed successfully: " + data),
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
