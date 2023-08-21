import { authClient } from "../client";
import {
   useMutation,
   UseMutationOptions,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

const signOut = async () => {
   const { status, data } = await authClient.post(`/signout`);

   if (status !== HttpStatusCode.NoContent) {
      throw new Error("error");
   }

   return data;
};
export const useSignOutMutation = (
   options?: Omit<
      UseMutationOptions<any, unknown, any, string[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();

   return useMutation(signOut, {
      onError: console.error,
      onSuccess: (data) => {
         console.log("Sign out success: " + data);
         client.clear();
      },
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
      ...options,
   });
};
