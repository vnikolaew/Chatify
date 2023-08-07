import { authClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface RegularSignUpModel {
   email: string;
   username: string;
   password: string;
}
const regularSignUp = async (model: RegularSignUpModel) => {
   const { status, data } = await authClient.post(`/signup`, model);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useRegularSignUpMutation = () => {
   const client = useQueryClient();
   return useMutation(regularSignUp, {
      onError: console.error,
      onSuccess: (data) => console.log("Sign up success: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
