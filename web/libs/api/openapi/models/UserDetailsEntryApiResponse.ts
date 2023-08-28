/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { UserDetailsEntry } from './UserDetailsEntry';

export type UserDetailsEntryApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: UserDetailsEntry;
    timestamp?: string;
};
