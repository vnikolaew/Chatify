/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { ChatGroupDetailsEntry } from './ChatGroupDetailsEntry';

export type ChatGroupDetailsEntryApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: ChatGroupDetailsEntry;
    timestamp?: string;
};
