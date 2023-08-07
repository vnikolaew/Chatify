/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeleteGroupChatMessageRequest } from '../models/DeleteGroupChatMessageRequest';
import type { EditGroupChatMessageRequest } from '../models/EditGroupChatMessageRequest';
import type { GetMessagesByChatGroupRequest } from '../models/GetMessagesByChatGroupRequest';
import type { GetRepliesByForMessageRequest } from '../models/GetRepliesByForMessageRequest';
import type { SendGroupChatMessageReplyRequest } from '../models/SendGroupChatMessageReplyRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MessagesService {

    /**
     * @param groupId 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static getApiMessages(
groupId: string,
requestBody?: GetMessagesByChatGroupRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Messages/{groupId}',
            path: {
                'groupId': groupId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param groupId 
     * @param formData 
     * @returns any Accepted
     * @throws ApiError
     */
    public static postApiMessages(
groupId: string,
formData?: {
ChatGroupId?: string;
Content?: string;
Files?: Array<Blob>;
Metadata?: Record<string, string>;
},
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Messages/{groupId}',
            path: {
                'groupId': groupId,
            },
            formData: formData,
            mediaType: 'multipart/form-data',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param messageId 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static getApiMessagesReplies(
messageId: string,
requestBody?: GetRepliesByForMessageRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Messages/replies/{messageId}',
            path: {
                'messageId': messageId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param messageId 
     * @param requestBody 
     * @returns void 
     * @throws ApiError
     */
    public static deleteApiMessagesReplies(
messageId: string,
requestBody?: DeleteGroupChatMessageRequest,
): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Messages/replies/{messageId}',
            path: {
                'messageId': messageId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param messageId 
     * @param requestBody 
     * @returns any Accepted
     * @throws ApiError
     */
    public static postApiMessagesReplies(
messageId: string,
requestBody?: SendGroupChatMessageReplyRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Messages/replies/{messageId}',
            path: {
                'messageId': messageId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param messageId 
     * @param requestBody 
     * @returns any Accepted
     * @throws ApiError
     */
    public static putApiMessagesReplies(
messageId: string,
requestBody?: EditGroupChatMessageRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Messages/replies/{messageId}',
            path: {
                'messageId': messageId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param messageId 
     * @param requestBody 
     * @returns any Accepted
     * @throws ApiError
     */
    public static putApiMessages(
messageId: string,
requestBody?: EditGroupChatMessageRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Messages/{messageId}',
            path: {
                'messageId': messageId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @param messageId 
     * @param requestBody 
     * @returns void 
     * @throws ApiError
     */
    public static deleteApiMessages(
messageId: string,
requestBody?: DeleteGroupChatMessageRequest,
): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Messages/{messageId}',
            path: {
                'messageId': messageId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

}
