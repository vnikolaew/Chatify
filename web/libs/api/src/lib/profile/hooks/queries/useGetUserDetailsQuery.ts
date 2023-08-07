import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { profileClient } from "../../client";

export interface GetUserDetailsModel {
   userId: string;
}

const getUserDetails = async (model: GetUserDetailsModel) => {
   const { status, data } = await profileClient.get(`${model.userId}/details`, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetUserDetailsQuery = (userId: string) => {
   const client = useQueryClient();
   return useQuery({
      queryKey: ["user-details", userId],
      queryFn: ({ queryKey: [_, userId] }) => getUserDetails({ userId }),
      cacheTime: 60 * 60 * 1000,
   });
};
