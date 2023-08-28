/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { ChatGroupFeedEntry } from './ChatGroupFeedEntry';

export type ChatGroupFeedEntryListApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: Array<ChatGroupFeedEntry> | null;
    timestamp?: string;
};
