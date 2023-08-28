"use client";
import React, { Fragment, useMemo } from "react";
import Link from "next/link";
import SignOut from "@components/SignOut";
import { useGetChatGroupDetailsQuery, useGetMyClaimsQuery } from "@web/api";
import { useIsUserLoggedIn } from "@hooks";
import {
   Button,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   useDisclosure,
} from "@nextui-org/react";
import { useCurrentChatGroup } from "@hooks";
import ChatGroupTopBar from "@components/chat-group/ChatGroupTopBar";
import { useSearchParams } from "next/navigation";
import CookieConsentBanner from "@components/CookieConsentBanner";

export const revalidate = 0;

function IndexPage(props) {
   const params = useSearchParams();
   const {
      isOpen: isCookieConsentOpen,
      onOpenChange: onCookieConsentOpenChange,
   } = useDisclosure({ defaultOpen: true });

   const chatGroupId = useCurrentChatGroup();
   const { isUserLoggedIn } = useIsUserLoggedIn();
   const {
      data: chatGroupDetails,
      error,
      isLoading,
   } = useGetChatGroupDetailsQuery(chatGroupId, {
      enabled: !!chatGroupId && isUserLoggedIn,
   });
   const { data: me, error: meError } = useGetMyClaimsQuery({
      enabled: isUserLoggedIn,
   });

   console.log(chatGroupId);
   console.log(me?.claims?.nameidentifier);
   console.log(chatGroupDetails);

   return (
      <div
         className={`flex min-h-[60vh] text-xl gap-3 flex-col items-center shadow-gray-300 w-full rounded-md mb-6`}
      >
         <ChatGroupTopBar />
         <h1 className={`text-3xl my-4`}>
            {chatGroupDetails?.chatGroup?.name}
         </h1>
         <CookieConsentBanner
            isOpen={isCookieConsentOpen}
            onOpenChange={onCookieConsentOpenChange}
         />
         {!isUserLoggedIn ? (
            <Fragment>
               <h2 className={`font-bold`}>You are currently not logged in.</h2>
               <div className={`flex items-center gap-8`}>
                  <Link
                     className={`hover:underline text-blue-600`}
                     href={`/signup`}
                  >
                     {" "}
                     Sign Up
                  </Link>
                  <Link
                     className={`hover:underline text-blue-600`}
                     href={`/signin`}
                  >
                     Sign In
                  </Link>
               </div>
            </Fragment>
         ) : (
            <div className={`self-center mt-4`}>
               <SignOut />
            </div>
         )}
      </div>
   );
}

export default IndexPage;
