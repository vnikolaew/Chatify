import { authClient } from "../../client";
import {
   useQuery,
   useQueryClient,
   UseQueryOptions,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
// @ts-ignore
import { ObjectApiResponse } from "@openapi/index";

export interface GetMyClaimsResponse {
   claims: Record<string, any>;
}

export const getMyClaims = async (): Promise<GetMyClaimsResponse> => {
   const { status, data } = await authClient.get<ObjectApiResponse>(`/me`);

   if (status !== HttpStatusCode.Ok) {
      throw new Error("error");
   }

   return data.data;
};

export const useGetMyClaimsQuery = (
   options?: Omit<
      UseQueryOptions<
         GetMyClaimsResponse,
         unknown,
         GetMyClaimsResponse,
         string[]
      >,
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
