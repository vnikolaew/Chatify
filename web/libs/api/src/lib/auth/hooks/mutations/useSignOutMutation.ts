import { authClient } from "../../client";
import {
   useMutation,
   UseMutationOptions,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { IS_MOBILE } from "../../../common/createClient";
import AsyncStorage from "@react-native-async-storage/async-storage";

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

   return useMutation({
      mutationFn: signOut,
      onError: console.error,
      onSuccess: async (data) => {
         console.log("Sign out success: " + data);
         if (IS_MOBILE) await AsyncStorage.removeItem(`auth_cookie`);

         client.clear();
      },
      onSettled: (res) => console.log(res),
      ...options,
   });
};
