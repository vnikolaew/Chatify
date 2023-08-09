import { authClient } from "../client";
import {
   useQuery,
   useQueryClient,
   UseQueryOptions,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

const getMyClaims = async () => {
   const { status, data } = await authClient.get(`/me`);

   if (status !== HttpStatusCode.Ok) {
      throw new Error("error");
   }

   return data;
};
export const useGetMyClaimsQuery = (
   options?: Omit<
      UseQueryOptions<any, unknown, any, string[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: ["me", "claims"],
      queryFn: getMyClaims,
      // cacheTime: 60 * 60 * 1000,
      ...options,
   });
};
