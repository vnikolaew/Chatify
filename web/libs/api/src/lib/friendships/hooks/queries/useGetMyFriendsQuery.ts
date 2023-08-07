import { friendshipsClient } from "../../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

const getMyFriends = async () => {
   const { status, data } = await friendshipsClient.get(``, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetMyFriendsQuery = () => {
   const client = useQueryClient();
   // Get current User Id somehow:

   return useQuery({
      queryKey: [`friends`],
      queryFn: () => getMyFriends(),
      cacheTime: 60 * 60 * 1000,
   });
};
