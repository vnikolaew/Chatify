/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { ChatGroupMessageEntry } from './ChatGroupMessageEntry';

export type ChatGroupMessageEntryCursorPagedApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: Array<ChatGroupMessageEntry> | null;
    timestamp?: string;
};
