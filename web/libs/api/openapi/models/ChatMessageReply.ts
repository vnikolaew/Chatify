/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ChatGroup } from './ChatGroup';
import type { Media } from './Media';
import type { User } from './User';

export type ChatMessageReply = {
    id?: string;
    chatGroupId?: string;
    chatGroup?: ChatGroup;
    userId?: string;
    user?: User;
    content?: string | null;
    attachments?: Array<Media> | null;
    reactionCounts?: Record<string, number> | null;
    createdAt?: string;
    updatedAt?: string;
    metadata?: Record<string, string> | null;
    replyToId?: string;
};
