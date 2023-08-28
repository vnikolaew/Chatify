/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { FriendInvitation } from './FriendInvitation';

export type FriendInvitationListApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: Array<FriendInvitation> | null;
    timestamp?: string;
};
