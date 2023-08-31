/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ChatMessage } from './ChatMessage';
import type { MessageRepliersInfoEntry } from './MessageRepliersInfoEntry';
import type { MessageSenderInfoEntry } from './MessageSenderInfoEntry';
import type { UserMessageReaction } from './UserMessageReaction';

export type ChatGroupMessageEntry = {
    message?: ChatMessage;
    forwardedMessage?: ChatMessage;
    senderInfo?: MessageSenderInfoEntry;
    userReaction?: UserMessageReaction;
    repliersInfo?: MessageRepliersInfoEntry;
};
