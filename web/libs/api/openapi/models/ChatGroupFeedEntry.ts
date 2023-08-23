/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ChatGroup } from './ChatGroup';
import type { ChatMessage } from './ChatMessage';
import type { User } from './User';

export type ChatGroupFeedEntry = {
    chatGroup?: ChatGroup;
    latestMessage?: ChatMessage;
    messageSender?: User;
};
