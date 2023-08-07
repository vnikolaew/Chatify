import { friendshipsClient } from "../../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

const getIncomingInvitations = async () => {
   const { status, data } = await friendshipsClient.get(`incoming`, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetIncomingInvitationsQuery = () => {
   const client = useQueryClient();
   return useQuery({
      queryKey: [`friend-invites-incoming`],
      queryFn: () => getIncomingInvitations(),
      cacheTime: 60 * 60 * 1000,
   });
};
