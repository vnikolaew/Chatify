/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ApiResponseStatus } from './ApiResponseStatus';
import type { Unit } from './Unit';

export type UnitApiResponse = {
    status?: ApiResponseStatus;
    errors?: Array<string> | null;
    message?: string | null;
    data?: Unit;
    timestamp?: string;
};
