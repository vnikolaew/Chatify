import { authClient } from "../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface GoogleSignUpModel {
   accessToken: string;
}
const googleSignUp = async (model: GoogleSignUpModel) => {
   const { status, data } = await authClient.post(`/signup/google`, model);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGoogleSignUpMutation = () => {
   const client = useQueryClient();
   return useMutation(googleSignUp, {
      onError: console.error,
      onSuccess: (data) => console.log("Sign up success: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
