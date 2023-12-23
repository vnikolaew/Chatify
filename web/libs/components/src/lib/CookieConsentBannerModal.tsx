'use server'
import React from "react";
import { cookies } from "next/headers";
import { CookieConsentBannerWrapper } from "./CookieConsentBanner";

export interface CookieConsentBannerWrapperProps {}

export const CookieConsentBannerModal = ({}: CookieConsentBannerWrapperProps) => {
   const isConsentNeeded = !cookies().get("Cookie-Consent");
   return isConsentNeeded && <CookieConsentBannerWrapper />;
};
