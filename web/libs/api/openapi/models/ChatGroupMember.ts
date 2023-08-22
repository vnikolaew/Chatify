/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ChatGroup } from './ChatGroup';
import type { User } from './User';

export type ChatGroupMember = {
    id?: string;
    userId?: string;
    username?: string | null;
    user?: User;
    chatGroupId?: string;
    chatGroup?: ChatGroup;
    createdAt?: string;
    membershipType?: number;
};
