/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { User } from "./User";
import type { UserNotificationType } from "./UserNotificationType";

export type UserNotification = {
   id?: string;
   userId?: string;
   user?: User;
   createdAt?: string;
   updatedAt?: string | null;
   type?: UserNotificationType;
   metadata?: Record<string, string | null | any> | null;
   summary?: string | null;
   read?: boolean;
};
