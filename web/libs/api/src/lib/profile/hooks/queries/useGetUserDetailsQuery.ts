import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { profileClient } from "../../client";
import { UserDetailsEntry, UserDetailsEntryApiResponse } from "@openapi";
import { sleep } from "../../../utils";

// @ts-ignore

export interface GetUserDetailsModel {
   userId: string;
}

export const getUserDetails = async (
   model: GetUserDetailsModel
): Promise<UserDetailsEntry> => {
   const { status, data } =
      await profileClient.get<UserDetailsEntryApiResponse>(
         `${model.userId}/details`,
         {
            headers: {},
         }
      );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }
   await sleep(1000);

   return data.data!;
};

export const USER_DETAILS_KEY = "user-details";

export const useGetUserDetailsQuery = (
   userId: string,
   options?: Omit<
      UseQueryOptions<UserDetailsEntry, Error, UserDetailsEntry, string[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();
   return useQuery<UserDetailsEntry, Error, UserDetailsEntry, string[]>({
      queryKey: [USER_DETAILS_KEY, userId],
      queryFn: ({ queryKey: [_, userId] }) => getUserDetails({ userId }),
      cacheTime: 60 * 60 * 1000,
      staleTime: 60 * 60 * 1000, // 1 HOUR
      ...options,
   });
};
