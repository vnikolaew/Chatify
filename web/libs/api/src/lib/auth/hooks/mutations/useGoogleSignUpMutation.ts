import { authClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface GoogleSignUpModel {
   accessToken: string;
   returnUrl?: string;
}

const googleSignUp = async ({ returnUrl, accessToken }: GoogleSignUpModel) => {
   const params = new URLSearchParams(returnUrl ? { returnUrl } : {})!;

   const { status, data } = await authClient.post(
      `/signup/google`,
      { accessToken },
      {
         params,
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error(data.error ?? "Error");
   }

   return data;
};
export const useGoogleSignUpMutation = () => {
   const client = useQueryClient();
   return useMutation({
      mutationFn: googleSignUp,
      onError: (err) => console.error("A sign up error occurred: ", err),
      onSuccess: (data) => console.log("Sign up success: " + data),
      onSettled: (res) => console.log(res),
   });
};
