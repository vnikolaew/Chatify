/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { InputFile } from './InputFile';
import type { NewPasswordInput } from './NewPasswordInput';

export type EditUserDetails = {
    username?: string | null;
    displayName?: string | null;
    profilePicture?: InputFile;
    phoneNumbers?: Array<string> | null;
    newPasswordInput?: NewPasswordInput;
};
