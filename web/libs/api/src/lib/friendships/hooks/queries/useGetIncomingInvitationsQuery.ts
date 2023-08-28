import { friendshipsClient } from "../../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { FriendInvitationListApiResponse, FriendInvitation } from "@openapi";

const getIncomingInvitations = async (): Promise<FriendInvitation[]> => {
   const { status, data } =
      await friendshipsClient.get<FriendInvitationListApiResponse>(`incoming`, {
         headers: {},
      });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export const useGetIncomingInvitationsQuery = () => {
   const client = useQueryClient();
   return useQuery<FriendInvitation[], Error, FriendInvitation[], any>({
      queryKey: [`friend-invites-incoming`],
      queryFn: () => getIncomingInvitations(),
      cacheTime: 60 * 60 * 1000,
   });
};
