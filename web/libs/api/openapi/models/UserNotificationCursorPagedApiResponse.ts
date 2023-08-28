/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { UserNotification } from './UserNotification';

export type UserNotificationCursorPagedApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: Array<UserNotification> | null;
    timestamp?: string;
};
