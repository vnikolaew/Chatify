/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Media } from './Media';
import type { PinnedMessage } from './PinnedMessage';
import type { User } from './User';

export type ChatGroup = {
    id?: string;
    creatorId?: string;
    name?: string | null;
    about?: string | null;
    picture?: Media;
    adminIds?: Array<string> | null;
    admins?: Array<User> | null;
    pinnedMessages?: Array<PinnedMessage> | null;
    metadata?: Record<string, string> | null;
    updatedAt?: string | null;
    createdAt?: string;
};
