import { authClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface RegularSignInModel {
   email: string;
   password: string;
   rememberMe: boolean;
   returnUrl?: string;
}

const regularSignIn = async (model: RegularSignInModel) => {
   const params = new URLSearchParams(
      model.returnUrl ? { returnUrl: model.returnUrl } : {}
   )!;

   const { status, data } = await authClient.post(`/signin`, model, {
      params,
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useRegularSignInMutation = () => {
   const client = useQueryClient();
   return useMutation<any, Error, RegularSignInModel, any>(regularSignIn, {
      onError: console.error,
      onSuccess: (data) => console.log("Sign in success: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
