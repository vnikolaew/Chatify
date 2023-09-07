import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { profileClient } from "../../client";
// @ts-ignore
import { UserDetailsEntry, UserDetailsEntryApiResponse } from "@openapi";

export interface FindUserByHandleModel {
   userHandle: string;
}

export const findUserByHandleQuery = async ({
   userHandle,
}: FindUserByHandleModel): Promise<UserDetailsEntry> => {
   const { status, data } =
      await profileClient.get<UserDetailsEntryApiResponse>(``, {
         headers: {},
         params: new URLSearchParams({ handle: userHandle }),
      });

   if (status === HttpStatusCode.NotFound) {
      throw new Error("error");
   }

   return data.data!;
};

export const USER_DETAILS_BY_HANDLE_KEY = "user-details-by-handle";

export const useFindUserByHandleQuery = (
   userHandle: string,
   options?: Omit<
      UseQueryOptions<UserDetailsEntry, Error, UserDetailsEntry, string[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();
   return useQuery<UserDetailsEntry, Error, UserDetailsEntry, string[]>({
      queryKey: [USER_DETAILS_BY_HANDLE_KEY, userHandle],
      queryFn: ({ queryKey: [_, handle] }) =>
         findUserByHandleQuery({ userHandle: handle }),
      enabled: false,
      ...options,
   });
};
