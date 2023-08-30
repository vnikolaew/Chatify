/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ChatGroup } from './ChatGroup';
import type { ChatMessage } from './ChatMessage';
import type { User } from './User';

export type ChatMessageReaction = {
    id?: string;
    messageId?: string;
    message?: ChatMessage;
    chatGroupId?: string;
    chatGroup?: ChatGroup;
    userId?: string;
    username?: string | null;
    user?: User;
    reactionCode?: number;
    createdAt?: string;
    updatedAt?: string;
};
