import {
   useQueryClient,
   useMutation,
   UseMutationOptions,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { notificationsClient } from "../../client";
import { UserNotification } from "@openapi/index";
import { NOTIFICATIONS_KEY } from "../queries";

export interface MarkNotificationsAsReadModel {}

const markNotificationsAsRead =
   async ({}: MarkNotificationsAsReadModel): Promise<any> => {
      const { status, data } = await notificationsClient.put(`/read`, {
         headers: {},
      });

      if (status === HttpStatusCode.BadRequest) {
         throw new Error("error");
      }

      return data;
   };

export const useMarkNotificationsAsReadMutation = (
   options?:
      | Omit<
           UseMutationOptions<UserNotification[], Error, any, any>,
           "mutationFn"
        >
      | undefined
) => {
   const client = useQueryClient();
   return useMutation(() => markNotificationsAsRead({}), {
      onError: console.error,
      onSuccess: (data, vars, context) => {
         // Delete all unread notifications from query cache:
         client.setQueryData<UserNotification[]>(
            [NOTIFICATIONS_KEY, `unread`],
            (_) => []
         );
      },
      onSettled: console.log,
      ...options,
   });
};
