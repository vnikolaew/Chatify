import { profileClient } from "../../client";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { useGetMyClaimsQuery } from "../../../auth";
import { useGetUserDetailsQuery, USER_DETAILS_KEY } from "../queries";
import { UserDetailsEntry, UserStatus } from "../../../../../openapi";

export interface ChangeUserStatusModel {
   newStatus: string;
}

const changeUserStatus = async (model: ChangeUserStatusModel) => {
   const { status, data } = await profileClient.put(`status`, model, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useChangeUserStatusMutation = () => {
   const client = useQueryClient();
   const { data: userId } = useGetMyClaimsQuery({
      networkMode: "offlineFirst",
      select: (data) => data.claims["nameidentifier"],
   });
   const { data: userDetails } = useGetUserDetailsQuery(
      (userId as unknown as string)!,
      { networkMode: "offlineFirst" }
   );

   console.log("User Id:", userId);
   console.log(
      "User details:",
      client.getQueryData([USER_DETAILS_KEY, userId])
   );

   return useMutation(changeUserStatus, {
      onError: console.error,
      onSuccess: (data, { newStatus }) => {
         console.log("User status changed successfully: " + data);
         console.log("Updating User status to " + newStatus);

         client.setQueryData<UserDetailsEntry>(
            [USER_DETAILS_KEY, userId],
            (current: UserDetailsEntry | undefined) => ({
               ...current,
               user: {
                  ...current?.user,
                  status: newStatus as UserStatus,
               },
            })
         );
      },
      onSettled: (res) => console.log(res),
      // cacheTime: 60 * 60 * 1000,
   });
};
