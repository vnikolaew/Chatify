/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AddressFamily } from './AddressFamily';

export type IPAddress = {
    addressFamily?: AddressFamily;
    scopeId?: number;
    readonly isIPv6Multicast?: boolean;
    readonly isIPv6LinkLocal?: boolean;
    readonly isIPv6SiteLocal?: boolean;
    readonly isIPv6Teredo?: boolean;
    readonly isIPv6UniqueLocal?: boolean;
    readonly isIPv4MappedToIPv6?: boolean;
    /**
     * @deprecated
     */
    address?: number;
};
