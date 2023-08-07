/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class FriendshipsService {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getApiFriendships(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Friendships',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getApiFriendshipsSent(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Friendships/sent',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getApiFriendshipsIncoming(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Friendships/incoming',
        });
    }

    /**
     * @param userId 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiFriendshipsInvite(
userId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Friendships/invite/{userId}',
            path: {
                'userId': userId,
            },
        });
    }

    /**
     * @param inviteId 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiFriendshipsAccept(
inviteId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Friendships/accept/{inviteId}',
            path: {
                'inviteId': inviteId,
            },
        });
    }

    /**
     * @param inviteId 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiFriendshipsDecline(
inviteId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Friendships/decline/{inviteId}',
            path: {
                'inviteId': inviteId,
            },
        });
    }

    /**
     * @param friendId 
     * @returns any Success
     * @throws ApiError
     */
    public static deleteApiFriendships(
friendId: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Friendships/{friendId}',
            path: {
                'friendId': friendId,
            },
        });
    }

}
