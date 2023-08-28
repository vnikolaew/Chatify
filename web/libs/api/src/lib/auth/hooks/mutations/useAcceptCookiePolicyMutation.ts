import { authClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

const acceptCookiePolicy = async () => {
   const { status, data } = await authClient.post(`/cookie-policy`);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useAcceptCookiePolicyMutation = () => {
   const client = useQueryClient();

   return useMutation(acceptCookiePolicy, {
      onError: console.error,
      onSuccess: (data) => console.log("Accepted cookie policy: " + data),
      onSettled: (res) => console.log(res),
      cacheTime: 60 * 60 * 1000,
   });
};
