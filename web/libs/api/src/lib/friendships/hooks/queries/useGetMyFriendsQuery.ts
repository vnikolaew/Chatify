import { friendshipsClient } from "../../client";
import { useQuery, useQueryClient, UseQueryOptions } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "../../../constants";
//@ts-ignore
import { UserListApiResponse, User } from "@openapi";

const getMyFriends = async (): Promise<User[]> => {
   const { status, data } = await friendshipsClient.get<UserListApiResponse>(
      ``,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export const FRIENDS_KEY = `friends`;
type GetMyFriendsResult = Awaited<ReturnType<typeof getMyFriends>>;

export const useGetMyFriendsQuery = (options?: Omit<UseQueryOptions<User[], Error, User[], string[]>, "initialData"> & {initialData?: (() => undefined) | undefined}) => {
   const client = useQueryClient();
   // Get current User Id somehow:

   return useQuery<User[], Error, User[], string[]>({
      queryKey: [FRIENDS_KEY],
      queryFn: () => getMyFriends(),
      cacheTime: DEFAULT_CACHE_TIME,
      staleTime: DEFAULT_STALE_TIME,
      ...options
   });
};
