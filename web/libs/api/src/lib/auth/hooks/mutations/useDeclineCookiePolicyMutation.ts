import { authClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

const declineCookiePolicy = async () => {
   const { status, data } = await authClient.delete(`/cookie-policy`);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useDeclineCookiePolicyMutation = () => {
   const client = useQueryClient();

   return useMutation({
      mutationFn: declineCookiePolicy,
      onError: console.error,
      onSuccess: (data) => console.log("Declined cookie policy: " + data),
      onSettled: (res) => console.log(res),
   });
};
