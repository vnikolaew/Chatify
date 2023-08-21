import { authClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface RegularSignInModel {
   email: string;
   password: string;
   rememberMe: boolean;
}
const regularSignIn = async (model: RegularSignInModel) => {
   const { status, data } = await authClient.post(`/signin`, model);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useRegularSignInMutation = () => {
   const client = useQueryClient();
   return useMutation(regularSignIn, {
      onError: console.error,
      onSuccess: (data) => console.log("Sign in success: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
