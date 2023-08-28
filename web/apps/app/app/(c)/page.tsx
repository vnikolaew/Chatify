"use client";
import React, { Fragment } from "react";
import Link from "next/link";
import SignOut from "@components/SignOut";
import {
   getMediaUrl,
   useGetChatGroupDetailsQuery,
   useGetMyClaimsQuery,
} from "@web/api";
import { useIsUserLoggedIn } from "@hooks";
import {
   Avatar,
   Button,
   Image,
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
import CrossIcon from "@components/icons/CrossIcon";
import useCookie from "react-use-cookie";

export const revalidate = 0;

function IndexPage(props) {
   const params = useSearchParams();
   const isNew = params.get("new") === "true";

   const {
      isOpen: isCookieConsentOpen,
      onOpenChange: onCookieConsentOpenChange,
   } = useDisclosure({ defaultOpen: true });
   const isConsentNeeded = !useCookie("Cookie-Consent", null!)[0];

   const {
      isOpen: isSuccessModalOpen,
      onOpenChange: onSuccessModalOpenChange,
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

   console.log(chatGroupDetails);

   return (
      <div
         className={`flex min-h-[60vh] text-xl gap-3 flex-col items-center shadow-gray-300 w-full rounded-md mb-6`}
      >
         <ChatGroupTopBar />
         <h1 className={`text-3xl my-4`}>
            {chatGroupDetails?.chatGroup?.name}
         </h1>
         {isNew && chatGroupDetails && (
            <Modal
               backdrop={"transparent"}
               isOpen={isSuccessModalOpen}
               onOpenChange={onSuccessModalOpenChange}
               classNames={{
                  base: "absolute text-xs sm:my-0 my-0  bg-success-300 opacity-80 bottom-10 right-10 w-fit min-w-fit max-w-fit mx-auto",
               }}
               shadow={"md"}
               closeButton={
                  <Button
                     size={"sm"}
                     radius={"full"}
                     variant={"light"}
                     isIconOnly
                     className={`top-2 bg-transparent p-0 right-2`}
                     // color={"default"}
                  >
                     <CrossIcon
                        className={`fill-transparent`}
                        fill={`white`}
                        size={8}
                     />
                  </Button>
               }
               radius={"sm"}
               motionProps={{
                  variants: {
                     enter: {
                        y: 0,
                        opacity: 1,
                        transition: {
                           duration: 0.3,
                           ease: "easeOut",
                        },
                     },
                     exit: {
                        y: 20,
                        opacity: 0,
                        transition: {
                           duration: 0.2,
                        },
                     },
                  },
               }}
               size={"md"}
               placement={"bottom"}
            >
               <ModalContent>
                  <ModalHeader className={`pb-0`}>Success!</ModalHeader>
                  <ModalBody className={`pb-4`}>
                     <p>
                        You successfully created{" "}
                        <strong className={`text-small mr-1`}>
                           {chatGroupDetails?.chatGroup?.name}
                        </strong>
                        chat group.
                     </p>
                  </ModalBody>
               </ModalContent>
            </Modal>
         )}
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
