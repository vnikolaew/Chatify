import { friendshipsClient } from "../../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

const getSentInvitations = async () => {
   const { status, data } = await friendshipsClient.get(`sent`, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetSentInvitationsQuery = () => {
   const client = useQueryClient();

   return useQuery({
      queryKey: [`friend-invites-sent`],
      queryFn: () => getSentInvitations(),
      cacheTime: 60 * 60 * 1000,
   });
};
