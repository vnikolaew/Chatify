/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AddChatGroupAdmin } from '../models/AddChatGroupAdmin';
import type { AddChatGroupMember } from '../models/AddChatGroupMember';
import type { LeaveChatGroup } from '../models/LeaveChatGroup';
import type { RemoveChatGroupMember } from '../models/RemoveChatGroupMember';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ChatGroupsService {

    /**
     * @param formData 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiChatGroups(
formData?: {
About?: string;
Name?: string;
File?: Blob;
},
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/ChatGroups',
            formData: formData,
            mediaType: 'multipart/form-data',
        });
    }

    /**
     * @param groupId 
     * @returns any Success
     * @throws ApiError
     */
    public static getApiChatGroups(
groupId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/ChatGroups/{groupId}',
            path: {
                'groupId': groupId,
            },
        });
    }

    /**
     * @param groupId 
     * @param formData 
     * @returns any Success
     * @throws ApiError
     */
    public static patchApiChatGroups(
groupId: string,
formData?: {
ChatGroupId?: string;
Name?: string;
About?: string;
File?: Blob;
},
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PATCH',
            url: '/api/ChatGroups/{groupId}',
            path: {
                'groupId': groupId,
            },
            formData: formData,
            mediaType: 'multipart/form-data',
        });
    }

    /**
     * @param limit 
     * @param offset 
     * @returns any Success
     * @throws ApiError
     */
    public static getApiChatGroupsFeed(
limit?: number,
offset?: number,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/ChatGroups/feed',
            query: {
                'limit': limit,
                'offset': offset,
            },
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiChatGroupsMembers(
requestBody?: AddChatGroupMember,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/ChatGroups/members',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static deleteApiChatGroupsMembers(
requestBody?: RemoveChatGroupMember,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/ChatGroups/members',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiChatGroupsLeave(
requestBody?: LeaveChatGroup,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/ChatGroups/leave',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiChatGroupsAdmins(
requestBody?: AddChatGroupAdmin,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/ChatGroups/admins',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
