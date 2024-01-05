import { friendshipsClient } from "../../client";
import {
   useQuery,
   useQueryClient,
   UseQueryOptions,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "../../../constants";
//@ts-ignore
import { UserListApiResponse, User, UserDetailsEntry } from "@openapi";
import { USER_DETAILS_KEY } from "../../../profile";

const getFriendSuggestions = async (): Promise<User[]> => {
   const { status, data } = await friendshipsClient.get<UserListApiResponse>(
      `suggestions`,
      {
         headers: {},
      }
   );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export const FRIENDS_SUGGESTIONS_KEY = `friend-suggestions`;
type GetMyFriendsResult = Awaited<ReturnType<typeof getFriendSuggestions>>;

export const useGetFriendSuggestions = (
   options?: Omit<
      UseQueryOptions<User[], Error, User[], string[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();

   return useQuery<User[], Error, User[], string[]>({
      queryKey: [FRIENDS_SUGGESTIONS_KEY],
      queryFn: () => getFriendSuggestions(),
      onSuccess: (users) => {
         users.forEach((user) => {
            client.setQueryData<UserDetailsEntry>(
               [USER_DETAILS_KEY, user.id],
               (old: UserDetailsEntry) =>
                  !old
                     ? { user, friendInvitation: null!, friendsRelation: null! }
                     : {
                          ...old,
                          user: { ...old.user, ...user },
                       }
            );
         });
      },
      gcTime: DEFAULT_CACHE_TIME,
      staleTime: DEFAULT_STALE_TIME,
      ...options,
   });
};
