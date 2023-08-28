/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { ChatMessageReply } from './ChatMessageReply';

export type ChatMessageReplyCursorPagedApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: Array<ChatMessageReply> | null;
    timestamp?: string;
};
