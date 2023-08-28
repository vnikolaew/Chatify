import { friendshipsClient } from "../../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { FriendInvitationListApiResponse, FriendInvitation } from "@openapi";

const getSentInvitations = async (): Promise<FriendInvitation[]> => {
   const { status, data } =
      await friendshipsClient.get<FriendInvitationListApiResponse>(`sent`, {
         headers: {},
      });

   tifi;
   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export const useGetSentInvitationsQuery = () => {
   const client = useQueryClient();

   return useQuery<FriendInvitation[], Error, FriendInvitation[], any>({
      queryKey: [`friend-invites-sent`],
      queryFn: () => getSentInvitations(),
      cacheTime: 60 * 60 * 1000,
   });
};
