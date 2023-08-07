import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { profileClient } from "../../client";

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

export const useSearchUsersByNameQuery = (usernameQuery: string) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: ["users-search-by-name", usernameQuery],
      queryFn: ({ queryKey: [_, usernameQuery] }) =>
         searchUsersByName({ usernameQuery }),
      cacheTime: 60 * 60 * 1000,
   });
};
