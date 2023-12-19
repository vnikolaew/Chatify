import { friendshipsClient } from "../../client";
import { useQuery, useQueryClient, UseQueryOptions } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "../../../constants";
//@ts-ignore
import { UserListApiResponse, User, ChatGroupMember } from "@openapi";

const getFriendSuggestions = async (): Promise<User[]> => {
   const { status, data } = await friendshipsClient.get<UserListApiResponse>(
      `suggestions`,
      {
         headers: {},
      },
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export const FRIENDS_SUGGESTIONS_KEY = `friend-suggestions`;
type GetMyFriendsResult = Awaited<ReturnType<typeof getFriendSuggestions>>;

export const useGetFriendSuggestions = (
   options?: Omit<UseQueryOptions<ChatGroupMember[], Error, ChatGroupMember[], string[]>, "initialData"> & { initialData?: (() => undefined) | undefined }) => {
   const client = useQueryClient();

   return useQuery<ChatGroupMember[], Error, ChatGroupMember[], string[]>({
      queryKey: [FRIENDS_SUGGESTIONS_KEY],
      queryFn: () => getFriendSuggestions(),
      cacheTime: DEFAULT_CACHE_TIME,
      staleTime: DEFAULT_STALE_TIME,
      ...options,
   });
};
