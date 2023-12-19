import {
   useQueryClient,
   useMutation,
   UseMutationOptions, InfiniteData,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { notificationsClient } from "../../client";
import { CursorPaged, UserNotification } from "@openapi";
import { NOTIFICATIONS_KEY } from "../queries";
import { produce } from "immer";

export interface MarkNotificationsAsReadModel {
}

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
      | undefined,
) => {
   const client = useQueryClient();
   return useMutation(() => markNotificationsAsRead({}), {
      onError: console.error,
      onSuccess: () => {
         const unreadNotifications = client
            .getQueryData<UserNotification[]>([NOTIFICATIONS_KEY, `unread`])!;
         const unreadIds = new Set(unreadNotifications.map<string>(n => n.id));

         // Delete all unread notifications from query cache:
         client.setQueryData<UserNotification[]>(
            [NOTIFICATIONS_KEY, `unread`],
            (_) => [],
         );

         client.setQueryData<InfiniteData<CursorPaged<UserNotification>>>([NOTIFICATIONS_KEY], (old) =>
            produce(old, (notifications: InfiniteData<CursorPaged<UserNotification>>) => {
               notifications.pages.forEach(page => {
                  page.items = page.items.filter((un: UserNotification) => !unreadIds.has(un.id));
                  page.pageSize = page.items.length;
               });
               return notifications;
            }));
      },
      onSettled: console.log,
      ...options,
   });
};
