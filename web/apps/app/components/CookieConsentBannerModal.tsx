import React from "react";
import { cookies } from "next/headers";
import { CookieConsentBannerWrapper } from "@components/CookieConsentBanner";

export interface CookieConsentBannerWrapperProps {}

const CookieConsentBannerModal = ({}: CookieConsentBannerWrapperProps) => {
   const isConsentNeeded = !cookies().get("Cookie-Consent");
   console.log(`Is consent needed: `, isConsentNeeded);
   return isConsentNeeded && <CookieConsentBannerWrapper />;
};

export default CookieConsentBannerModal;
