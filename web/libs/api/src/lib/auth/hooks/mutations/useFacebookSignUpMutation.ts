import { authClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface FacebookSignUpModel {
   accessToken: string;
}

const facebookSignUp = async (model: FacebookSignUpModel) => {
   const { status, data } = await authClient.post(`/signup/facebook`, model);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useFacebookSignUpMutation = () => {
   const client = useQueryClient();
   return useMutation({
      mutationFn: facebookSignUp,
      onError: console.error,
      onSuccess: (data) => console.log("Sign up success: " + data),
      onSettled: (res) => console.log(res),
   });
};
