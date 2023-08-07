/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ReactToChatMessageRequest } from '../models/ReactToChatMessageRequest';
import type { UnreactToChatMessageRequest } from '../models/UnreactToChatMessageRequest';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ReactionsService {

    /**
     * @param messageId 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiReactions(
messageId: string,
requestBody?: ReactToChatMessageRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Reactions/{messageId}',
            path: {
                'messageId': messageId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param messageId 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiReactionsReplies(
messageId: string,
requestBody?: ReactToChatMessageRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Reactions/replies/{messageId}',
            path: {
                'messageId': messageId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param messageReactionId 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static deleteApiReactions(
messageReactionId: string,
requestBody?: UnreactToChatMessageRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Reactions/{messageReactionId}',
            path: {
                'messageReactionId': messageReactionId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param messageReactionId 
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static deleteApiReactionsReplies(
messageReactionId: string,
requestBody?: UnreactToChatMessageRequest,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Reactions/replies/{messageReactionId}',
            path: {
                'messageReactionId': messageReactionId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }

}
