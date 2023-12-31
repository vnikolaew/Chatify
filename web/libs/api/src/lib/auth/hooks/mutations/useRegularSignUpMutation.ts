import { authClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface RegularSignUpModel {
   email: string;
   username: string;
   password: string;
   acceptTermsAndConditions: boolean;
}
const regularSignUp = async (model: RegularSignUpModel) => {
   const { status, data, headers } = await authClient.post(`/signup`, model);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useRegularSignUpMutation = () => {
   const client = useQueryClient();
   return useMutation({
      mutationFn: regularSignUp,
      onError: console.error,
      onSuccess: (data) => console.log("Sign up success: " + data),
   });
};
