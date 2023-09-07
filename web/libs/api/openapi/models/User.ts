/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Email } from './Email';
import type { IPAddress } from './IPAddress';
import type { Media } from './Media';
import type { PhoneNumber } from './PhoneNumber';
import type { UserStatus } from './UserStatus';

export type User = {
    id?: string;
    username?: string | null;
    email?: Email;
    status?: UserStatus;
    roles?: Array<string> | null;
    phoneNumbers?: Array<PhoneNumber> | null;
    profilePicture?: Media;
    createdAt?: string;
    updatedAt?: string | null;
    lastLogin?: string;
    deviceIps?: Array<IPAddress> | null;
    metadata?: Record<string, string> | null;
    displayName?: string | null;
    userHandle?: string | null;
};
