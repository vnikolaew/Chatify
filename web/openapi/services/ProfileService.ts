/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ChangeUserStatus } from '../models/ChangeUserStatus';
import type { EditUserDetails } from '../models/EditUserDetails';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ProfileService {

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static putApiProfileStatus(
requestBody?: ChangeUserStatus,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Profile/status',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static patchApiProfileDetails(
requestBody?: EditUserDetails,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PATCH',
            url: '/api/Profile/details',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param usernameQuery 
     * @returns any Success
     * @throws ApiError
     */
    public static getApiProfileDetails(
usernameQuery?: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Profile/details',
            query: {
                'usernameQuery': usernameQuery,
            },
        });
    }

    /**
     * @param userId 
     * @returns any Success
     * @throws ApiError
     */
    public static getApiProfileDetails1(
userId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Profile/{userId}/details',
            path: {
                'userId': userId,
            },
        });
    }

}
