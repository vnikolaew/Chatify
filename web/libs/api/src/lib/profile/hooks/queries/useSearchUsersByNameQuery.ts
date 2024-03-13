import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { profileClient } from "../../client";
import { DEFAULT_CACHE_TIME } from "../../../constants";

export interface SearchUsersByNameModel {
   usernameQuery: string;
}

const searchUsersByName = async ({ usernameQuery }: SearchUsersByNameModel) => {
   const params = new URLSearchParams({
      usernameQuery,
   });

   const { status, data } = await profileClient.get(`details`, {
      headers: {},
      params,
   });

   if (status === HttpStatusCode.NotFound) {
      throw new Error("error");
   }

   return data;
};

export const GET_USERS_SEARCH_KEY = (username: string) => [
   "users-search-by-name",
   username,
];

export const useSearchUsersByNameQuery = (usernameQuery: string) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: GET_USERS_SEARCH_KEY(usernameQuery),
      queryFn: ({ queryKey: [_, usernameQuery] }) =>
         searchUsersByName({ usernameQuery }),
      gcTime: DEFAULT_CACHE_TIME,
   });
};
