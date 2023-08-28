import React from "react";
import { cookies } from "next/headers";
import { CookieConsentBannerWrapper } from "@components/CookieConsentBanner";

export interface CookieConsentBannerWrapperProps {}

const CookieConsentBannerModal = ({}: CookieConsentBannerWrapperProps) => {
   const isConsentNeeded = !cookies().get("Cookie-Consent");
   return isConsentNeeded && <CookieConsentBannerWrapper />;
};

export default CookieConsentBannerModal;
