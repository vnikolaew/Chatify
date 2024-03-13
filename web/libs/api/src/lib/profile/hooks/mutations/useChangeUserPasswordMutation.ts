import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { profileClient } from "../../client";

export interface ChangeUserPasswordModel {
   oldPassword: string;
   newPassword: string;
}

const changeUserPassword = async (model: ChangeUserPasswordModel) => {
   const { status, data } = await profileClient.put(`password`, model, {
      headers: {},
   });

   if (status !== HttpStatusCode.Accepted) {
      throw new Error("error");
   }

   return data;
};

export const useChangeUserPasswordMutation = () => {
   const client = useQueryClient();
   return useMutation<any, Error, ChangeUserPasswordModel, any>({
      mutationFn: changeUserPassword,
      onError: console.error,
      onSuccess: (data) => {
         console.log("User password changed successfully: " + data);
      },
      onSettled: (res) => console.log(res),
   });
};
