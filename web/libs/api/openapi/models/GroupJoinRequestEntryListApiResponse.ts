/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { GroupJoinRequestEntry } from './GroupJoinRequestEntry';

export type GroupJoinRequestEntryListApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: Array<GroupJoinRequestEntry> | null;
    timestamp?: string;
};
