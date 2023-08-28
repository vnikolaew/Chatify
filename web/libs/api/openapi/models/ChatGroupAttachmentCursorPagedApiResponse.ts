/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { ChatGroupAttachment } from './ChatGroupAttachment';

export type ChatGroupAttachmentCursorPagedApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: Array<ChatGroupAttachment> | null;
    timestamp?: string;
};
