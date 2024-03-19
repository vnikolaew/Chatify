import { authClient } from "../../client";
import { useMutation } from "@tanstack/react-query";
import { HttpStatusCode, RawAxiosResponseHeaders } from "axios";
import { IS_MOBILE } from "../../../common/createClient";
import AsyncStorage from "@react-native-async-storage/async-storage";

export interface RegularSignInModel {
   email: string;
   password: string;
   rememberMe: boolean;
   returnUrl?: string;
}

export interface RegularSignInResponse {
   data: any;
   headers: RawAxiosResponseHeaders;
}

const regularSignIn = async (
   model: RegularSignInModel
): Promise<RegularSignInResponse> => {
   const params = new URLSearchParams(
      model.returnUrl ? { returnUrl: model.returnUrl } : {}
   )!;

   const { status, data, headers } = await authClient.post(`/signin`, model, {
      params,
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return { data, headers };
};

export const useRegularSignInMutation = () => {
   return useMutation<RegularSignInResponse, Error, RegularSignInModel, any>({
      mutationFn: regularSignIn,
      onError: console.error,
      onSuccess: async ({ headers }) => {
         const authCookieValue = headers["x-auth-token"] as string;

         if (authCookieValue?.length && IS_MOBILE) {
            await AsyncStorage.setItem(`auth_cookie`, authCookieValue);
         }
      },
      onSettled: () => {},
   });
};
