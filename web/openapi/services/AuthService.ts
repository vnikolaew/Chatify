/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { FacebookSignUp } from '../models/FacebookSignUp';
import type { GoogleSignUp } from '../models/GoogleSignUp';
import type { RegularSignIn } from '../models/RegularSignIn';
import type { RegularSignUp } from '../models/RegularSignUp';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class AuthService {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getApiAuthMe(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Auth/me',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiAuthSignup(
requestBody?: RegularSignUp,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Auth/signup',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiAuthSignin(
requestBody?: RegularSignIn,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Auth/signin',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiAuthSignupGoogle(
requestBody?: GoogleSignUp,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Auth/signup/google',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param requestBody 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiAuthSignupFacebook(
requestBody?: FacebookSignUp,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Auth/signup/facebook',
            body: requestBody,
            mediaType: 'application/json',
        });
    }

    /**
     * @param token 
     * @returns any Success
     * @throws ApiError
     */
    public static postApiAuthConfirmEmail(
token?: string,
): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Auth/confirm-email',
            query: {
                'token': token,
            },
        });
    }

}
