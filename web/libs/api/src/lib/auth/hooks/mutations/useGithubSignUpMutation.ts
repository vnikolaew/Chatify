import { authClient } from "../../client";
import {
   useMutation,
   UseMutationOptions,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface GithubSignUpModel {
   code: string;
   returnUrl?: string;
}

const githubSignUp = async ({ code, returnUrl }: GithubSignUpModel) => {
   const params = new URLSearchParams(
      returnUrl ? { returnUrl, code } : { code }
   )!;

   const { status, data } = await authClient.post(
      `/signup/github`,
      {},
      {
         params,
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error(data.error ?? "Error");
   }

   return data;
};

export const useGithubSignUpMutation = (
   options?:
      | Omit<
           UseMutationOptions<any, unknown, GithubSignUpModel, unknown>,
           "mutationFn"
        >
      | undefined
) => {
   const client = useQueryClient();

   return useMutation({
      mutationFn: githubSignUp,
      onError: (err) => console.error("A sign up error occurred: ", err),
      onSuccess: (data) => console.log("Sign up success: " + data),
      onSettled: (res) => console.log(res),
      ...options,
   });
};
